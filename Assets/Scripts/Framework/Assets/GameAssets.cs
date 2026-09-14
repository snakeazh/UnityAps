using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinFlip.Assets
{
    /// <summary>
    /// Facade: static entry aligned with YooAsset <c>YooAssets</c>.
    /// Async-only public load API.
    /// </summary>
    public static class GameAssets
    {
        static readonly Dictionary<string, ResourcePackage> Packages =
            new Dictionary<string, ResourcePackage>(StringComparer.Ordinal);

        static bool _bootstrapped;

        public static bool Initialized { get; private set; }
        public static ResourcePackage DefaultPackage { get; private set; }

        public static void Initialize()
        {
            if (_bootstrapped)
            {
                return;
            }

            _bootstrapped = true;
            Debug.Log("[GameAssets] Initialized (async asset runtime).");
        }

        public static void SetFileSystemFactory(Func<IBundleFileSystem> factory) =>
            BundleFileSystemFactory.SetFactory(factory);

        public static void SetDecryptionFactory(Func<ResourceSettings, IDecryptionServices> factory) =>
            DecryptionServicesFactory.SetFactory(factory);

        public static ResourcePackage CreatePackage(string packageName)
        {
            Initialize();
            if (string.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentException("packageName");
            }

            if (Packages.TryGetValue(packageName, out var existing))
            {
                return existing;
            }

            var package = new ResourcePackage(packageName);
            Packages[packageName] = package;
            return package;
        }

        public static ResourcePackage GetPackage(string packageName)
        {
            Packages.TryGetValue(packageName, out var package);
            return package;
        }

        public static void SetDefaultPackage(ResourcePackage package)
        {
            DefaultPackage = package;
            Initialized = package != null && package.InitializeStatus;
        }

        public static InitializationOperation EnsureInitializedAsync(
            string packageName = GameAssetLocations.DefaultPackage,
            ResourceInitParameters parameters = null)
        {
            Initialize();
            var package = CreatePackage(packageName);
            if (!package.InitializeStatus)
            {
                var op = package.InitializeAsync(parameters ?? DefaultInitParameters());
                // Mark default early so awaiters can use facade after init completes.
                op.Completed += _ => SetDefaultPackage(package);
                DefaultPackage = package;
                return op;
            }

            SetDefaultPackage(package);
            var done = new InitializationOperation();
            done.Complete(true);
            return done;
        }

        public static AssetHandle LoadAssetAsync<TObject>(string location, uint priority = 0)
            where TObject : UnityEngine.Object =>
            RequireDefault().LoadAssetAsync<TObject>(location, priority);

        public static AssetHandle LoadAssetAsync(string location, uint priority = 0) =>
            RequireDefault().LoadAssetAsync(location, priority);

        public static SceneHandle LoadSceneAsync(
            string location,
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            bool allowSceneActivation = true,
            uint priority = 0) =>
            RequireDefault().LoadSceneAsync(location, sceneMode, physicsMode, allowSceneActivation, priority);

        public static RawFileHandle LoadRawFileAsync(string location) =>
            RequireDefault().LoadRawFileAsync(location);

        public static AllAssetsHandle LoadAllAssetsAsync(string locationOrBundle) =>
            RequireDefault().LoadAllAssetsAsync(locationOrBundle);

        public static SubAssetsHandle LoadSubAssetsAsync<TObject>(string location)
            where TObject : UnityEngine.Object =>
            RequireDefault().LoadSubAssetsAsync<TObject>(location);

        public static SubAssetsHandle LoadSubAssetsAsync(string location) =>
            RequireDefault().LoadSubAssetsAsync(location);

        public static InitializationOperation UpdatePackageAsync() =>
            RequireDefault().UpdatePackageAsync();

        public static InitializationOperation CheckForUpdatesAsync(Action<bool, long> onResult = null) =>
            RequireDefault().CheckForUpdatesAsync(onResult);

        public static InitializationOperation GetDownloadSizeAsync(string tags, Action<long> onSize = null) =>
            RequireDefault().GetDownloadSizeAsync(tags, onSize);

        public static ResourceDownloader DownloadBundlesAsync(string tags) =>
            RequireDefault().DownloadBundlesAsync(tags);

        public static ResourceDownloader DownloadBundlesAsync(IList<string> bundleNames) =>
            RequireDefault().DownloadBundlesAsync(bundleNames);

        public static ResourceDownloader CreateDownloader(string tags) =>
            RequireDefault().CreateDownloader(tags);

        public static InitializationOperation PreloadFirstPackageAsync() =>
            RequireDefault().PreloadFirstPackageAsync();

        public static InitializationOperation ClearCacheAsync() =>
            RequireDefault().ClearCacheAsync();

        public static InitializationOperation ClearUnusedCacheAsync() =>
            RequireDefault().ClearUnusedCacheAsync();

        public static void UnloadUnusedAssets() => RequireDefault().UnloadUnusedAssets();

        public static void UnloadBundle(string bundleName, bool force = false) =>
            RequireDefault().UnloadBundle(bundleName, force);

        public static bool IsFirstPackage(string location) =>
            RequireDefault().IsFirstPackage(location);

        public static bool IsBundleReady(string bundleName) =>
            RequireDefault().IsBundleReady(bundleName);

        public static bool HasAsset(string location) =>
            RequireDefault().HasAsset(location);

        public static bool CheckLocationValid(string location) =>
            RequireDefault().CheckLocationValid(location);

        public static AssetInfo[] GetAssetInfosByTag(string tag) =>
            RequireDefault().GetAssetInfosByTag(tag);

        public static string GetPackageVersion() =>
            DefaultPackage != null ? DefaultPackage.GetPackageVersion() : string.Empty;

        static ResourcePackage RequireDefault()
        {
            if (DefaultPackage == null || !DefaultPackage.InitializeStatus)
            {
                throw new InvalidOperationException(
                    "Call GameAssets.EnsureInitializedAsync / SetDefaultPackage first.");
            }

            return DefaultPackage;
        }

        static ResourceInitParameters DefaultInitParameters()
        {
            return new ResourceInitParameters
            {
                SettingsAssetPath = ResourceSettings.DefaultAssetPath
            };
        }
    }
}
