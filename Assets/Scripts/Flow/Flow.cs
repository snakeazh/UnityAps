using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Awaitable flow unit with no result. External types may inherit this class
    /// (or implement <see cref="IFlowAwaitable"/>) to participate in await / WhenAll.
    /// Static factories live here (industry-style entry API, similar to UniTask).
    /// Supports <c>async Flow</c> via <see cref="AsyncFlowMethodBuilder"/>.
    /// </summary>
    [AsyncMethodBuilder(typeof(AsyncFlowMethodBuilder))]
    public partial class Flow : IFlowAwaitable
    {
        static int s_nextId = 1;

        int _id;
        readonly object _gate = new object();
        bool _completed;
        bool _observed;
        bool _fromPool;
        bool _returnedToPool;
        Exception _exception;
        Action _continuation;
        readonly List<Action> _extraContinuations = new List<Action>(0);
        CancellationTokenRegistration _cancelRegistration;

        protected Flow()
        {
            _id = s_nextId++;
        }

        internal static Flow CreatePooled()
        {
            return new Flow { _fromPool = true };
        }

        internal bool IsPooledInstance => _fromPool;

        internal void PrepareFromPool()
        {
            _id = s_nextId++;
            _completed = false;
            _observed = false;
            _returnedToPool = false;
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

        public FlowAwaiter GetAwaiter()
        {
            MarkObserved();
            return new FlowAwaiter(this);
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

        /// <summary>
        /// Observe result without awaiting further. Faults are logged; instance may return to pool.
        /// </summary>
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

        public void GetResultAsVoid()
        {
            MarkObserved();
            try
            {
                ThrowIfFaulted();
            }
            finally
            {
                TryReturnToPool();
            }
        }

        public void ThrowIfFaulted()
        {
            MarkObserved();
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

        void MarkObserved()
        {
            lock (_gate)
            {
                _observed = true;
            }
        }

        /// <summary>Complete successfully. Safe to call once; later calls are ignored.</summary>
        protected void SetResult() => Complete(null);

        /// <summary>Fail the flow. Safe to call once; later calls are ignored.</summary>
        protected void SetException(Exception exception) =>
            Complete(exception ?? new Exception("Flow faulted with null exception."));

        /// <summary>Cancel via <see cref="OperationCanceledException"/>.</summary>
        protected void SetCanceled() => Complete(new OperationCanceledException());

        protected void SetCanceled(CancellationToken cancellationToken) =>
            Complete(new OperationCanceledException(cancellationToken));

        public void TrySetResult() => Complete(null);

        public void TrySetException(Exception exception) =>
            Complete(exception ?? new Exception("Flow faulted with null exception."));

        public void TrySetCanceled() => Complete(new OperationCanceledException());

        public void TrySetCanceled(CancellationToken cancellationToken) =>
            Complete(new OperationCanceledException(cancellationToken));

        /// <summary>
        /// When <paramref name="cancellationToken"/> fires, this flow becomes canceled.
        /// </summary>
        public Flow AttachCancellation(CancellationToken cancellationToken)
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

        void Complete(Exception exception)
        {
            if (!FlowRunner.IsMainThread)
            {
                FlowRunner.Post(() => Complete(exception));
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
                _exception = exception;
                first = _continuation;
                _continuation = null;
                if (_extraContinuations.Count > 0)
                {
                    extras = _extraContinuations.ToArray();
                    _extraContinuations.Clear();
                }

                // Fault with nobody waiting yet → check next tick for unobserved exception.
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
                Debug.LogException(new Exception($"Unobserved Flow fault (Id={_id})", error));
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

            FlowPool.ReturnVoid(this);
        }

        public IEnumerator ToCoroutine()
        {
            MarkObserved();
            while (!IsCompleted)
            {
                yield return null;
            }

            GetResultAsVoid();
        }

        public FlowYieldInstruction ToYieldInstruction()
        {
            MarkObserved();
            return new FlowYieldInstruction(this);
        }

        #region Static factories

        public static Flow Completed()
        {
            var flow = FlowPool.RentVoid();
            flow.TrySetResult();
            return flow;
        }

        public static Flow Create(Action<Flow> starter)
        {
            if (starter == null)
            {
                throw new ArgumentNullException(nameof(starter));
            }

            var flow = FlowPool.RentVoid();
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
            var flow = FlowPool.RentVoid();
            flow.TrySetException(exception);
            return flow;
        }

        public static Flow Canceled(CancellationToken cancellationToken = default)
        {
            var flow = FlowPool.RentVoid();
            flow.TrySetCanceled(cancellationToken);
            return flow;
        }

        public static Flow Delay(
            float seconds,
            bool ignoreTimeScale = true,
            CancellationToken cancellationToken = default)
        {
            var flow = FlowPool.RentVoid();
            flow.AttachCancellation(cancellationToken);
            if (flow.IsCompleted)
            {
                return flow;
            }

            FlowRunner.StartRoutine(DelayRoutine(flow, Mathf.Max(0f, seconds), ignoreTimeScale, cancellationToken));
            return flow;
        }

        public static Flow NextFrame(CancellationToken cancellationToken = default)
        {
            var flow = FlowPool.RentVoid();
            flow.AttachCancellation(cancellationToken);
            if (flow.IsCompleted)
            {
                return flow;
            }

            FlowRunner.StartRoutine(NextFrameRoutine(flow, cancellationToken));
            return flow;
        }

        /// <summary>Wrap a Unity coroutine as an awaitable Flow.</summary>
        public static Flow FromCoroutine(IEnumerator routine, CancellationToken cancellationToken = default)
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

            FlowRunner.StartRoutine(WrapCoroutine(flow, routine, cancellationToken));
            return flow;
        }

        public static Flow WhenAll(params Flow[] flows) => WhenAll(default, flows);

        public static Flow WhenAll(CancellationToken cancellationToken, params Flow[] flows)
        {
            if (flows == null || flows.Length == 0)
            {
                return cancellationToken.IsCancellationRequested
                    ? Canceled(cancellationToken)
                    : Completed();
            }

            var parent = FlowPool.RentVoid();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = flows.Length;
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
                        return;
                    }

                    if (child.IsCanceled)
                    {
                        parent.TrySetCanceled();
                        return;
                    }

                    if (Interlocked.Decrement(ref remaining) == 0)
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

            var parent = FlowPool.RentVoid();
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
                    else if (child.IsCanceled)
                    {
                        parent.TrySetCanceled();
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

        static IEnumerator DelayRoutine(
            Flow flow,
            float seconds,
            bool ignoreTimeScale,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                flow.TrySetCanceled(cancellationToken);
                yield break;
            }

            if (seconds <= 0f)
            {
                flow.TrySetResult();
                yield break;
            }

            if (ignoreTimeScale)
            {
                var end = Time.realtimeSinceStartup + seconds;
                while (Time.realtimeSinceStartup < end)
                {
                    if (cancellationToken.IsCancellationRequested || flow.IsCompleted)
                    {
                        if (!flow.IsCompleted)
                        {
                            flow.TrySetCanceled(cancellationToken);
                        }

                        yield break;
                    }

                    yield return null;
                }
            }
            else
            {
                var elapsed = 0f;
                while (elapsed < seconds)
                {
                    if (cancellationToken.IsCancellationRequested || flow.IsCompleted)
                    {
                        if (!flow.IsCompleted)
                        {
                            flow.TrySetCanceled(cancellationToken);
                        }

                        yield break;
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            if (!flow.IsCompleted)
            {
                flow.TrySetResult();
            }
        }

        static IEnumerator NextFrameRoutine(Flow flow, CancellationToken cancellationToken)
        {
            yield return null;
            if (cancellationToken.IsCancellationRequested)
            {
                flow.TrySetCanceled(cancellationToken);
            }
            else if (!flow.IsCompleted)
            {
                flow.TrySetResult();
            }
        }

        static IEnumerator WrapCoroutine(Flow flow, IEnumerator routine, CancellationToken cancellationToken)
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

        #endregion
    }
}
