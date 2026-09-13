using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Awaitable flow unit with no result. External types may inherit this class
    /// (or implement <see cref="IFlowAwaitable"/>) to participate in await / WhenAll.
    /// Static factories live here (industry-style entry API, similar to UniTask).
    /// </summary>
    public partial class Flow : IFlowAwaitable
    {
        static int s_nextId = 1;

        readonly int _id = s_nextId++;
        readonly object _gate = new object();
        bool _completed;
        Exception _exception;
        Action _continuation;
        readonly List<Action> _extraContinuations = new List<Action>(0);

        public bool IsCompleted
        {
            get
            {
                lock (_gate)
                {
                    return _completed;
                }
            }
        }

        public bool IsFaulted
        {
            get
            {
                lock (_gate)
                {
                    return _completed && _exception != null;
                }
            }
        }

        public bool IsCanceled
        {
            get
            {
                lock (_gate)
                {
                    return _exception is OperationCanceledException;
                }
            }
        }

        public int Id => _id;

        public FlowAwaiter GetAwaiter() => new FlowAwaiter(this);

        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            var alreadyDone = false;
            lock (_gate)
            {
                if (_completed)
                {
                    alreadyDone = true;
                }
                else if (_continuation == null)
                {
                    _continuation = continuation;
                }
                else
                {
                    _extraContinuations.Add(continuation);
                }
            }

            if (alreadyDone)
            {
                // Always marshal to main-thread pump.
                FlowRunner.Post(continuation);
            }
        }

        public void ThrowIfFaulted()
        {
            Exception error;
            lock (_gate)
            {
                error = _exception;
            }

            if (error != null)
            {
                throw error;
            }
        }

        /// <summary>Complete successfully. Safe to call once; later calls are ignored.</summary>
        protected void SetResult()
        {
            Complete(null);
        }

        /// <summary>Fail the flow. Safe to call once; later calls are ignored.</summary>
        protected void SetException(Exception exception)
        {
            Complete(exception ?? new Exception("Flow faulted with null exception."));
        }

        /// <summary>Cancel via <see cref="OperationCanceledException"/>.</summary>
        protected void SetCanceled()
        {
            Complete(new OperationCanceledException());
        }

        /// <summary>Public complete for factory-built flows that are not subclassed.</summary>
        public void TrySetResult() => Complete(null);

        public void TrySetException(Exception exception) =>
            Complete(exception ?? new Exception("Flow faulted with null exception."));

        public void TrySetCanceled() => Complete(new OperationCanceledException());

        void Complete(Exception exception)
        {
            // Completion always runs on the Unity main thread so Unity API use in
            // continuations / subclass overrides stays safe.
            if (!FlowRunner.IsMainThread)
            {
                FlowRunner.Post(() => Complete(exception));
                return;
            }

            Action first = null;
            Action[] extras = null;
            lock (_gate)
            {
                if (_completed)
                {
                    return;
                }

                _completed = true;
                _exception = exception;
                first = _continuation;
                _continuation = null;
                if (_extraContinuations.Count > 0)
                {
                    extras = _extraContinuations.ToArray();
                    _extraContinuations.Clear();
                }
            }

            if (first != null)
            {
                FlowRunner.Post(first);
            }

            if (extras != null)
            {
                for (var i = 0; i < extras.Length; i++)
                {
                    FlowRunner.Post(extras[i]);
                }
            }
        }

        public IEnumerator ToCoroutine()
        {
            while (!_completed)
            {
                yield return null;
            }

            ThrowIfFaulted();
        }

        public FlowYieldInstruction ToYieldInstruction() => new FlowYieldInstruction(this);

        #region Static factories

        public static Flow Completed()
        {
            var flow = new Flow();
            flow.TrySetResult();
            return flow;
        }

        /// <summary>Create a flow and run a starter that completes it via TrySet*.</summary>
        public static Flow Create(Action<Flow> starter)
        {
            if (starter == null)
            {
                throw new ArgumentNullException(nameof(starter));
            }

            var flow = new Flow();
            try
            {
                starter(flow);
            }
            catch (Exception ex)
            {
                flow.TrySetException(ex);
            }

            return flow;
        }

        public static Flow FromException(Exception exception)
        {
            var flow = new Flow();
            flow.TrySetException(exception);
            return flow;
        }

        public static Flow Delay(float seconds, bool ignoreTimeScale = true)
        {
            var flow = new Flow();
            FlowRunner.StartRoutine(DelayRoutine(flow, Mathf.Max(0f, seconds), ignoreTimeScale));
            return flow;
        }

        public static Flow NextFrame()
        {
            var flow = new Flow();
            FlowRunner.StartRoutine(NextFrameRoutine(flow));
            return flow;
        }

        /// <summary>Wrap a Unity coroutine as an awaitable Flow.</summary>
        public static Flow FromCoroutine(IEnumerator routine)
        {
            if (routine == null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            var flow = new Flow();
            FlowRunner.StartRoutine(WrapCoroutine(flow, routine));
            return flow;
        }

        public static Flow WhenAll(params Flow[] flows)
        {
            if (flows == null || flows.Length == 0)
            {
                return Completed();
            }

            var parent = new Flow();
            var remaining = flows.Length;
            for (var i = 0; i < flows.Length; i++)
            {
                var child = flows[i] ?? Completed();
                child.OnCompleted(() =>
                {
                    if (child.IsFaulted)
                    {
                        parent.TrySetException(GetException(child));
                        return;
                    }

                    if (System.Threading.Interlocked.Decrement(ref remaining) == 0)
                    {
                        parent.TrySetResult();
                    }
                });
            }

            return parent;
        }

        public static Flow WhenAny(params Flow[] flows)
        {
            if (flows == null || flows.Length == 0)
            {
                return Completed();
            }

            var parent = new Flow();
            for (var i = 0; i < flows.Length; i++)
            {
                var child = flows[i] ?? Completed();
                child.OnCompleted(() =>
                {
                    if (parent.IsCompleted)
                    {
                        return;
                    }

                    if (child.IsFaulted)
                    {
                        parent.TrySetException(GetException(child));
                    }
                    else
                    {
                        parent.TrySetResult();
                    }
                });
            }

            return parent;
        }

        static Exception GetException(Flow flow)
        {
            try
            {
                flow.ThrowIfFaulted();
                return new Exception("Flow faulted.");
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        static IEnumerator DelayRoutine(Flow flow, float seconds, bool ignoreTimeScale)
        {
            if (seconds <= 0f)
            {
                flow.TrySetResult();
                yield break;
            }

            if (ignoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(seconds);
            }
            else
            {
                yield return new WaitForSeconds(seconds);
            }

            flow.TrySetResult();
        }

        static IEnumerator NextFrameRoutine(Flow flow)
        {
            yield return null;
            flow.TrySetResult();
        }

        static IEnumerator WrapCoroutine(Flow flow, IEnumerator routine)
        {
            Exception error = null;
            while (true)
            {
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

            if (error != null)
            {
                flow.TrySetException(error);
            }
            else
            {
                flow.TrySetResult();
            }
        }

        #endregion
    }
}
