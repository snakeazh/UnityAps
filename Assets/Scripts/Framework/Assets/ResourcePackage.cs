using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CoinFlip.FlowFramework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace CoinFlip.Assets
{
    /// <summary>
    /// Package facade (DIP): depends on IBundleFileSystem / IDecryptionServices, not concrete platforms.
    /// </summary>
    public sealed class ResourcePackage
    {
        readonly Dictionary<string, AssetBundle> _loadedBundles =
            new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, AssetHandle> _assetHandles =
            new Dictionary<string, AssetHandle>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, int> _bundleRefCounts =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        AddressCatalog _catalog;
        FirstPackageManifest _firstPackage;
        ResourceSettings _settings;
        IBundleFileSystem _fileSystem;
        IDecryptionServices _decryption;
        VersionManifest _remoteVersion;
        VersionManifest _localVersion;
        bool _initialized;

        public string PackageName { get; }
        public bool InitializeStatus => _initialized;
        public EPlayMode PlayMode { get; private set; } = EPlayMode.OfflinePlayMode;
        public string PackageVersion => _localVersion?.version ?? _settings?.packageVersion ?? "0";
        public ResourceSettings Settings => _settings;
        public AddressCatalog Catalog => _catalog;

        internal ResourcePackage(string packageName)
        {
            PackageName = packageName;
        }

        public InitializationOperation InitializeAsync(ResourceInitParameters parameters = null)
        {
            var op = new InitializationOperation();
            FlowRunner.StartRoutine(InitializeRoutine(op, parameters ?? new ResourceInitParameters()));
            return op;
        }

        IEnumerator InitializeRoutine(InitializationOperation op, ResourceInitParameters parameters)
        {
            _fileSystem = parameters.FileSystem ??
                          BundleFileSystemFactory.Create(
                              parameters.Settings != null ? parameters.Settings.cacheRoot : "BundleCache");

            _settings = parameters.Settings ?? LoadSettingsAsset(parameters.SettingsAssetPath);
            if (_settings == null)
            {
                yield return LoadBootstrapIntoSettings();
            }

            if (parameters.PlayModeSpecified)
            {
                PlayMode = parameters.PlayMode;
            }
            else if (_settings != null)
            {
                PlayMode = _settings.ResolvePlayMode();
            }
            else
            {
                PlayMode = Application.isEditor ? EPlayMode.EditorSimulateMode : EPlayMode.OfflinePlayMode;
            }

            if (_fileSystem == null)
            {
                _fileSystem = BundleFileSystemFactory.Create(_settings != null ? _settings.cacheRoot : "BundleCache");
            }

            _decryption = parameters.Decryption ?? DecryptionServicesFactory.Create(_settings);

            var catalogPath = !string.IsNullOrEmpty(parameters.CatalogAssetPath)
                ? parameters.CatalogAssetPath
                : (_settings != null ? _settings.catalogAssetPath : ResRoot.CatalogAssetPath);
            _catalog = LoadCatalog(catalogPath);
            if (_catalog == null || _catalog.entries == null || _catalog.entries.Count == 0)
            {
                yield return LoadBootstrapCatalogIfNeeded();
            }

            _firstPackage = LoadFirstPackage(
                _settings != null ? _settings.firstPackageManifestPath : ResRoot.Folder + "/FirstPackageManifest.asset");

            _localVersion = new VersionManifest
            {
                version = _settings != null ? _settings.packageVersion : "1.0.0"
            };
            yield return TryLoadLocalVersionManifest();

            _initialized = true;
            op.Complete(true);
            Debug.Log($"[ResourcePackage] '{PackageName}' initialized ({PlayMode}).");
        }

        IEnumerator LoadBootstrapIntoSettings()
        {
            byte[] bytes = null;
            string err = null;
            yield return _fileSystem.ReadAllBytesAsync(
                BootstrapManifest.StreamingRelativePath,
                (b, e) =>
                {
                    bytes = b;
                    err = e;
                });
            if (bytes == null || bytes.Length == 0)
            {
                if (!string.IsNullOrEmpty(err))
                {
                    Debug.LogWarning("[ResourcePackage] bootstrap.json missing: " + err);
                }

                yield break;
            }

            var bootstrap = BootstrapManifest.FromJson(System.Text.Encoding.UTF8.GetString(bytes));
            _settings = ResourceSettings.CreateDefaultInstance();
            _settings.packageVersion = bootstrap.packageVersion;
            _settings.editorPlayMode = (EPlayMode)bootstrap.editorPlayMode;
            _settings.runtimePlayMode = (EPlayMode)bootstrap.runtimePlayMode;
            _settings.remoteRootUrl = bootstrap.remoteRootUrl;
            _settings.cacheRoot = string.IsNullOrEmpty(bootstrap.cacheRoot) ? "BundleCache" : bootstrap.cacheRoot;
            _settings.firstPackageTags = bootstrap.firstPackageTags;
            _settings.enableEncryption = bootstrap.enableEncryption;
            _settings.decryptionType = (EDecryptionType)bootstrap.decryptionType;
            _settings.encryptionOffset = bootstrap.encryptionOffset;
            _settings.xorKey = bootstrap.xorKey;
        }

        IEnumerator LoadBootstrapCatalogIfNeeded()
        {
            if (_catalog != null && _catalog.entries != null && _catalog.entries.Count > 0)
            {
                yield break;
            }

            byte[] bytes = null;
            yield return _fileSystem.ReadAllBytesAsync(
                BootstrapManifest.StreamingRelativePath,
                (b, e) => { bytes = b; });
            if (bytes == null)
            {
                _catalog = AddressCatalog.CreateBuiltin();
                yield break;
            }

            var bootstrap = BootstrapManifest.FromJson(System.Text.Encoding.UTF8.GetString(bytes));
            ApplyBootstrapEntries(bootstrap);
        }

        void ApplyBootstrapEntries(BootstrapManifest bootstrap)
        {
            if (bootstrap?.entries == null)
            {
                return;
            }

            if (_catalog == null)
            {
                _catalog = ScriptableObject.CreateInstance<AddressCatalog>();
                _catalog.name = "AddressCatalog (Bootstrap)";
            }

            _catalog.entries = new List<AddressEntry>();
            for (var i = 0; i < bootstrap.entries.Count; i++)
            {
                var e = bootstrap.entries[i];
                if (e == null)
                {
                    continue;
                }

                _catalog.entries.Add(new AddressEntry
                {
                    location = e.location,
                    assetPath = e.assetPath,
                    sceneName = e.sceneName,
                    bundleName = e.bundleName,
                    tags = string.IsNullOrEmpty(e.tags)
                        ? Array.Empty<string>()
                        : e.tags.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                    isFirstPackage = e.isFirstPackage,
                    isRawFile = e.isRawFile,
                    rawFileName = e.rawFileName
                });
            }
        }

        IEnumerator TryLoadLocalVersionManifest()
        {
            byte[] bytes = null;
            yield return _fileSystem.ReadAllBytesAsync(
                StreamingBundlePaths.Relative("version.json"),
                (b, e) => { bytes = b; });
            if (bytes == null || bytes.Length == 0)
            {
                yield break;
            }

            _localVersion = VersionManifest.FromJson(System.Text.Encoding.UTF8.GetString(bytes));
        }

        public AssetHandle LoadAssetAsync<TObject>(string location, uint priority = 0)
            where TObject : UnityEngine.Object =>
            LoadAssetAsync(location, typeof(TObject), priority);

        public AssetHandle LoadAssetAsync(string location, uint priority = 0) =>
            LoadAssetAsync(location, typeof(UnityEngine.Object), priority);

        public AssetHandle LoadAssetAsync(string location, Type type, uint priority = 0)
        {
            CheckInitialized();
            type = type ?? typeof(UnityEngine.Object);

            if (_assetHandles.TryGetValue(location, out var existing) &&
                existing != null && !existing.IsReleased && existing.IsDone && existing.LastOperationSucceed)
            {
                existing.Retain();
                return existing;
            }

            var handle = new AssetHandle(location);
            _assetHandles[location] = handle;
            FlowRunner.StartRoutine(LoadAssetRoutine(handle, location, type));
            return handle;
        }

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
                handle.Complete(null, false);
                return handle;
            }

            FlowRunner.StartRoutine(LoadSceneRoutine(handle, sceneName, sceneMode, allowSceneActivation));
            return handle;
        }

        public RawFileHandle LoadRawFileAsync(string location)
        {
            CheckInitialized();
            var handle = new RawFileHandle(location);
            FlowRunner.StartRoutine(LoadRawRoutine(handle, location));
            return handle;
        }

        public AllAssetsHandle LoadAllAssetsAsync(string locationOrBundle)
        {
            CheckInitialized();
            var handle = new AllAssetsHandle(locationOrBundle);
            FlowRunner.StartRoutine(LoadAllRoutine(handle, locationOrBundle));
            return handle;
        }

        public SubAssetsHandle LoadSubAssetsAsync<TObject>(string location) where TObject : UnityEngine.Object
        {
            CheckInitialized();
            var handle = new SubAssetsHandle(location);
            FlowRunner.StartRoutine(LoadSubRoutine(handle, location, typeof(TObject)));
            return handle;
        }

        public SubAssetsHandle LoadSubAssetsAsync(string location) =>
            LoadSubAssetsAsync<UnityEngine.Object>(location);

        public InitializationOperation UpdatePackageAsync()
        {
            CheckInitialized();
            var op = new InitializationOperation();
            if (PlayMode != EPlayMode.HostPlayMode)
            {
                op.Complete(true);
                return op;
            }

            FlowRunner.StartRoutine(UpdatePackageRoutine(op));
            return op;
        }

        public InitializationOperation CheckForUpdatesAsync(Action<bool, long> onResult = null)
        {
            CheckInitialized();
            var op = new InitializationOperation();
            FlowRunner.StartRoutine(CheckUpdatesRoutine(op, onResult));
            return op;
        }

        public InitializationOperation GetDownloadSizeAsync(string tags, Action<long> onSize = null)
        {
            CheckInitialized();
            var op = new InitializationOperation();
            FlowRunner.StartRoutine(GetDownloadSizeRoutine(op, tags, onSize));
            return op;
        }

        public ResourceDownloader DownloadBundlesAsync(string tags)
        {
            CheckInitialized();
            var pending = CollectPendingByTags(tags);
            var remote = _settings != null ? _settings.ResolveRemoteRootUrl() : string.Empty;
            var downloader = new ResourceDownloader(
                _fileSystem,
                remote,
                pending,
                _settings != null ? _settings.downloadRetryCount : 3,
                _settings != null ? _settings.downloadTimeoutSeconds : 30f);
            downloader.Begin();
            return downloader;
        }

        public ResourceDownloader DownloadBundlesAsync(IList<string> bundleNames)
        {
            CheckInitialized();
            var pending = new List<VersionBundleInfo>();
            if (bundleNames != null && _remoteVersion != null)
            {
                for (var i = 0; i < bundleNames.Count; i++)
                {
                    var info = _remoteVersion.Find(bundleNames[i]);
                    if (info != null)
                    {
                        pending.Add(info);
                    }
                }
            }

            var remote = _settings != null ? _settings.ResolveRemoteRootUrl() : string.Empty;
            var downloader = new ResourceDownloader(
                _fileSystem,
                remote,
                pending,
                _settings != null ? _settings.downloadRetryCount : 3,
                _settings != null ? _settings.downloadTimeoutSeconds : 30f);
            downloader.Begin();
            return downloader;
        }

        public ResourceDownloader CreateDownloader(string tags) => DownloadBundlesAsync(tags);

        public InitializationOperation PreloadFirstPackageAsync()
        {
            CheckInitialized();
            var op = new InitializationOperation();
            FlowRunner.StartRoutine(PreloadFirstRoutine(op));
            return op;
        }

        public InitializationOperation ClearCacheAsync()
        {
            CheckInitialized();
            var op = new InitializationOperation();
            FlowRunner.StartRoutine(ClearCacheRoutine(op, unusedOnly: false));
            return op;
        }

        public InitializationOperation ClearUnusedCacheAsync()
        {
            CheckInitialized();
            var op = new InitializationOperation();
            FlowRunner.StartRoutine(ClearCacheRoutine(op, unusedOnly: true));
            return op;
        }

        public string GetPackageVersion() => PackageVersion;

        public bool HasAsset(string location) => _catalog != null && _catalog.HasAsset(location);

        public bool CheckLocationValid(string location) => HasAsset(location);

        public bool IsFirstPackage(string location)
        {
            if (_firstPackage != null && _firstPackage.ContainsLocation(location))
            {
                return true;
            }

            return _catalog != null &&
                   _catalog.TryResolve(location, out var entry) &&
                   entry.isFirstPackage;
        }

        public bool IsBundleReady(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                return true;
            }

            if (_loadedBundles.ContainsKey(bundleName))
            {
                return true;
            }

            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                return true;
            }

            // Sync-friendly probe via cache/streaming path helpers used by FS implementations.
            var cacheRoot = _fileSystem != null ? _fileSystem.GetCacheRootPath() : null;
            if (!string.IsNullOrEmpty(cacheRoot))
            {
                var cachePath = Path.Combine(cacheRoot, bundleName);
                if (File.Exists(cachePath) || File.Exists(Path.Combine(cacheRoot, StreamingBundlePaths.Relative(bundleName))))
                {
                    return true;
                }
            }

