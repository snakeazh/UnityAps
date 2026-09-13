using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Awaitable flow with a typed result. Inherit and call <see cref="SetResult"/> /
    /// <see cref="SetException"/>, or complete via <see cref="TrySetResult"/>.
    /// </summary>
    public class Flow<T> : IFlowAwaitable<T>
    {
        static int s_nextId = 1;

        int _id;
        readonly object _gate = new object();
        bool _completed;
        bool _observed;
        bool _fromPool;
        bool _returnedToPool;
        T _result;
        Exception _exception;
        Action _continuation;
        readonly List<Action> _extraContinuations = new List<Action>(0);
        CancellationTokenRegistration _cancelRegistration;

        public Flow()
        {
            _id = s_nextId++;
        }

        internal static Flow<T> CreatePooled()
        {
            return new Flow<T> { _fromPool = true };
        }

        internal bool IsPooledInstance => _fromPool;

        internal void PrepareFromPool()
        {
            _id = s_nextId++;
            _completed = false;
            _observed = false;
            _returnedToPool = false;
            _result = default;
            _exception = null;
            _continuation = null;
            _extraContinuations.Clear();
            _cancelRegistration = default;
        }

        internal void ResetForPool()
        {
            DisposeCancellationRegistration();
            _completed = false;
            _observed = false;
            // Keep _returnedToPool true while stored in the pool so a stale
            // reference cannot return the same instance twice.
            _result = default;
            _exception = null;
            _continuation = null;
            _extraContinuations.Clear();
        }

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
                    return _completed && _exception != null && _exception is not OperationCanceledException;
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

        public FlowAwaiter<T> GetAwaiter()
        {
            MarkObserved();
            return new FlowAwaiter<T>(this);
        }

        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            MarkObserved();

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
                FlowRunner.Post(continuation);
            }
        }

        public void Forget()
        {
            MarkObserved();
            if (IsCompleted)
            {
                HandleForgetContinuation();
                return;
            }

            OnCompleted(HandleForgetContinuation);
        }

        void HandleForgetContinuation()
        {
            Exception error;
            lock (_gate)
            {
                error = _exception;
            }

            if (error != null && error is not OperationCanceledException)
            {
                Debug.LogException(error);
            }

            TryReturnToPool();
        }

        public T GetResult()
        {
            MarkObserved();
            Exception error;
            T value;
            lock (_gate)
            {
                error = _exception;
                value = _result;
            }

            try
            {
                if (error != null)
                {
                    throw error;
                }

                return value;
            }
            finally
            {
                TryReturnToPool();
            }
        }

        void MarkObserved()
        {
            lock (_gate)
            {
                _observed = true;
            }
        }

        protected void SetResult(T value) => Complete(value, null);

        protected void SetException(Exception exception) =>
            Complete(default, exception ?? new Exception("Flow faulted with null exception."));

        protected void SetCanceled() => Complete(default, new OperationCanceledException());

        protected void SetCanceled(CancellationToken cancellationToken) =>
            Complete(default, new OperationCanceledException(cancellationToken));

        public void TrySetResult(T value) => Complete(value, null);

        public void TrySetException(Exception exception) =>
            Complete(default, exception ?? new Exception("Flow faulted with null exception."));

        public void TrySetCanceled() => Complete(default, new OperationCanceledException());

        public void TrySetCanceled(CancellationToken cancellationToken) =>
            Complete(default, new OperationCanceledException(cancellationToken));

        public Flow<T> AttachCancellation(CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled || IsCompleted)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    TrySetCanceled(cancellationToken);
                }

                return this;
            }

            DisposeCancellationRegistration();
            _cancelRegistration = FlowCancellation.Attach(this, cancellationToken);
            return this;
        }

        void DisposeCancellationRegistration()
        {
            _cancelRegistration.Dispose();
            _cancelRegistration = default;
        }

        void Complete(T value, Exception exception)
        {
            if (!FlowRunner.IsMainThread)
            {
                FlowRunner.Post(() => Complete(value, exception));
                return;
            }

            Action first = null;
            Action[] extras = null;
            var reportUnobserved = false;
            lock (_gate)
            {
                if (_completed)
                {
                    return;
                }

                _completed = true;
                _result = value;
                _exception = exception;
                first = _continuation;
                _continuation = null;
                if (_extraContinuations.Count > 0)
                {
                    extras = _extraContinuations.ToArray();
                    _extraContinuations.Clear();
                }

                reportUnobserved = exception != null
                    && exception is not OperationCanceledException
                    && first == null
                    && extras == null
                    && !_observed;
            }

            DisposeCancellationRegistration();

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

            if (reportUnobserved)
            {
                FlowRunner.Post(ReportUnobservedIfNeeded);
            }
        }

        void ReportUnobservedIfNeeded()
        {
            Exception error = null;
            lock (_gate)
            {
                if (!_observed && _exception != null && _exception is not OperationCanceledException)
                {
                    error = _exception;
                    _observed = true;
                }
            }

            if (error != null)
            {
                Debug.LogException(new Exception($"Unobserved Flow<{typeof(T).Name}> fault (Id={_id})", error));
            }
        }

        void TryReturnToPool()
        {
            lock (_gate)
            {
                if (!_fromPool || !_completed || _returnedToPool)
                {
                    return;
                }

                _returnedToPool = true;
            }

            FlowPool.Return(this);
        }

        public System.Collections.IEnumerator ToCoroutine(Action<T> onResult = null)
        {
            MarkObserved();
            while (!IsCompleted)
            {
                yield return null;
            }

            var value = GetResult();
            onResult?.Invoke(value);
        }

        public FlowYieldInstruction ToYieldInstruction()
        {
            MarkObserved();
            return new FlowYieldInstruction(AsVoid());
        }

        public Flow AsVoid()
        {
            var proxy = FlowPool.RentVoid();
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

                if (IsCanceled)
                {
                    proxy.TrySetCanceled();
                    return;
                }

                proxy.TrySetResult();
            });
            return proxy;
        }

        public static Flow<T> FromResult(T value)
        {
            var flow = FlowPool.Rent<T>();
            flow.TrySetResult(value);
            return flow;
        }

        public static Flow<T> FromException(Exception exception)
        {
            var flow = FlowPool.Rent<T>();
            flow.TrySetException(exception);
            return flow;
        }

        public static Flow<T> Canceled(CancellationToken cancellationToken = default)
        {
            var flow = FlowPool.Rent<T>();
            flow.TrySetCanceled(cancellationToken);
            return flow;
        }

        public static Flow<T> Create(Action<Flow<T>> starter)
        {
            if (starter == null)
            {
                throw new ArgumentNullException(nameof(starter));
            }

            var flow = FlowPool.Rent<T>();
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
