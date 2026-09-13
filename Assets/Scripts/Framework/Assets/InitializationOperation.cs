using System;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>Package init operation, aligned with YooAsset <c>InitializationOperation</c>.</summary>
    public sealed class InitializationOperation : CustomYieldInstruction
    {
        Action<InitializationOperation> _completed;

        public bool IsDone { get; private set; }
        public bool StatusIsSucceed { get; private set; } = true;
        public string Error { get; private set; } = string.Empty;
        public override bool keepWaiting => !IsDone;

        public event Action<InitializationOperation> Completed
        {
            add
            {
                _completed += value;
                if (IsDone && value != null)
                {
                    value(this);
                }
            }
            remove => _completed -= value;
        }

        internal void Complete(bool success, string error = null)
        {
            if (IsDone)
            {
                return;
            }

            StatusIsSucceed = success;
            Error = error ?? string.Empty;
            IsDone = true;
            _completed?.Invoke(this);
        }

        public Flow ToFlow()
        {
            if (IsDone)
            {
                return StatusIsSucceed
                    ? Flow.Completed()
                    : Flow.FromException(new InvalidOperationException(Error));
            }

            return Flow.Create(flow =>
            {
                Completed += _ =>
                {
                    if (StatusIsSucceed)
                    {
                        flow.TrySetResult();
                    }
                    else
                    {
                        flow.TrySetException(new InvalidOperationException(Error));
                    }
                };
            });
        }

        public FlowAwaiter GetAwaiter() => ToFlow().GetAwaiter();
    }
}
