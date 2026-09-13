using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Hidden runner that pumps continuations and host coroutines on the Unity main thread.
    /// </summary>
    public sealed class FlowRunner : MonoBehaviour
    {
        static FlowRunner s_instance;
        static readonly Queue<Action> s_posted = new Queue<Action>(32);
        static readonly List<Action> s_execBuffer = new List<Action>(32);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            Ensure();
        }

        public static FlowRunner Ensure()
        {
            if (s_instance != null)
            {
                return s_instance;
            }

            var go = new GameObject("[FlowRunner]");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            s_instance = go.AddComponent<FlowRunner>();
            return s_instance;
        }

        public static void Post(Action action)
        {
            if (action == null)
            {
                return;
            }

            Ensure();
            lock (s_posted)
            {
                s_posted.Enqueue(action);
            }
        }

        public static Coroutine StartRoutine(IEnumerator routine)
        {
            return Ensure().StartCoroutine(routine);
        }

        void Update()
        {
            lock (s_posted)
            {
                while (s_posted.Count > 0)
                {
                    s_execBuffer.Add(s_posted.Dequeue());
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
