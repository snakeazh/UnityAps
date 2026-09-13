using System;
using System.Threading;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Typed WhenAll overloads (2–16). Returns a Flow of ValueTuple results.
    /// Optional <see cref="CancellationToken"/> cancels the combined flow.
    /// </summary>
    public partial class Flow
    {
        public static Flow<(T1, T2)> WhenAll<T1, T2>(
            Flow<T1> f1, Flow<T2> f2,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 2;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            return parent;
        }
        public static Flow<(T1, T2, T3)> WhenAll<T1, T2, T3>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 3;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 4;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 5;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 6;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 7;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7, Flow<T8> f8,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7, T8)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 8;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;
            var a8 = f8 ?? Flow<T8>.FromResult(default);
            T8 r8 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8));
                }
            }

            void Done8()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a8.IsFaulted)
                {
                    try
                    {
                        a8.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a8.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r8 = a8.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            a8.OnCompleted(Done8);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7, Flow<T8> f8, Flow<T9> f9,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7, T8, T9)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 9;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;
            var a8 = f8 ?? Flow<T8>.FromResult(default);
            T8 r8 = default;
            var a9 = f9 ?? Flow<T9>.FromResult(default);
            T9 r9 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9));
                }
            }

            void Done8()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a8.IsFaulted)
                {
                    try
                    {
                        a8.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a8.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r8 = a8.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9));
                }
            }

            void Done9()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a9.IsFaulted)
                {
                    try
                    {
                        a9.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a9.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r9 = a9.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            a8.OnCompleted(Done8);
            a9.OnCompleted(Done9);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7, Flow<T8> f8, Flow<T9> f9, Flow<T10> f10,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 10;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;
            var a8 = f8 ?? Flow<T8>.FromResult(default);
            T8 r8 = default;
            var a9 = f9 ?? Flow<T9>.FromResult(default);
            T9 r9 = default;
            var a10 = f10 ?? Flow<T10>.FromResult(default);
            T10 r10 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            void Done8()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a8.IsFaulted)
                {
                    try
                    {
                        a8.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a8.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r8 = a8.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            void Done9()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a9.IsFaulted)
                {
                    try
                    {
                        a9.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a9.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r9 = a9.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            void Done10()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a10.IsFaulted)
                {
                    try
                    {
                        a10.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a10.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r10 = a10.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            a8.OnCompleted(Done8);
            a9.OnCompleted(Done9);
            a10.OnCompleted(Done10);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7, Flow<T8> f8, Flow<T9> f9, Flow<T10> f10, Flow<T11> f11,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 11;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;
            var a8 = f8 ?? Flow<T8>.FromResult(default);
            T8 r8 = default;
            var a9 = f9 ?? Flow<T9>.FromResult(default);
            T9 r9 = default;
            var a10 = f10 ?? Flow<T10>.FromResult(default);
            T10 r10 = default;
            var a11 = f11 ?? Flow<T11>.FromResult(default);
            T11 r11 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done8()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a8.IsFaulted)
                {
                    try
                    {
                        a8.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a8.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r8 = a8.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done9()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a9.IsFaulted)
                {
                    try
                    {
                        a9.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a9.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r9 = a9.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done10()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a10.IsFaulted)
                {
                    try
                    {
                        a10.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a10.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r10 = a10.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            void Done11()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a11.IsFaulted)
                {
                    try
                    {
                        a11.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a11.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r11 = a11.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            a8.OnCompleted(Done8);
            a9.OnCompleted(Done9);
            a10.OnCompleted(Done10);
            a11.OnCompleted(Done11);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7, Flow<T8> f8, Flow<T9> f9, Flow<T10> f10, Flow<T11> f11, Flow<T12> f12,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 12;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;
            var a8 = f8 ?? Flow<T8>.FromResult(default);
            T8 r8 = default;
            var a9 = f9 ?? Flow<T9>.FromResult(default);
            T9 r9 = default;
            var a10 = f10 ?? Flow<T10>.FromResult(default);
            T10 r10 = default;
            var a11 = f11 ?? Flow<T11>.FromResult(default);
            T11 r11 = default;
            var a12 = f12 ?? Flow<T12>.FromResult(default);
            T12 r12 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done8()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a8.IsFaulted)
                {
                    try
                    {
                        a8.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a8.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r8 = a8.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done9()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a9.IsFaulted)
                {
                    try
                    {
                        a9.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a9.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r9 = a9.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done10()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a10.IsFaulted)
                {
                    try
                    {
                        a10.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a10.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r10 = a10.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done11()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a11.IsFaulted)
                {
                    try
                    {
                        a11.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a11.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r11 = a11.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            void Done12()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a12.IsFaulted)
                {
                    try
                    {
                        a12.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a12.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r12 = a12.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            a8.OnCompleted(Done8);
            a9.OnCompleted(Done9);
            a10.OnCompleted(Done10);
            a11.OnCompleted(Done11);
            a12.OnCompleted(Done12);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7, Flow<T8> f8, Flow<T9> f9, Flow<T10> f10, Flow<T11> f11, Flow<T12> f12, Flow<T13> f13,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 13;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;
            var a8 = f8 ?? Flow<T8>.FromResult(default);
            T8 r8 = default;
            var a9 = f9 ?? Flow<T9>.FromResult(default);
            T9 r9 = default;
            var a10 = f10 ?? Flow<T10>.FromResult(default);
            T10 r10 = default;
            var a11 = f11 ?? Flow<T11>.FromResult(default);
            T11 r11 = default;
            var a12 = f12 ?? Flow<T12>.FromResult(default);
            T12 r12 = default;
            var a13 = f13 ?? Flow<T13>.FromResult(default);
            T13 r13 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done8()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a8.IsFaulted)
                {
                    try
                    {
                        a8.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a8.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r8 = a8.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done9()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a9.IsFaulted)
                {
                    try
                    {
                        a9.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a9.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r9 = a9.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done10()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a10.IsFaulted)
                {
                    try
                    {
                        a10.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a10.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r10 = a10.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done11()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a11.IsFaulted)
                {
                    try
                    {
                        a11.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a11.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r11 = a11.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done12()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a12.IsFaulted)
                {
                    try
                    {
                        a12.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a12.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r12 = a12.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            void Done13()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a13.IsFaulted)
                {
                    try
                    {
                        a13.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a13.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r13 = a13.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            a8.OnCompleted(Done8);
            a9.OnCompleted(Done9);
            a10.OnCompleted(Done10);
            a11.OnCompleted(Done11);
            a12.OnCompleted(Done12);
            a13.OnCompleted(Done13);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7, Flow<T8> f8, Flow<T9> f9, Flow<T10> f10, Flow<T11> f11, Flow<T12> f12, Flow<T13> f13, Flow<T14> f14,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 14;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;
            var a8 = f8 ?? Flow<T8>.FromResult(default);
            T8 r8 = default;
            var a9 = f9 ?? Flow<T9>.FromResult(default);
            T9 r9 = default;
            var a10 = f10 ?? Flow<T10>.FromResult(default);
            T10 r10 = default;
            var a11 = f11 ?? Flow<T11>.FromResult(default);
            T11 r11 = default;
            var a12 = f12 ?? Flow<T12>.FromResult(default);
            T12 r12 = default;
            var a13 = f13 ?? Flow<T13>.FromResult(default);
            T13 r13 = default;
            var a14 = f14 ?? Flow<T14>.FromResult(default);
            T14 r14 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done8()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a8.IsFaulted)
                {
                    try
                    {
                        a8.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a8.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r8 = a8.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done9()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a9.IsFaulted)
                {
                    try
                    {
                        a9.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a9.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r9 = a9.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done10()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a10.IsFaulted)
                {
                    try
                    {
                        a10.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a10.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r10 = a10.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done11()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a11.IsFaulted)
                {
                    try
                    {
                        a11.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a11.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r11 = a11.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done12()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a12.IsFaulted)
                {
                    try
                    {
                        a12.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a12.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r12 = a12.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done13()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a13.IsFaulted)
                {
                    try
                    {
                        a13.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a13.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r13 = a13.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            void Done14()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a14.IsFaulted)
                {
                    try
                    {
                        a14.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a14.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r14 = a14.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            a8.OnCompleted(Done8);
            a9.OnCompleted(Done9);
            a10.OnCompleted(Done10);
            a11.OnCompleted(Done11);
            a12.OnCompleted(Done12);
            a13.OnCompleted(Done13);
            a14.OnCompleted(Done14);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7, Flow<T8> f8, Flow<T9> f9, Flow<T10> f10, Flow<T11> f11, Flow<T12> f12, Flow<T13> f13, Flow<T14> f14, Flow<T15> f15,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 15;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;
            var a8 = f8 ?? Flow<T8>.FromResult(default);
            T8 r8 = default;
            var a9 = f9 ?? Flow<T9>.FromResult(default);
            T9 r9 = default;
            var a10 = f10 ?? Flow<T10>.FromResult(default);
            T10 r10 = default;
            var a11 = f11 ?? Flow<T11>.FromResult(default);
            T11 r11 = default;
            var a12 = f12 ?? Flow<T12>.FromResult(default);
            T12 r12 = default;
            var a13 = f13 ?? Flow<T13>.FromResult(default);
            T13 r13 = default;
            var a14 = f14 ?? Flow<T14>.FromResult(default);
            T14 r14 = default;
            var a15 = f15 ?? Flow<T15>.FromResult(default);
            T15 r15 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done8()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a8.IsFaulted)
                {
                    try
                    {
                        a8.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a8.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r8 = a8.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done9()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a9.IsFaulted)
                {
                    try
                    {
                        a9.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a9.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r9 = a9.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done10()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a10.IsFaulted)
                {
                    try
                    {
                        a10.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a10.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r10 = a10.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done11()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a11.IsFaulted)
                {
                    try
                    {
                        a11.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a11.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r11 = a11.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done12()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a12.IsFaulted)
                {
                    try
                    {
                        a12.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a12.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r12 = a12.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done13()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a13.IsFaulted)
                {
                    try
                    {
                        a13.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a13.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r13 = a13.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done14()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a14.IsFaulted)
                {
                    try
                    {
                        a14.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a14.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r14 = a14.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            void Done15()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a15.IsFaulted)
                {
                    try
                    {
                        a15.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a15.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r15 = a15.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            a8.OnCompleted(Done8);
            a9.OnCompleted(Done9);
            a10.OnCompleted(Done10);
            a11.OnCompleted(Done11);
            a12.OnCompleted(Done12);
            a13.OnCompleted(Done13);
            a14.OnCompleted(Done14);
            a15.OnCompleted(Done15);
            return parent;
        }
        public static Flow<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
            Flow<T1> f1, Flow<T2> f2, Flow<T3> f3, Flow<T4> f4, Flow<T5> f5, Flow<T6> f6, Flow<T7> f7, Flow<T8> f8, Flow<T9> f9, Flow<T10> f10, Flow<T11> f11, Flow<T12> f12, Flow<T13> f13, Flow<T14> f14, Flow<T15> f15, Flow<T16> f16,
            CancellationToken cancellationToken = default)
        {
            var parent = FlowPool.Rent<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)>();
            parent.AttachCancellation(cancellationToken);
            if (parent.IsCompleted)
            {
                return parent;
            }

            var remaining = 16;
            var a1 = f1 ?? Flow<T1>.FromResult(default);
            T1 r1 = default;
            var a2 = f2 ?? Flow<T2>.FromResult(default);
            T2 r2 = default;
            var a3 = f3 ?? Flow<T3>.FromResult(default);
            T3 r3 = default;
            var a4 = f4 ?? Flow<T4>.FromResult(default);
            T4 r4 = default;
            var a5 = f5 ?? Flow<T5>.FromResult(default);
            T5 r5 = default;
            var a6 = f6 ?? Flow<T6>.FromResult(default);
            T6 r6 = default;
            var a7 = f7 ?? Flow<T7>.FromResult(default);
            T7 r7 = default;
            var a8 = f8 ?? Flow<T8>.FromResult(default);
            T8 r8 = default;
            var a9 = f9 ?? Flow<T9>.FromResult(default);
            T9 r9 = default;
            var a10 = f10 ?? Flow<T10>.FromResult(default);
            T10 r10 = default;
            var a11 = f11 ?? Flow<T11>.FromResult(default);
            T11 r11 = default;
            var a12 = f12 ?? Flow<T12>.FromResult(default);
            T12 r12 = default;
            var a13 = f13 ?? Flow<T13>.FromResult(default);
            T13 r13 = default;
            var a14 = f14 ?? Flow<T14>.FromResult(default);
            T14 r14 = default;
            var a15 = f15 ?? Flow<T15>.FromResult(default);
            T15 r15 = default;
            var a16 = f16 ?? Flow<T16>.FromResult(default);
            T16 r16 = default;

            void Done1()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a1.IsFaulted)
                {
                    try
                    {
                        a1.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a1.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r1 = a1.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done2()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a2.IsFaulted)
                {
                    try
                    {
                        a2.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a2.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r2 = a2.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done3()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a3.IsFaulted)
                {
                    try
                    {
                        a3.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a3.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r3 = a3.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done4()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a4.IsFaulted)
                {
                    try
                    {
                        a4.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a4.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r4 = a4.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done5()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a5.IsFaulted)
                {
                    try
                    {
                        a5.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a5.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r5 = a5.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done6()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a6.IsFaulted)
                {
                    try
                    {
                        a6.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a6.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r6 = a6.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done7()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a7.IsFaulted)
                {
                    try
                    {
                        a7.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a7.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r7 = a7.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done8()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a8.IsFaulted)
                {
                    try
                    {
                        a8.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a8.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r8 = a8.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done9()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a9.IsFaulted)
                {
                    try
                    {
                        a9.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a9.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r9 = a9.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done10()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a10.IsFaulted)
                {
                    try
                    {
                        a10.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a10.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r10 = a10.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done11()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a11.IsFaulted)
                {
                    try
                    {
                        a11.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a11.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r11 = a11.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done12()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a12.IsFaulted)
                {
                    try
                    {
                        a12.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a12.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r12 = a12.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done13()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a13.IsFaulted)
                {
                    try
                    {
                        a13.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a13.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r13 = a13.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done14()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a14.IsFaulted)
                {
                    try
                    {
                        a14.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a14.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r14 = a14.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done15()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a15.IsFaulted)
                {
                    try
                    {
                        a15.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a15.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r15 = a15.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            void Done16()
            {
                if (parent.IsCompleted)
                {
                    return;
                }

                if (a16.IsFaulted)
                {
                    try
                    {
                        a16.GetResult();
                    }
                    catch (Exception ex)
                    {
                        parent.TrySetException(ex);
                    }

                    return;
                }

                if (a16.IsCanceled)
                {
                    parent.TrySetCanceled();
                    return;
                }

                r16 = a16.GetResult();
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    parent.TrySetResult((r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16));
                }
            }

            a1.OnCompleted(Done1);
            a2.OnCompleted(Done2);
            a3.OnCompleted(Done3);
            a4.OnCompleted(Done4);
            a5.OnCompleted(Done5);
            a6.OnCompleted(Done6);
            a7.OnCompleted(Done7);
            a8.OnCompleted(Done8);
            a9.OnCompleted(Done9);
            a10.OnCompleted(Done10);
            a11.OnCompleted(Done11);
            a12.OnCompleted(Done12);
            a13.OnCompleted(Done13);
            a14.OnCompleted(Done14);
            a15.OnCompleted(Done15);
            a16.OnCompleted(Done16);
            return parent;
        }
    }
}
