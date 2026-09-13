using System;
using System.Threading;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// <see cref="IProgress{T}"/> that marshals <see cref="Report"/> onto a Unity player-loop timing.
    /// </summary>
    public sealed class FlowProgress<T> : IProgress<T>
    {
        readonly Action<T> _handler;
        readonly PlayerLoopTiming _timing;

        public FlowProgress(Action<T> handler, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _timing = timing;
        }

        public void Report(T value)
        {
            var handler = _handler;
            var reported = value;
            if (FlowRunner.IsMainThread && _timing == PlayerLoopTiming.Update)
            {
                // Keep Report synchronous on main/Update so callers see immediate UI updates
                // when already on the right thread; other timings still go through the pump.
                try
                {
                    handler(reported);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }

                return;
            }

            FlowRunner.Post(() =>
            {
                try
                {
                    handler(reported);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }, _timing);
        }
    }

    static class NullProgress<T>
    {
        public static readonly IProgress<T> Instance = new NullProgressImpl();

        sealed class NullProgressImpl : IProgress<T>
        {
            public void Report(T value)
            {
            }
        }
    }

    public partial class Flow
    {
        /// <summary>
        /// Create a progress reporter that delivers callbacks on the Unity main player loop.
        /// </summary>
        public static IProgress<T> CreateProgress<T>(
            Action<T> handler,
            PlayerLoopTiming timing = PlayerLoopTiming.Update) =>
            new FlowProgress<T>(handler, timing);

        /// <summary>
        /// Create a void Flow with an optional progress channel (defaults to a no-op progress).
        /// </summary>
        public static Flow Create(
            Action<Flow, IProgress<float>> starter,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (starter == null)
            {
                throw new ArgumentNullException(nameof(starter));
            }

            var flow = FlowPool.RentVoid();
            flow.AttachCancellation(cancellationToken);
            if (flow.IsCompleted)
            {
                return flow;
            }

            try
            {
                starter(flow, progress ?? NullProgress<float>.Instance);
            }
            catch (Exception ex)
            {
                flow.TrySetException(ex);
            }

            return flow;
        }
    }

    public partial class Flow<T>
    {
        /// <summary>
        /// Create a typed Flow with an optional progress channel.
        /// </summary>
        public static Flow<T> Create(
            Action<Flow<T>, IProgress<float>> starter,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (starter == null)
            {
                throw new ArgumentNullException(nameof(starter));
            }

            var flow = FlowPool.Rent<T>();
            flow.AttachCancellation(cancellationToken);
            if (flow.IsCompleted)
            {
                return flow;
            }

            try
            {
                starter(flow, progress ?? NullProgress<float>.Instance);
            }
            catch (Exception ex)
            {
                flow.TrySetException(ex);
            }

            return flow;
        }
    }
}
