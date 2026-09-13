using System;
using System.Runtime.CompilerServices;

namespace CoinFlip.FlowFramework
{
    public struct FlowAwaiter : INotifyCompletion
    {
        readonly Flow _flow;

        public FlowAwaiter(Flow flow)
        {
            _flow = flow;
        }

        public bool IsCompleted => _flow.IsCompleted;

        public void OnCompleted(Action continuation) => _flow.OnCompleted(continuation);

        public void GetResult() => _flow.ThrowIfFaulted();
    }

    public struct FlowAwaiter<T> : INotifyCompletion
    {
        readonly Flow<T> _flow;

        public FlowAwaiter(Flow<T> flow)
        {
            _flow = flow;
        }

        public bool IsCompleted => _flow.IsCompleted;

        public void OnCompleted(Action continuation) => _flow.OnCompleted(continuation);

        public T GetResult() => _flow.GetResult();
    }
}
