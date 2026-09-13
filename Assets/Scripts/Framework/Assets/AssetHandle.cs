using System;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>
    /// Load handle aligned with YooAsset <c>AssetHandle</c>:
    /// Completed / yield / await / AssetObject / Release / InstantiateSync.
    /// </summary>
    public sealed class AssetHandle : CustomYieldInstruction
    {
        Action<AssetHandle> _completed;
        bool _released;

        public bool IsDone { get; private set; }
        public bool LastOperationSucceed { get; private set; } = true;
        public string Location { get; }
        public UnityEngine.Object AssetObject { get; private set; }
        public override bool keepWaiting => !IsDone;

        public event Action<AssetHandle> Completed
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

        internal AssetHandle(string location)
        {
            Location = location ?? string.Empty;
        }

        internal void Complete(UnityEngine.Object asset, bool success)
        {
            if (IsDone)
            {
                return;
            }

            AssetObject = asset;
            LastOperationSucceed = success && asset != null;
            IsDone = true;
            _completed?.Invoke(this);
        }

        public TObject GetAssetObject<TObject>() where TObject : UnityEngine.Object =>
            AssetObject as TObject;

        public GameObject InstantiateSync(Transform parent = null, bool worldPositionStays = false)
        {
            var prefab = AssetObject as GameObject;
            if (prefab == null)
            {
                Debug.LogError($"[AssetHandle] InstantiateSync failed for '{Location}'.");
                return null;
            }

            return UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays);
        }

        public void Release()
        {
            if (_released)
            {
                return;
            }

            _released = true;
            AssetObject = null;
        }

        public Flow ToFlow()
        {
            if (IsDone)
            {
                return LastOperationSucceed
                    ? Flow.Completed()
                    : Flow.FromException(new InvalidOperationException($"Load failed: {Location}"));
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
                        flow.TrySetException(new InvalidOperationException($"Load failed: {Location}"));
                    }
                };
            });
        }

        public FlowAwaiter GetAwaiter() => ToFlow().GetAwaiter();
    }
}
