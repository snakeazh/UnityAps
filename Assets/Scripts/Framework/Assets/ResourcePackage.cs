using System;
using System.Collections;
using CoinFlip.FlowFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinFlip.Assets
{
    /// <summary>
    /// Package facade aligned with YooAsset <c>ResourcePackage</c>.
    /// Local backend: AddressCatalog + Resources + SceneManager.
    /// Define <c>YOOASSET</c> and drop in the official plugin to swap the backend.
    /// </summary>
    public sealed class ResourcePackage
    {
        AddressCatalog _catalog;
        bool _initialized;

        public string PackageName { get; }
        public bool InitializeStatus => _initialized;
        public EPlayMode PlayMode { get; private set; } = EPlayMode.OfflinePlayMode;

        internal ResourcePackage(string packageName)
        {
            PackageName = packageName;
        }

        public InitializationOperation InitializeAsync(ResourceInitParameters parameters = null)
        {
            var op = new InitializationOperation();
            parameters = parameters ?? new ResourceInitParameters();
            PlayMode = parameters.PlayMode;

            if (PlayMode == EPlayMode.HostPlayMode)
            {
                Debug.LogWarning(
                    "[ResourcePackage] HostPlayMode needs the YooAsset plugin. Falling back to OfflinePlayMode.");
                PlayMode = EPlayMode.OfflinePlayMode;
            }

            _catalog = Resources.Load<AddressCatalog>(parameters.CatalogResourcesPath);
            _initialized = true;
            op.Complete(true);
            Debug.Log($"[ResourcePackage] '{PackageName}' initialized ({PlayMode}).");
            return op;
        }

        public AssetHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object =>
            LoadAssetInternal(location, typeof(TObject));

        public AssetHandle LoadAssetSync(string location, Type type) =>
            LoadAssetInternal(location, type ?? typeof(UnityEngine.Object));

        public AssetHandle LoadAssetAsync<TObject>(string location, uint priority = 0)
            where TObject : UnityEngine.Object =>
            LoadAssetInternal(location, typeof(TObject));

        public AssetHandle LoadAssetAsync(string location, Type type, uint priority = 0) =>
            LoadAssetInternal(location, type ?? typeof(UnityEngine.Object));

        public AssetHandle LoadAssetAsync(string location, uint priority = 0) =>
            LoadAssetInternal(location, typeof(UnityEngine.Object));

        public SceneHandle LoadSceneAsync(
            string location,
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            bool allowSceneActivation = true,
            uint priority = 0)
        {
            CheckInitialized();
            var handle = new SceneHandle(location);
            var sceneName = ResolveSceneName(location);
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"[ResourcePackage] Scene location not found: '{location}'.");
                handle.Complete(null, false);
                return handle;
            }

            FlowRunner.StartRoutine(LoadSceneRoutine(handle, sceneName, sceneMode, allowSceneActivation));
            return handle;
        }

        public AssetInfo ConvertLocationToAssetInfo(string location, Type type)
        {
            AddressEntry entry = null;
            _catalog?.TryResolve(location, out entry);
            var path = entry != null && !string.IsNullOrEmpty(entry.assetPath)
                ? entry.assetPath
                : location;
            return new AssetInfo(location, path, type);
        }

        public void UnloadUnusedAssets() => Resources.UnloadUnusedAssets();

        AssetHandle LoadAssetInternal(string location, Type type)
        {
            CheckInitialized();
            var handle = new AssetHandle(location);
            var asset = LoadLocal(location, type);
            handle.Complete(asset, asset != null);
            if (asset == null)
            {
                Debug.LogWarning($"[ResourcePackage] Asset not found: '{location}' ({type.Name}).");
            }

            return handle;
        }

        UnityEngine.Object LoadLocal(string location, Type type)
        {
            if (_catalog != null && _catalog.TryResolve(location, out var entry))
            {
                if (!string.IsNullOrEmpty(entry.resourcesPath))
                {
                    var mapped = Resources.Load(entry.resourcesPath, type);
                    if (mapped != null)
                    {
                        return mapped;
                    }
                }
            }

            var direct = Resources.Load(location, type);
            if (direct != null)
            {
                return direct;
            }

            var fileName = System.IO.Path.GetFileNameWithoutExtension(location);
            return string.IsNullOrEmpty(fileName) ? null : Resources.Load(fileName, type);
        }

        string ResolveSceneName(string location)
        {
            if (_catalog != null && _catalog.TryResolve(location, out var entry) &&
                !string.IsNullOrEmpty(entry.sceneName))
            {
                return entry.sceneName;
            }

            if (Application.CanStreamedLevelBeLoaded(location))
            {
                return location;
            }

            var fileName = System.IO.Path.GetFileNameWithoutExtension(location);
            if (!string.IsNullOrEmpty(fileName) && Application.CanStreamedLevelBeLoaded(fileName))
            {
                return fileName;
            }

            return null;
        }

        static IEnumerator LoadSceneRoutine(
            SceneHandle handle,
            string sceneName,
            LoadSceneMode sceneMode,
            bool allowSceneActivation)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, sceneMode);
            if (op == null)
            {
                handle.Complete(sceneName, false);
                yield break;
            }

            op.allowSceneActivation = allowSceneActivation;
            while (!op.isDone)
            {
                yield return null;
            }

            handle.Complete(sceneName, true);
        }

        void CheckInitialized()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException(
                    $"Package '{PackageName}' is not initialized. Call InitializeAsync first.");
            }
        }
    }
}
