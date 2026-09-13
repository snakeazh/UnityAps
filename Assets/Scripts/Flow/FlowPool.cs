using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Small pool for factory-created <see cref="Flow"/> / <see cref="Flow{T}"/> instances.
    /// Subclasses are never pooled.
    /// </summary>
    static class FlowPool
    {
        const int MaxSize = 64;
        static readonly Stack<Flow> s_void = new Stack<Flow>(16);
        static readonly object s_voidGate = new object();

        // Per-closed-type pools for Flow<T> would need a concurrent dictionary; keep a simple
        // typed pool helper used by Flow<T>.
        public static Flow RentVoid()
        {
            lock (s_voidGate)
            {
                if (s_void.Count > 0)
                {
                    var flow = s_void.Pop();
                    flow.PrepareFromPool();
                    return flow;
                }
            }

            return Flow.CreatePooled();
        }

        public static void ReturnVoid(Flow flow)
        {
            if (flow == null || !flow.IsPooledInstance)
            {
                return;
            }

            flow.ResetForPool();
            lock (s_voidGate)
            {
                if (s_void.Count < MaxSize)
                {
                    s_void.Push(flow);
                }
            }
        }

        public static Flow<T> Rent<T>()
        {
            return FlowTypedPool<T>.Rent();
        }

        public static void Return<T>(Flow<T> flow)
        {
            FlowTypedPool<T>.Return(flow);
        }

        static class FlowTypedPool<T>
        {
            static readonly Stack<Flow<T>> s_stack = new Stack<Flow<T>>(16);
            static readonly object s_gate = new object();

            public static Flow<T> Rent()
            {
                lock (s_gate)
                {
                    if (s_stack.Count > 0)
                    {
                        var flow = s_stack.Pop();
                        flow.PrepareFromPool();
                        return flow;
                    }
                }

                return Flow<T>.CreatePooled();
            }

            public static void Return(Flow<T> flow)
            {
                if (flow == null || !flow.IsPooledInstance)
                {
                    return;
                }

                flow.ResetForPool();
                lock (s_gate)
                {
                    if (s_stack.Count < MaxSize)
                    {
                        s_stack.Push(flow);
                    }
                }
            }
        }
    }

    /// <summary>Shared cancellation registration helpers.</summary>
    static class FlowCancellation
    {
        public static CancellationTokenRegistration Attach(Flow flow, CancellationToken cancellationToken)
        {
            if (flow == null || !cancellationToken.CanBeCanceled)
            {
                return default;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                flow.TrySetCanceled(cancellationToken);
                return default;
            }

            return cancellationToken.Register(static state =>
            {
                var box = ((Flow flow, CancellationToken token))state;
                box.flow.TrySetCanceled(box.token);
            }, (flow, cancellationToken));
        }

        public static CancellationTokenRegistration Attach<T>(Flow<T> flow, CancellationToken cancellationToken)
        {
            if (flow == null || !cancellationToken.CanBeCanceled)
            {
                return default;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                flow.TrySetCanceled(cancellationToken);
                return default;
            }

            return cancellationToken.Register(static state =>
            {
                var box = ((Flow<T> flow, CancellationToken token))state;
                box.flow.TrySetCanceled(box.token);
            }, (flow, cancellationToken));
        }
    }
}
