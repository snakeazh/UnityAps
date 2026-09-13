using System;
using System.Collections.Generic;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Awaitable flow with a typed result. Inherit and call <see cref="SetResult"/> /
    /// <see cref="SetException"/>, or complete via <see cref="TrySetResult"/>.
    /// </summary>
    public class Flow<T> : IFlowAwaitable<T>
    {
        bool _completed;
        T _result;
        Exception _exception;
        Action _continuation;
        readonly List<Action> _extraContinuations = new List<Action>(0);

        public bool IsCompleted => _completed;
        public bool IsFaulted => _completed && _exception != null;

        public FlowAwaiter<T> GetAwaiter() => new FlowAwaiter<T>(this);

        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            if (_completed)
            {
                FlowRunner.Post(continuation);
                return;
            }

            if (_continuation == null)
            {
                _continuation = continuation;
            }
            else
            {
                _extraContinuations.Add(continuation);
            }
        }

        public T GetResult()
        {
            if (_exception != null)
            {
                throw _exception;
            }

            return _result;
        }

        protected void SetResult(T value) => Complete(value, null);

        protected void SetException(Exception exception) =>
            Complete(default, exception ?? new Exception("Flow faulted with null exception."));

        protected void SetCanceled() => Complete(default, new OperationCanceledException());

        public void TrySetResult(T value) => Complete(value, null);

        public void TrySetException(Exception exception) =>
            Complete(default, exception ?? new Exception("Flow faulted with null exception."));

        public void TrySetCanceled() => Complete(default, new OperationCanceledException());

        void Complete(T value, Exception exception)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;
            _result = value;
            _exception = exception;

            var first = _continuation;
            _continuation = null;
            var extras = _extraContinuations.Count > 0 ? _extraContinuations.ToArray() : null;
            _extraContinuations.Clear();

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

        public System.Collections.IEnumerator ToCoroutine(Action<T> onResult = null)
        {
            while (!_completed)
            {
                yield return null;
            }

            var value = GetResult();
            onResult?.Invoke(value);
        }

        public FlowYieldInstruction ToYieldInstruction() => new FlowYieldInstruction(AsVoid());

        /// <summary>Project to a non-generic flow that completes when this does.</summary>
        public Flow AsVoid()
        {
            var proxy = new Flow();
            OnCompleted(() =>
            {
                if (IsFaulted)
                {
                    try
                    {
                        GetResult();
                    }
                    catch (Exception ex)
                    {
                        proxy.TrySetException(ex);
                    }

                    return;
                }

                proxy.TrySetResult();
            });
            return proxy;
        }

        public static Flow<T> FromResult(T value)
        {
            var flow = new Flow<T>();
            flow.TrySetResult(value);
            return flow;
        }

        public static Flow<T> FromException(Exception exception)
        {
            var flow = new Flow<T>();
            flow.TrySetException(exception);
            return flow;
        }

        public static Flow<T> Create(Action<Flow<T>> starter)
        {
            if (starter == null)
            {
                throw new ArgumentNullException(nameof(starter));
            }

            var flow = new Flow<T>();
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
    }
}
