using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Player loop slot used to pump Flow continuations (UniTask-style timing).
    /// </summary>
    public enum PlayerLoopTiming
    {
        Update = 0,
        FixedUpdate = 1,
        LateUpdate = 2,
        EndOfFrame = 3,
    }

    /// <summary>
    /// Await the next tick of the given player-loop timing.
    /// <code>await Flow.Yield(PlayerLoopTiming.EndOfFrame);</code>
    /// </summary>
    public readonly struct PlayerLoopTimingAwaitable
    {
        readonly PlayerLoopTiming _timing;

        public PlayerLoopTimingAwaitable(PlayerLoopTiming timing)
        {
            _timing = timing;
        }

        public PlayerLoopTimingAwaiter GetAwaiter() => new PlayerLoopTimingAwaiter(_timing);
    }

    public readonly struct PlayerLoopTimingAwaiter : ICriticalNotifyCompletion
    {
        readonly PlayerLoopTiming _timing;

        public PlayerLoopTimingAwaiter(PlayerLoopTiming timing)
        {
            _timing = timing;
        }

        /// <summary>Always false so callers wait at least one timed tick.</summary>
        public bool IsCompleted => false;

        public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            FlowRunner.Post(continuation, _timing);
        }

        public void GetResult()
        {
        }
    }

    public partial class Flow
    {
        /// <summary>Resume on the next Unity player-loop tick of <paramref name="timing"/>.</summary>
        public static PlayerLoopTimingAwaitable Yield(PlayerLoopTiming timing = PlayerLoopTiming.Update) =>
            new PlayerLoopTimingAwaitable(timing);

        /// <summary>
        /// Completes on the next tick of <paramref name="timing"/> (same idea as <see cref="Yield"/>,
        /// but returns a <see cref="Flow"/> for chaining / <c>ToYieldInstruction</c>).
        /// </summary>
        public static Flow NextFrame(
            PlayerLoopTiming timing,
            CancellationToken cancellationToken = default)
        {
            var flow = FlowPool.RentVoid();
            flow.AttachCancellation(cancellationToken);
            if (flow.IsCompleted)
            {
                return flow;
            }

            FlowRunner.Post(() =>
            {
                if (!flow.IsCompleted)
                {
                    flow.TrySetResult();
                }
            }, timing);
            return flow;
        }
    }
}

