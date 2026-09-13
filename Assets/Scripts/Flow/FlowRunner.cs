using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// How Flow schedules await continuations after completion.
    /// </summary>
    public enum FlowContinuationScheduling
    {
        /// <summary>Always queue to the next player-loop tick (avoids re-entrancy; may cost +1 frame).</summary>
        Post = 0,
        /// <summary>Run inline when already on the main thread; otherwise <see cref="FlowRunner.Post"/>.</summary>
        Run = 1,
    }

    /// <summary>
    /// Pumps continuations and host coroutines on the Unity main thread.
    /// Creation is main-thread only; <see cref="Post"/> is safe from any thread.
    /// Supports <see cref="PlayerLoopTiming"/> queues (Update / FixedUpdate / LateUpdate / EndOfFrame).
    /// </summary>
    public sealed class FlowRunner : MonoBehaviour
    {
        const int TimingCount = 4;

        static FlowRunner s_instance;
        static int s_mainThreadId;
        static bool s_mainThreadCaptured;
        static readonly Queue<Action>[] s_posted =
        {
            new Queue<Action>(64),
            new Queue<Action>(16),
            new Queue<Action>(16),
            new Queue<Action>(16),
        };
        static readonly List<Action> s_execBuffer = new List<Action>(64);
        static readonly object s_gate = new object();
        static bool s_endOfFramePumpStarted;
        static int s_syncDepth;

        /// <summary>
        /// Default scheduling for Flow completion continuations.
        /// <see cref="FlowContinuationScheduling.Run"/> avoids the extra frame when completing on main.
        /// </summary>
        public static FlowContinuationScheduling ContinuationScheduling { get; set; } =
            FlowContinuationScheduling.Post;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            CaptureMainThread();
            Ensure();
        }

        static void CaptureMainThread()
        {
            s_mainThreadId = Thread.CurrentThread.ManagedThreadId;
            s_mainThreadCaptured = true;
        }

        /// <summary>True when the caller is on the Unity main thread.</summary>
        public static bool IsMainThread
        {
            get
            {
                if (!s_mainThreadCaptured)
                {
                    CaptureMainThread();
                }

                return Thread.CurrentThread.ManagedThreadId == s_mainThreadId;
            }
        }

        public static FlowRunner Ensure()
        {
            if (s_instance != null)
            {
                return s_instance;
            }

            if (!IsMainThread)
            {
                throw new InvalidOperationException(
                    "FlowRunner.Ensure() must run on the Unity main thread. " +
                    "await Flow.SwitchToMainThread() first, or wait for RuntimeInitializeOnLoad bootstrap.");
            }

            var go = new GameObject("[FlowRunner]");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            s_instance = go.AddComponent<FlowRunner>();
            return s_instance;
        }

        /// <summary>
        /// Queue work onto the Update pump. Safe from any thread.
        /// Continuations always run on the next timed tick (avoids re-entrancy).
        /// </summary>
        public static void Post(Action action) => Post(action, PlayerLoopTiming.Update);

        /// <summary>Queue work onto a specific player-loop timing. Safe from any thread.</summary>
        public static void Post(Action action, PlayerLoopTiming timing)
        {
            if (action == null)
            {
                return;
            }

            var index = ClampTiming(timing);
            lock (s_gate)
            {
                s_posted[index].Enqueue(action);
            }

            if (s_instance == null && IsMainThread)
            {
                Ensure();
            }

            if (timing == PlayerLoopTiming.EndOfFrame && s_instance != null)
            {
                s_instance.EnsureEndOfFramePump();
            }
        }

        /// <summary>
        /// Run immediately if already on the main thread; otherwise <see cref="Post"/>.
        /// </summary>
        public static void RunOnMainThread(Action action)
        {
            if (action == null)
            {
                return;
            }

            if (IsMainThread)
            {
                InvokeGuarded(action);
            }
            else
            {
                Post(action);
            }
        }

        /// <summary>
        /// Schedule work using <see cref="ContinuationScheduling"/> (or an explicit override).
        /// </summary>
        public static void Schedule(
            Action action,
            FlowContinuationScheduling? scheduling = null,
            PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            if (action == null)
            {
                return;
            }

            var mode = scheduling ?? ContinuationScheduling;
            if (mode == FlowContinuationScheduling.Run && IsMainThread)
            {
                InvokeGuarded(action);
                return;
            }

            Post(action, timing);
        }

        static void InvokeGuarded(Action action)
        {
            s_syncDepth++;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                s_syncDepth--;
            }
        }

        /// <summary>
        /// Start a coroutine on the main thread. Off-thread calls are posted;
        /// the returned handle may be null until the start runs (prefer <see cref="StartRoutineAsFlow"/>).
        /// </summary>
        public static Coroutine StartRoutine(IEnumerator routine)
        {
            if (routine == null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            if (IsMainThread)
            {
                return Ensure().StartCoroutine(routine);
            }

            Post(() => Ensure().StartCoroutine(routine));
            return null;
        }

        /// <summary>
        /// Start a coroutine and return a <see cref="Flow"/> that completes when it finishes
        /// (or when <paramref name="cancellationToken"/> cancels). Safe from any thread.
        /// </summary>
        public static Flow StartRoutineAsFlow(
            IEnumerator routine,
            CancellationToken cancellationToken = default)
        {
            if (routine == null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            var flow = FlowPool.RentVoid();
            flow.AttachCancellation(cancellationToken);
            if (flow.IsCompleted)
            {
                return flow;
            }

            void Start()
            {
                if (flow.IsCompleted)
                {
                    return;
                }

                Ensure().StartCoroutine(WatchRoutine(flow, routine, cancellationToken));
            }

            if (IsMainThread)
            {
                Start();
            }
            else
            {
                Post(Start);
            }

            return flow;
        }

        static IEnumerator WatchRoutine(
            Flow flow,
            IEnumerator routine,
            CancellationToken cancellationToken)
        {
            Exception error = null;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested || flow.IsCompleted)
                {
                    if (!flow.IsCompleted)
                    {
                        flow.TrySetCanceled(cancellationToken);
                    }

                    yield break;
                }

                bool moved;
                try
                {
                    moved = routine.MoveNext();
                }
                catch (Exception ex)
                {
                    error = ex;
                    break;
                }

                if (!moved)
                {
                    break;
                }

                yield return routine.Current;
            }

            if (flow.IsCompleted)
            {
                yield break;
            }

            if (error != null)
            {
                flow.TrySetException(error);
            }
            else
            {
                flow.TrySetResult();
            }
        }

        static int ClampTiming(PlayerLoopTiming timing)
        {
            var index = (int)timing;
            if (index < 0 || index >= TimingCount)
            {
                return (int)PlayerLoopTiming.Update;
            }

            return index;
        }

        void EnsureEndOfFramePump()
        {
            if (s_endOfFramePumpStarted)
            {
                return;
            }

            s_endOfFramePumpStarted = true;
            StartCoroutine(EndOfFramePump());
        }

        IEnumerator EndOfFramePump()
        {
            var wait = new WaitForEndOfFrame();
            while (true)
            {
                yield return wait;
                Drain(PlayerLoopTiming.EndOfFrame);
            }
        }

        void Update() => Drain(PlayerLoopTiming.Update);

        void FixedUpdate() => Drain(PlayerLoopTiming.FixedUpdate);

        void LateUpdate() => Drain(PlayerLoopTiming.LateUpdate);

        void Drain(PlayerLoopTiming timing)
        {
            var index = ClampTiming(timing);
            lock (s_gate)
            {
                var queue = s_posted[index];
                while (queue.Count > 0)
                {
                    s_execBuffer.Add(queue.Dequeue());
                }
            }

            for (var i = 0; i < s_execBuffer.Count; i++)
            {
                try
                {
                    s_execBuffer[i]?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            s_execBuffer.Clear();
        }
    }

    /// <summary>
    /// Allows <c>yield return flow.ToYieldInstruction()</c> inside Unity coroutines.
    /// </summary>
    public sealed class FlowYieldInstruction : CustomYieldInstruction
    {
        readonly Flow _flow;

        public FlowYieldInstruction(Flow flow)
        {
            _flow = flow ?? throw new ArgumentNullException(nameof(flow));
        }

        public override bool keepWaiting
        {
            get
            {
                if (!_flow.IsCompleted)
                {
                    return true;
                }

                _flow.ThrowIfFaulted();
                return false;
            }
        }
    }
}
