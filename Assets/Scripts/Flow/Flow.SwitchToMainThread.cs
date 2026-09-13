using System;
using System.Runtime.CompilerServices;

namespace CoinFlip.FlowFramework
{
    /// <summary>
    /// Await to resume on the Unity main thread (no-op if already there).
    /// <code>await Flow.SwitchToMainThread();</code>
    /// </summary>
    public readonly struct SwitchToMainThreadAwaitable
    {
        public SwitchToMainThreadAwaiter GetAwaiter() => new SwitchToMainThreadAwaiter();
    }

    public readonly struct SwitchToMainThreadAwaiter : INotifyCompletion
    {
        public bool IsCompleted => FlowRunner.IsMainThread;

        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            FlowRunner.Post(continuation);
        }

        public void GetResult()
        {
        }
    }

    public partial class Flow
    {
        /// <summary>
        /// Switch execution to the Unity main thread.
        /// If already on main, the await completes synchronously (<see cref="SwitchToMainThreadAwaiter.IsCompleted"/>).
        /// </summary>
        public static SwitchToMainThreadAwaitable SwitchToMainThread() => default;
    }
}
