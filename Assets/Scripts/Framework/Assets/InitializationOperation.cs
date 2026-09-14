using System;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>Package init / update operation (Observer + awaitable).</summary>
    public sealed class InitializationOperation : CustomYieldInstruction
    {
        Action<InitializationOperation> _completed;

        public bool IsDone { get; private set; }
        public bool StatusIsSucceed { get; private set; } = true;
        public string Error { get; private set; } = string.Empty;
        public float Progress { get; private set; }
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

        internal void SetProgress(float value) => Progress = Mathf.Clamp01(value);

        internal void Complete(bool success, string error = null)
        {
            if (IsDone)
            {
                return;
            }

            StatusIsSucceed = success;
            Error = error ?? string.Empty;
            Progress = success ? 1f : Progress;
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
