using System;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>Scene load handle aligned with YooAsset <c>SceneHandle</c>.</summary>
    public sealed class SceneHandle : CustomYieldInstruction
    {
        Action<SceneHandle> _completed;

        public bool IsDone { get; private set; }
        public bool LastOperationSucceed { get; private set; } = true;
        public string Location { get; }
        public string SceneName { get; private set; }
        public override bool keepWaiting => !IsDone;

        public event Action<SceneHandle> Completed
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

        internal SceneHandle(string location)
        {
            Location = location ?? string.Empty;
        }

        internal void Complete(string sceneName, bool success)
        {
            if (IsDone)
            {
                return;
            }

            SceneName = sceneName;
            LastOperationSucceed = success;
            IsDone = true;
            _completed?.Invoke(this);
        }

        public Flow ToFlow()
        {
            if (IsDone)
            {
                return LastOperationSucceed
                    ? Flow.Completed()
                    : Flow.FromException(new InvalidOperationException($"Scene load failed: {Location}"));
            }

            return Flow.Create(flow =>
            {
                Completed += _ =>
                {
                    if (LastOperationSucceed)
                    {
                        flow.TrySetResult();
                    }
                    else
                    {
                        flow.TrySetException(new InvalidOperationException($"Scene load failed: {Location}"));
                    }
                };
            });
        }

        public FlowAwaiter GetAwaiter() => ToFlow().GetAwaiter();
    }
}
