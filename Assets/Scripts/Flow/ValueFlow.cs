using System;
using System.Runtime.CompilerServices;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Zero-allocation completed void awaitable (struct). Prefer this over pooled
    /// <see cref="Flow.Completed"/> when the result is already known.
    /// <code>await ValueFlow.Completed;</code>
    /// </summary>
    public readonly struct ValueFlow
    {
        readonly Exception _exception;
        readonly bool _hasException;

        ValueFlow(Exception exception, bool hasException)
        {
            _exception = exception;
            _hasException = hasException;
        }

        public static ValueFlow Completed => default;

        public static ValueFlow FromException(Exception exception) =>
            new ValueFlow(exception ?? new Exception("ValueFlow faulted."), true);

        public static ValueFlow Canceled() =>
            new ValueFlow(new OperationCanceledException(), true);

        public ValueFlowAwaiter GetAwaiter() => new ValueFlowAwaiter(this);

        public readonly struct ValueFlowAwaiter : ICriticalNotifyCompletion
        {
            readonly ValueFlow _flow;

            public ValueFlowAwaiter(ValueFlow flow)
            {
                _flow = flow;
            }

            public bool IsCompleted => true;

            public void OnCompleted(Action continuation) => continuation?.Invoke();

            public void UnsafeOnCompleted(Action continuation) => continuation?.Invoke();

            public void GetResult()
            {
                if (_flow._hasException && _flow._exception != null)
                {
                    throw _flow._exception;
                }
            }
        }
    }

    /// <summary>
    /// Zero-allocation completed typed awaitable (struct).
    /// <code>await ValueFlow.FromResult(42);</code>
    /// </summary>
    public readonly struct ValueFlow<T>
    {
        readonly T _result;
        readonly Exception _exception;
        readonly bool _hasException;

        ValueFlow(T result, Exception exception, bool hasException)
        {
            _result = result;
            _exception = exception;
            _hasException = hasException;
        }

        public static ValueFlow<T> FromResult(T value) => new ValueFlow<T>(value, null, false);

        public static ValueFlow<T> FromException(Exception exception) =>
            new ValueFlow<T>(default, exception ?? new Exception("ValueFlow<T> faulted."), true);

        public static ValueFlow<T> Canceled() =>
            new ValueFlow<T>(default, new OperationCanceledException(), true);

        public ValueFlowAwaiter GetAwaiter() => new ValueFlowAwaiter(this);

        public readonly struct ValueFlowAwaiter : ICriticalNotifyCompletion
        {
            readonly ValueFlow<T> _flow;

            public ValueFlowAwaiter(ValueFlow<T> flow)
            {
                _flow = flow;
            }

            public bool IsCompleted => true;

            public void OnCompleted(Action continuation) => continuation?.Invoke();

            public void UnsafeOnCompleted(Action continuation) => continuation?.Invoke();

            public T GetResult()
            {
                if (_flow._hasException && _flow._exception != null)
                {
                    throw _flow._exception;
                }

                return _flow._result;
            }
        }
    }

    public partial class Flow
    {
        /// <summary>Zero-allocation completed void awaitable.</summary>
        public static ValueFlow CompletedValue() => ValueFlow.Completed;

        /// <summary>Zero-allocation completed typed awaitable.</summary>
        public static ValueFlow<T> FromResultValue<T>(T value) => ValueFlow<T>.FromResult(value);
    }
}
