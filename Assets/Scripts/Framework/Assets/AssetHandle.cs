using System;
using CoinFlip.FlowFramework;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>
    /// Load handle: Completed / yield / await / Retain-Release / Progress (Observer + ref-count).
    /// </summary>
    public sealed class AssetHandle : CustomYieldInstruction
    {
        Action<AssetHandle> _completed;
        int _refCount = 1;
        bool _released;

        public bool IsDone { get; private set; }
        public bool LastOperationSucceed { get; private set; } = true;
        public string Location { get; }
        public string Error { get; private set; } = string.Empty;
        public float Progress { get; private set; }
        public UnityEngine.Object AssetObject { get; private set; }
        public string BundleName { get; private set; }
        public int RefCount => _refCount;
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

        internal void SetBundleName(string bundleName) => BundleName = bundleName;

        internal void SetProgress(float progress) => Progress = Mathf.Clamp01(progress);

        internal void Complete(UnityEngine.Object asset, bool success, string error = null)
        {
            if (IsDone)
            {
                return;
            }

            AssetObject = asset;
            LastOperationSucceed = success && asset != null;
            Error = LastOperationSucceed ? string.Empty : (error ?? "Load failed");
            Progress = 1f;
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

        public void Retain()
        {
            if (_released)
            {
                return;
            }

            _refCount++;
        }

        public void Release()
        {
            if (_released)
            {
                return;
            }

            _refCount--;
            if (_refCount > 0)
            {
                return;
            }

            _released = true;
            AssetObject = null;
        }

        internal bool IsReleased => _released;

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
