using System;
using System.Runtime.CompilerServices;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Enables <c>async Flow</c> methods. Continuations resume through <see cref="FlowRunner"/>.
    /// </summary>
    public struct AsyncFlowMethodBuilder
    {
        Flow _flow;

        public static AsyncFlowMethodBuilder Create()
        {
            return new AsyncFlowMethodBuilder
            {
                _flow = FlowPool.RentVoid(),
            };
        }

        public Flow Task => _flow;

        public void SetResult() => _flow.TrySetResult();

        public void SetException(Exception exception) =>
            _flow.TrySetException(exception ?? new Exception("async Flow faulted."));

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var box = (IAsyncStateMachine)stateMachine;
            stateMachine = (TStateMachine)box;
            awaiter.OnCompleted(box.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var box = (IAsyncStateMachine)stateMachine;
            stateMachine = (TStateMachine)box;
            awaiter.UnsafeOnCompleted(box.MoveNext);
        }
    }

    /// <summary>
    /// Enables <c>async Flow&lt;T&gt;</c> methods.
    /// </summary>
    public struct AsyncFlowMethodBuilder<T>
    {
        Flow<T> _flow;

        public static AsyncFlowMethodBuilder<T> Create()
        {
            return new AsyncFlowMethodBuilder<T>
            {
                _flow = FlowPool.Rent<T>(),
            };
        }

        public Flow<T> Task => _flow;

        public void SetResult(T result) => _flow.TrySetResult(result);

        public void SetException(Exception exception) =>
            _flow.TrySetException(exception ?? new Exception("async Flow<T> faulted."));

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var box = (IAsyncStateMachine)stateMachine;
            stateMachine = (TStateMachine)box;
            awaiter.OnCompleted(box.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter,
            ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var box = (IAsyncStateMachine)stateMachine;
            stateMachine = (TStateMachine)box;
            awaiter.UnsafeOnCompleted(box.MoveNext);
        }
    }
}
