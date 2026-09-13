using System;
using System.Threading;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Medium-value combinators: WhenAny (index / typed), Timeout, Then / ContinueWith.
    /// </summary>
    public partial class Flow
    {
        /// <summary>
        /// Completes with the index of the first flow that succeeds.
        /// Fault/cancel from the winner is propagated; losers are ignored once a winner is chosen.
        /// </summary>
        public static Flow<int> WhenAnyIndex(params Flow[] flows)
        {
            if (flows == null || flows.Length == 0)
            {
                return Flow<int>.FromResult(0);
            }

            var parent = FlowPool.Rent<int>();
            for (var i = 0; i < flows.Length; i++)
            {
                var index = i;
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
                        child.GetResultAsVoid();
                        parent.TrySetResult(index);
                    }
                });
            }

            return parent;
        }

        /// <summary>
        /// First successful <see cref="Flow{T}"/> wins; result is (index, value).
        /// </summary>
        public static Flow<(int index, T value)> WhenAny<T>(params Flow<T>[] flows)
        {
            if (flows == null || flows.Length == 0)
            {
                return Flow<(int index, T value)>.FromResult((0, default));
            }

            var parent = FlowPool.Rent<(int index, T value)>();
            for (var i = 0; i < flows.Length; i++)
            {
                var index = i;
                var child = flows[i] ?? Flow<T>.FromResult(default);
                child.OnCompleted(() =>
                {
                    if (parent.IsCompleted)
                    {
                        return;
                    }

                    if (child.IsFaulted)
                    {
                        try
                        {
                            child.GetResult();
                        }
                        catch (Exception ex)
                        {
                            parent.TrySetException(ex);
                        }

                        return;
                    }

                    if (child.IsCanceled)
                    {
                        parent.TrySetCanceled();
                        return;
                    }

                    parent.TrySetResult((index, child.GetResult()));
                });
            }

            return parent;
        }

        /// <summary>
        /// Fail with <see cref="TimeoutException"/> if this flow does not complete in time.
        /// </summary>
        public Flow Timeout(
            float seconds,
            bool ignoreTimeScale = true,
            CancellationToken cancellationToken = default)
        {
            var result = FlowPool.RentVoid();
            result.AttachCancellation(cancellationToken);
            if (result.IsCompleted)
            {
                return result;
            }

            var timer = Delay(Math.Max(0f, seconds), ignoreTimeScale, cancellationToken);
            timer.OnCompleted(() =>
            {
                if (result.IsCompleted)
                {
                    return;
                }

                if (timer.IsCanceled)
                {
                    result.TrySetCanceled();
                    return;
                }

                if (timer.IsFaulted)
                {
                    result.TrySetException(GetException(timer));
                    return;
                }

                timer.GetResultAsVoid();
                result.TrySetException(new TimeoutException($"Flow timed out after {seconds:0.###}s."));
            });

            OnCompleted(() =>
            {
                if (result.IsCompleted)
                {
                    return;
                }

                if (IsFaulted)
                {
                    result.TrySetException(GetException(this));
                    return;
                }

                if (IsCanceled)
                {
                    result.TrySetCanceled();
                    return;
                }

                GetResultAsVoid();
                result.TrySetResult();
            });

            return result;
        }

        /// <summary>Run <paramref name="action"/> after success; propagate fault/cancel.</summary>
        public Flow Then(Action action)
        {
            var next = FlowPool.RentVoid();
            OnCompleted(() =>
            {
                if (IsFaulted)
                {
                    next.TrySetException(GetException(this));
                    return;
                }

                if (IsCanceled)
                {
                    next.TrySetCanceled();
                    return;
                }

                try
                {
                    GetResultAsVoid();
                    action?.Invoke();
                    next.TrySetResult();
                }
                catch (Exception ex)
                {
                    next.TrySetException(ex);
                }
            });
            return next;
        }

        /// <summary>Chain another void Flow after success.</summary>
        public Flow Then(Func<Flow> nextFactory)
        {
            if (nextFactory == null)
            {
                throw new ArgumentNullException(nameof(nextFactory));
            }

            var next = FlowPool.RentVoid();
            OnCompleted(() =>
            {
                if (IsFaulted)
                {
                    next.TrySetException(GetException(this));
                    return;
                }

                if (IsCanceled)
                {
                    next.TrySetCanceled();
                    return;
                }

                try
                {
                    GetResultAsVoid();
                    var child = nextFactory() ?? Completed();
                    child.OnCompleted(() => PropagateVoid(child, next));
                }
                catch (Exception ex)
                {
                    next.TrySetException(ex);
                }
            });
            return next;
        }

        /// <summary>Alias of <see cref="Then(Func{Flow})"/>.</summary>
        public Flow ContinueWith(Func<Flow> nextFactory) => Then(nextFactory);

        /// <summary>Chain a typed Flow after success.</summary>
        public Flow<T> Then<T>(Func<Flow<T>> nextFactory)
        {
            if (nextFactory == null)
            {
                throw new ArgumentNullException(nameof(nextFactory));
            }

            var next = FlowPool.Rent<T>();
            OnCompleted(() =>
            {
                if (IsFaulted)
                {
                    next.TrySetException(GetException(this));
                    return;
                }

                if (IsCanceled)
                {
                    next.TrySetCanceled();
                    return;
                }

                try
                {
                    GetResultAsVoid();
                    var child = nextFactory() ?? Flow<T>.FromResult(default);
                    child.OnCompleted(() => PropagateValue(child, next));
                }
                catch (Exception ex)
                {
                    next.TrySetException(ex);
                }
            });
            return next;
        }

        /// <summary>Alias of <see cref="Then{T}(Func{Flow{T}})"/>.</summary>
        public Flow<T> ContinueWith<T>(Func<Flow<T>> nextFactory) => Then(nextFactory);

        internal static void PropagateVoid(Flow child, Flow parent)
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
                child.GetResultAsVoid();
                parent.TrySetResult();
            }
        }

        internal static void PropagateValue<T>(Flow<T> child, Flow<T> parent)
        {
            if (parent.IsCompleted)
            {
                return;
            }

            if (child.IsFaulted)
            {
                try
                {
                    child.GetResult();
                }
                catch (Exception ex)
                {
                    parent.TrySetException(ex);
                }

                return;
            }

            if (child.IsCanceled)
            {
                parent.TrySetCanceled();
                return;
            }

            parent.TrySetResult(child.GetResult());
        }
    }

    public partial class Flow<T>
    {
        /// <summary>Fail with <see cref="TimeoutException"/> if this flow does not complete in time.</summary>
        public Flow<T> Timeout(
            float seconds,
            bool ignoreTimeScale = true,
            CancellationToken cancellationToken = default)
        {
            var result = FlowPool.Rent<T>();
            result.AttachCancellation(cancellationToken);
            if (result.IsCompleted)
            {
                return result;
            }

            var timer = Flow.Delay(Math.Max(0f, seconds), ignoreTimeScale, cancellationToken);
            timer.OnCompleted(() =>
            {
                if (result.IsCompleted)
                {
                    return;
                }

                if (timer.IsCanceled)
                {
                    result.TrySetCanceled();
                    return;
                }

                if (timer.IsFaulted)
                {
                    result.TrySetException(Flow.GetExceptionPublic(timer));
                    return;
                }

                timer.GetResultAsVoid();
                result.TrySetException(new TimeoutException($"Flow<{typeof(T).Name}> timed out after {seconds:0.###}s."));
            });

            OnCompleted(() =>
            {
                if (result.IsCompleted)
                {
                    return;
                }

                if (IsFaulted)
                {
                    try
                    {
                        GetResult();
                    }
                    catch (Exception ex)
                    {
                        result.TrySetException(ex);
                    }

                    return;
                }

                if (IsCanceled)
                {
                    result.TrySetCanceled();
                    return;
                }

                result.TrySetResult(GetResult());
            });

            return result;
        }

        /// <summary>Map the result after success.</summary>
        public Flow<TOut> Then<TOut>(Func<T, TOut> map)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            var next = FlowPool.Rent<TOut>();
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
                        next.TrySetException(ex);
                    }

                    return;
                }

                if (IsCanceled)
                {
                    next.TrySetCanceled();
                    return;
                }

                try
                {
                    next.TrySetResult(map(GetResult()));
                }
                catch (Exception ex)
                {
                    next.TrySetException(ex);
                }
            });
            return next;
        }

        /// <summary>Chain another typed Flow using this result.</summary>
        public Flow<TOut> Then<TOut>(Func<T, Flow<TOut>> nextFactory)
        {
            if (nextFactory == null)
            {
                throw new ArgumentNullException(nameof(nextFactory));
            }

            var next = FlowPool.Rent<TOut>();
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
                        next.TrySetException(ex);
                    }

                    return;
                }

                if (IsCanceled)
                {
                    next.TrySetCanceled();
                    return;
                }

                try
                {
                    var child = nextFactory(GetResult()) ?? Flow<TOut>.FromResult(default);
                    child.OnCompleted(() => Flow.PropagateValue(child, next));
                }
                catch (Exception ex)
                {
                    next.TrySetException(ex);
                }
            });
            return next;
        }

        /// <summary>Alias of <see cref="Then{TOut}(Func{T, Flow{TOut}})"/>.</summary>
        public Flow<TOut> ContinueWith<TOut>(Func<T, Flow<TOut>> nextFactory) => Then(nextFactory);

        /// <summary>Drop the value and continue as a void Flow.</summary>
        public Flow Then(Action<T> action)
        {
            var next = FlowPool.RentVoid();
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
                        next.TrySetException(ex);
                    }

                    return;
                }

                if (IsCanceled)
                {
                    next.TrySetCanceled();
                    return;
                }

                try
                {
                    action?.Invoke(GetResult());
                    next.TrySetResult();
                }
                catch (Exception ex)
                {
                    next.TrySetException(ex);
                }
            });
            return next;
        }
    }

    // Allow Flow<T>.Timeout to reuse exception helper without widening GetException.
    public partial class Flow
    {
        internal static Exception GetExceptionPublic(Flow flow) => GetException(flow);
    }
}