#if !UNITY_ANDROID || UNITY_EDITOR
            var streaming = Path.Combine(Application.streamingAssetsPath, StreamingBundlePaths.Relative(bundleName));
            if (File.Exists(streaming))
            {
                return true;
            }
#endif
            return false;
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

        public AssetInfo[] GetAssetInfosByTag(string tag) =>
            _catalog != null
                ? _catalog.GetAssetInfosByTag(tag, typeof(UnityEngine.Object))
                : Array.Empty<AssetInfo>();

        public void UnloadUnusedAssets()
        {
            var deadKeys = new List<string>();
            foreach (var kv in _assetHandles)
            {
                if (kv.Value == null || kv.Value.IsReleased || kv.Value.RefCount <= 0)
                {
                    deadKeys.Add(kv.Key);
                }
            }

            for (var i = 0; i < deadKeys.Count; i++)
            {
                _assetHandles.Remove(deadKeys[i]);
            }

            var bundles = new List<string>(_bundleRefCounts.Keys);
            for (var i = 0; i < bundles.Count; i++)
            {
                var name = bundles[i];
                if (_bundleRefCounts.TryGetValue(name, out var c) && c <= 0)
                {
                    UnloadBundle(name, force: true);
                }
            }

            Resources.UnloadUnusedAssets();
        }

        public void UnloadBundle(string bundleName, bool force = false)
        {
            if (string.IsNullOrEmpty(bundleName) || !_loadedBundles.TryGetValue(bundleName, out var ab))
            {
                return;
            }

            if (!force && _bundleRefCounts.TryGetValue(bundleName, out var c) && c > 0)
            {
                return;
            }

            ab.Unload(false);
            _loadedBundles.Remove(bundleName);
            _bundleRefCounts.Remove(bundleName);
        }

        IEnumerator LoadAssetRoutine(AssetHandle handle, string location, Type type)
        {
            if (!_catalog.TryResolve(location, out var entry))
            {
                handle.Complete(null, false, "Location not found");
                yield break;
            }

            handle.SetBundleName(entry.bundleName);

            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                var asset = LoadEditorOrResources(entry.assetPath, type);
                handle.Complete(asset, asset != null, asset == null ? "Editor load failed" : null);
                yield break;
            }

            if (!string.IsNullOrEmpty(entry.bundleName))
            {
                yield return EnsureBundleLoaded(entry.bundleName, err =>
                {
                    if (!string.IsNullOrEmpty(err))
                    {
                        handle.Complete(null, false, err);
                    }
                });
                if (handle.IsDone)
                {
                    yield break;
                }

                if (_loadedBundles.TryGetValue(entry.bundleName, out var ab))
                {
                    var req = ab.LoadAssetAsync(entry.location, type);
                    while (!req.isDone)
                    {
                        handle.SetProgress(req.progress);
                        yield return null;
                    }

                    RetainBundle(entry.bundleName);
                    handle.Complete(req.asset, req.asset != null);
                    yield break;
                }
            }

            var fallback = LoadEditorOrResources(entry.assetPath, type);
            handle.Complete(fallback, fallback != null, fallback == null ? "Asset not found" : null);
        }

        IEnumerator LoadRawRoutine(RawFileHandle handle, string location)
        {
            if (!_catalog.TryResolve(location, out var entry))
            {
                handle.Complete(null, null, false, "Location not found");
                yield break;
            }

            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
#if UNITY_EDITOR
                try
                {
                    var bytes = File.ReadAllBytes(entry.assetPath);
                    handle.Complete(bytes, entry.assetPath, true);
                }
                catch (Exception ex)
                {
                    handle.Complete(null, null, false, ex.Message);
                }
#else
                handle.Complete(null, null, false, "EditorSimulate only in editor");
#endif
                yield break;
            }

            var relative = !string.IsNullOrEmpty(entry.rawFileName)
                ? entry.rawFileName
                : (!string.IsNullOrEmpty(entry.bundleName)
                    ? entry.bundleName
                    : Path.GetFileName(entry.assetPath));
            relative = StreamingBundlePaths.Relative(relative);
            byte[] data = null;
            string err = null;
            yield return _fileSystem.ReadAllBytesAsync(relative, (b, e) =>
            {
                data = b;
                err = e;
            });
            if (data != null && _decryption != null && !(_decryption is NullDecryptionServices) && entry.isRawFile)
            {
                data = _decryption.DecryptData(data);
            }

            handle.Complete(data, relative, data != null, err);
        }

        IEnumerator LoadAllRoutine(AllAssetsHandle handle, string locationOrBundle)
        {
            string bundleName = locationOrBundle;
            if (_catalog.TryResolve(locationOrBundle, out var entry) && !string.IsNullOrEmpty(entry.bundleName))
            {
                bundleName = entry.bundleName;
            }

            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                var list = new List<UnityEngine.Object>();
                if (_catalog?.entries != null)
                {
                    for (var i = 0; i < _catalog.entries.Count; i++)
                    {
                        var e = _catalog.entries[i];
                        if (e == null || !string.Equals(e.bundleName, bundleName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(e.assetPath) || !string.IsNullOrEmpty(e.sceneName))
                        {
                            continue;
                        }

                        var obj = LoadEditorOrResources(e.assetPath, typeof(UnityEngine.Object));
                        if (obj != null)
                        {
                            list.Add(obj);
                        }
                    }
                }

                handle.Complete(list.ToArray(), true);
                yield break;
            }

            string loadErr = null;
            yield return EnsureBundleLoaded(bundleName, e => loadErr = e);
            if (!string.IsNullOrEmpty(loadErr) || !_loadedBundles.TryGetValue(bundleName, out var ab))
            {
                handle.Complete(null, false, loadErr ?? "Bundle missing");
                yield break;
            }

            var req = ab.LoadAllAssetsAsync();
            yield return req;
            RetainBundle(bundleName);
            handle.Complete(req.allAssets, req.allAssets != null);
        }

        IEnumerator LoadSubRoutine(SubAssetsHandle handle, string location, Type type)
        {
            if (!_catalog.TryResolve(location, out var entry))
            {
                handle.Complete(null, false, "Location not found");
                yield break;
            }

            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
#if UNITY_EDITOR
                var assets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(entry.assetPath);
                handle.Complete(assets, true);
#else
                handle.Complete(Array.Empty<UnityEngine.Object>(), true);
#endif
                yield break;
            }

            string err = null;
            yield return EnsureBundleLoaded(entry.bundleName, e => err = e);
            if (!string.IsNullOrEmpty(err) || !_loadedBundles.TryGetValue(entry.bundleName, out var ab))
            {
                handle.Complete(null, false, err ?? "Bundle missing");
                yield break;
            }

            var req = ab.LoadAssetWithSubAssetsAsync(entry.location, type);
            yield return req;
            RetainBundle(entry.bundleName);
            handle.Complete(req.allAssets, req.allAssets != null);
        }

        IEnumerator EnsureBundleLoaded(string bundleName, Action<string> onError)
        {
            if (string.IsNullOrEmpty(bundleName) || _loadedBundles.ContainsKey(bundleName))
            {
                onError?.Invoke(null);
                yield break;
            }

            if (PlayMode == EPlayMode.HostPlayMode)
            {
                bool exists = false;
                yield return _fileSystem.ExistsAsync(StreamingBundlePaths.Relative(bundleName), v => exists = v);
                if (!exists)
                {
                    yield return _fileSystem.ExistsAsync(bundleName, v => exists = v);
                }

                if (!exists)
                {
                    var remote = _settings != null ? _settings.ResolveRemoteRootUrl() : string.Empty;
                    if (string.IsNullOrEmpty(remote))
                    {
                        onError?.Invoke("HostPlayMode missing remoteRootUrl and local bundle");
                        yield break;
                    }

                    var info = _remoteVersion?.Find(bundleName) ?? new VersionBundleInfo
                    {
                        name = bundleName,
                        size = 0,
                        hash = string.Empty
                    };
                    var dl = new ResourceDownloader(
                        _fileSystem,
                        remote,
                        new List<VersionBundleInfo> { info },
                        _settings != null ? _settings.downloadRetryCount : 3,
                        _settings != null ? _settings.downloadTimeoutSeconds : 30f);
                    dl.Begin();
                    while (!dl.IsDone)
                    {
                        yield return null;
                    }

                    if (!dl.StatusIsSucceed)
                    {
                        onError?.Invoke(dl.Error);
                        yield break;
                    }
                }
            }

            AssetBundle ab = null;
            string err = null;
            yield return _fileSystem.LoadBundleAsync(
                StreamingBundlePaths.Relative(bundleName),
                _decryption,
                (bundle, e) =>
                {
                    ab = bundle;
                    err = e;
                });
            if (ab == null)
            {
                yield return _fileSystem.LoadBundleAsync(bundleName, _decryption, (bundle, e) =>
                {
                    ab = bundle;
                    err = e;
                });
            }

            if (ab == null)
            {
                onError?.Invoke(err ?? "Bundle load failed");
                yield break;
            }

            _loadedBundles[bundleName] = ab;
            if (!_bundleRefCounts.ContainsKey(bundleName))
            {
                _bundleRefCounts[bundleName] = 0;
            }

            onError?.Invoke(null);
        }

        void RetainBundle(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
            {
                return;
            }

            if (!_bundleRefCounts.ContainsKey(bundleName))
            {
                _bundleRefCounts[bundleName] = 0;
            }

            _bundleRefCounts[bundleName]++;
        }

        IEnumerator UpdatePackageRoutine(InitializationOperation op)
        {
            var remote = _settings != null ? _settings.ResolveRemoteRootUrl() : string.Empty;
            if (string.IsNullOrEmpty(remote))
            {
                op.Complete(false, "remoteRootUrl empty");
                yield break;
            }

            var url = remote + "/version.json";
            using (var req = UnityWebRequest.Get(url))
            {
                req.timeout = Mathf.CeilToInt(_settings != null ? _settings.downloadTimeoutSeconds : 30f);
                yield return req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    op.Complete(false, req.error);
                    yield break;
                }

                _remoteVersion = VersionManifest.FromJson(req.downloadHandler.text);
            }

            var tags = _settings != null ? string.Join(";", _settings.ParseFirstPackageTags()) : "first";
            var pending = CollectPendingByTags(tags);
            if (pending.Count == 0)
            {
                _localVersion = _remoteVersion;
                op.Complete(true);
                yield break;
            }

            var dl = new ResourceDownloader(
                _fileSystem,
                remote,
                pending,
                _settings != null ? _settings.downloadRetryCount : 3,
                _settings != null ? _settings.downloadTimeoutSeconds : 30f);
            dl.Begin();
            while (!dl.IsDone)
            {
                op.SetProgress(dl.Progress);
                yield return null;
            }

            if (!dl.StatusIsSucceed)
            {
                op.Complete(false, dl.Error);
                yield break;
            }

            _localVersion = _remoteVersion;
            op.Complete(true);
        }

        IEnumerator CheckUpdatesRoutine(InitializationOperation op, Action<bool, long> onResult)
        {
            if (PlayMode != EPlayMode.HostPlayMode)
            {
                onResult?.Invoke(false, 0);
                op.Complete(true);
                yield break;
            }

            var remote = _settings != null ? _settings.ResolveRemoteRootUrl() : string.Empty;
            if (string.IsNullOrEmpty(remote))
            {
                onResult?.Invoke(false, 0);
                op.Complete(true);
                yield break;
            }

            using (var req = UnityWebRequest.Get(remote + "/version.json"))
            {
                req.timeout = Mathf.CeilToInt(_settings != null ? _settings.downloadTimeoutSeconds : 30f);
                yield return req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    op.Complete(false, req.error);
                    yield break;
                }

                _remoteVersion = VersionManifest.FromJson(req.downloadHandler.text);
            }

            long size = 0;
            var pending = CollectPendingByTags(null);
            for (var i = 0; i < pending.Count; i++)
            {
                size += Math.Max(0, pending[i].size);
            }

            onResult?.Invoke(pending.Count > 0, size);
            op.Complete(true);
        }

        IEnumerator GetDownloadSizeRoutine(InitializationOperation op, string tags, Action<long> onSize)
        {
            if (_remoteVersion == null && PlayMode == EPlayMode.HostPlayMode)
            {
                yield return CheckUpdatesRoutine(new InitializationOperation(), null);
            }

            long size = 0;
            var pending = CollectPendingByTags(tags);
            for (var i = 0; i < pending.Count; i++)
            {
                size += Math.Max(0, pending[i].size);
            }

            onSize?.Invoke(size);
            op.Complete(true);
        }

        IEnumerator PreloadFirstRoutine(InitializationOperation op)
        {
            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                op.Complete(true);
                yield break;
            }

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_firstPackage?.entries != null)
            {
                for (var i = 0; i < _firstPackage.entries.Count; i++)
                {
                    var e = _firstPackage.entries[i];
                    if (e != null && !string.IsNullOrEmpty(e.bundleName))
                    {
                        names.Add(e.bundleName);
                    }
                }
            }
            else if (_catalog?.entries != null)
            {
                for (var i = 0; i < _catalog.entries.Count; i++)
                {
                    var e = _catalog.entries[i];
                    if (e != null && e.isFirstPackage && !string.IsNullOrEmpty(e.bundleName))
                    {
                        names.Add(e.bundleName);
                    }
                }
            }

            foreach (var name in names)
            {
                string err = null;
                yield return EnsureBundleLoaded(name, e => err = e);
                if (!string.IsNullOrEmpty(err))
                {
                    Debug.LogWarning($"[ResourcePackage] First package bundle '{name}': {err}");
                }
            }

            op.Complete(true);
        }

        IEnumerator ClearCacheRoutine(InitializationOperation op, bool unusedOnly)
        {
            var root = _fileSystem.GetCacheRootPath();
            try
            {
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                    op.Complete(true);
                    yield break;
                }

                if (!unusedOnly)
                {
                    Directory.Delete(root, true);
                    Directory.CreateDirectory(root);
                    op.Complete(true);
                    yield break;
                }

                var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var source = _remoteVersion ?? _localVersion;
                if (source?.bundles != null)
                {
                    for (var i = 0; i < source.bundles.Count; i++)
                    {
                        var b = source.bundles[i];
                        if (b != null && !string.IsNullOrEmpty(b.name))
                        {
                            keep.Add(b.name);
                            keep.Add(StreamingBundlePaths.Relative(b.name));
                        }
                    }
                }

                keep.Add("version.json");
                keep.Add(BootstrapManifest.FileName);
                keep.Add(StreamingBundlePaths.Relative("version.json"));
                keep.Add(BootstrapManifest.StreamingRelativePath);

                foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                {
                    var rel = file.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, '/', '\\')
                        .Replace("\\", "/");
                    if (!keep.Contains(rel) && !keep.Contains(Path.GetFileName(rel)))
                    {
                        File.Delete(file);
                    }
                }

                op.Complete(true);
            }
            catch (Exception ex)
            {
                op.Complete(false, ex.Message);
            }

            yield break;
        }

        List<VersionBundleInfo> CollectPendingByTags(string tags)
        {
            var result = new List<VersionBundleInfo>();
            if (_remoteVersion?.bundles == null)
            {
                return result;
            }

            string[] tagFilter = null;
            if (!string.IsNullOrWhiteSpace(tags))
            {
                tagFilter = tags.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }

            for (var i = 0; i < _remoteVersion.bundles.Count; i++)
            {
                var b = _remoteVersion.bundles[i];
                if (b == null || string.IsNullOrEmpty(b.name))
                {
                    continue;
                }

                if (tagFilter != null && tagFilter.Length > 0)
                {
                    var ok = false;
                    var bt = b.tags ?? string.Empty;
                    for (var t = 0; t < tagFilter.Length; t++)
                    {
                        if (bt.IndexOf(tagFilter[t].Trim(), StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            ok = true;
                            break;
                        }
                    }

                    if (!ok)
                    {
                        continue;
                    }
                }

                if (IsBundleReady(b.name))
                {
                    var local = _localVersion?.Find(b.name);
                    if (local == null ||
                        string.IsNullOrEmpty(b.hash) ||
                        string.Equals(local.hash, b.hash, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                result.Add(b);
            }

            return result;
        }

        static ResourceSettings LoadSettingsAsset(string path)
        {
            path = string.IsNullOrEmpty(path) ? ResourceSettings.DefaultAssetPath : path;
#if UNITY_EDITOR
            var fromEditor = UnityEditor.AssetDatabase.LoadAssetAtPath<ResourceSettings>(path);
            if (fromEditor != null)
            {
                return fromEditor;
            }
#endif
            if (ResRoot.TryToResourcesKey(path, out var key))
            {
                var fromRes = Resources.Load<ResourceSettings>(key);
                if (fromRes != null)
                {
                    return fromRes;
                }
            }

            return null;
        }

        static AddressCatalog LoadCatalog(string catalogAssetPath)
        {
            var path = string.IsNullOrEmpty(catalogAssetPath)
                ? ResRoot.CatalogAssetPath
                : catalogAssetPath;
            if (!ResRoot.Contains(path))
            {
                Debug.LogError($"[ResourcePackage] Catalog must live under {ResRoot.Folder}: {path}");
                return null;
            }

            return LoadEditorOrResources(path, typeof(AddressCatalog)) as AddressCatalog;
        }

        static FirstPackageManifest LoadFirstPackage(string path)
        {
            if (string.IsNullOrEmpty(path) || !ResRoot.Contains(path))
            {
                return null;
            }

            return LoadEditorOrResources(path, typeof(FirstPackageManifest)) as FirstPackageManifest;
        }

        static UnityEngine.Object LoadEditorOrResources(string assetPath, Type type)
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

            var fileName = Path.GetFileNameWithoutExtension(location);
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
