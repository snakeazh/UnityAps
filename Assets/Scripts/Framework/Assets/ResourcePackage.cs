using System;
using System.Collections;
using CoinFlip.FlowFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinFlip.Assets
{
    /// <summary>
    /// Package facade aligned with YooAsset <c>ResourcePackage</c>.
    /// Local backend loads <b>only</b> files under <see cref="ResRoot.Folder"/>.
    /// Scenes still go through Build Settings by location.
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

            _catalog = LoadCatalog(parameters.CatalogAssetPath);
            _initialized = true;
            op.Complete(true);
            Debug.Log($"[ResourcePackage] '{PackageName}' initialized ({PlayMode}), root={ResRoot.Folder}.");
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
            if (!_catalog.TryResolve(location, out var entry) ||
                string.IsNullOrEmpty(entry.assetPath))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(entry.sceneName))
            {
                return null;
            }

            return LoadResAsset(entry.assetPath, type);
        }

        static AddressCatalog LoadCatalog(string catalogAssetPath)
        {
            var path = string.IsNullOrEmpty(catalogAssetPath)
                ? ResRoot.CatalogAssetPath
                : catalogAssetPath;
            if (!ResRoot.Contains(path))
            {
                Debug.LogError($"[ResourcePackage] Catalog must live under {ResRoot.Folder}: {path}");
                return AddressCatalog.CreateBuiltin();
            }

            var catalog = LoadResAsset(path, typeof(AddressCatalog)) as AddressCatalog;
            return catalog != null ? catalog : AddressCatalog.CreateBuiltin();
        }

        static UnityEngine.Object LoadResAsset(string assetPath, Type type)
        {
            var path = ResRoot.Normalize(assetPath);
            if (!ResRoot.Contains(path))
            {
                Debug.LogWarning($"[ResourcePackage] Refused path outside {ResRoot.Folder}: {path}");
                return null;
            }

#if UNITY_EDITOR
            var fromEditor = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
            if (fromEditor != null)
            {
                return fromEditor;
            }
#endif

            // Player / fallback: only Resources keys that map to Assets/Res (Resources/Res/...).
            if (ResRoot.TryToResourcesKey(path, out var key))
            {
                return Resources.Load(key, type);
            }

            return null;
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
