using System;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip.Assets
{
    public sealed class SubAssetsHandle : CustomYieldInstruction
    {
        Action<SubAssetsHandle> _completed;
        int _refCount = 1;

        public bool IsDone { get; private set; }
        public bool LastOperationSucceed { get; private set; } = true;
        public string Location { get; }
        public string Error { get; private set; } = string.Empty;
        public UnityEngine.Object[] SubAssetObjects { get; private set; }
        public override bool keepWaiting => !IsDone;

        public event Action<SubAssetsHandle> Completed
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

        internal SubAssetsHandle(string location)
        {
            Location = location ?? string.Empty;
        }

        internal void Complete(UnityEngine.Object[] assets, bool success, string error = null)
        {
            if (IsDone)
            {
                return;
            }

            SubAssetObjects = assets;
            LastOperationSucceed = success && assets != null;
            Error = LastOperationSucceed ? string.Empty : (error ?? "LoadSubAssets failed");
            IsDone = true;
            _completed?.Invoke(this);
        }

        public TObject[] GetSubAssetObjects<TObject>() where TObject : UnityEngine.Object
        {
            if (SubAssetObjects == null)
            {
                return Array.Empty<TObject>();
            }

            var list = new System.Collections.Generic.List<TObject>();
            for (var i = 0; i < SubAssetObjects.Length; i++)
            {
                if (SubAssetObjects[i] is TObject typed)
                {
                    list.Add(typed);
                }
            }

            return list.ToArray();
        }

        public void Retain() => _refCount++;

        public void Release()
        {
            _refCount--;
            if (_refCount <= 0)
            {
                SubAssetObjects = null;
            }
        }

        public Flow ToFlow()
        {
            if (IsDone)
            {
                return LastOperationSucceed
                    ? Flow.Completed()
                    : Flow.FromException(new InvalidOperationException(Error));
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
                        flow.TrySetException(new InvalidOperationException(Error));
                    }
                };
            });
        }

        public FlowAwaiter GetAwaiter() => ToFlow().GetAwaiter();
    }
}
