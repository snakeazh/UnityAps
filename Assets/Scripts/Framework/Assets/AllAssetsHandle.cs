using System;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip.Assets
{
    public sealed class AllAssetsHandle : CustomYieldInstruction
    {
        Action<AllAssetsHandle> _completed;
        int _refCount = 1;

        public bool IsDone { get; private set; }
        public bool LastOperationSucceed { get; private set; } = true;
        public string Location { get; }
        public string Error { get; private set; } = string.Empty;
        public float Progress { get; private set; }
        public UnityEngine.Object[] AllAssetObjects { get; private set; }
        public override bool keepWaiting => !IsDone;

        public event Action<AllAssetsHandle> Completed
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

        internal AllAssetsHandle(string location)
        {
            Location = location ?? string.Empty;
        }

        internal void Complete(UnityEngine.Object[] assets, bool success, string error = null)
        {
            if (IsDone)
            {
                return;
            }

            AllAssetObjects = assets;
            LastOperationSucceed = success && assets != null;
            Error = LastOperationSucceed ? string.Empty : (error ?? "LoadAllAssets failed");
            Progress = 1f;
            IsDone = true;
            _completed?.Invoke(this);
        }

        public TObject[] GetAssetObjects<TObject>() where TObject : UnityEngine.Object
        {
            if (AllAssetObjects == null)
            {
                return Array.Empty<TObject>();
            }

            var list = new System.Collections.Generic.List<TObject>(AllAssetObjects.Length);
            for (var i = 0; i < AllAssetObjects.Length; i++)
            {
                if (AllAssetObjects[i] is TObject typed)
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
                AllAssetObjects = null;
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
