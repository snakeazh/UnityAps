using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoinFlip.Assets
{
    /// <summary>
    /// Static entry aligned with YooAsset <c>YooAssets</c>:
    /// Initialize → CreatePackage → SetDefaultPackage → LoadAsset/Scene.
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
            Debug.Log("[GameAssets] Initialized (YooAsset-aligned local backend).");
        }

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

        /// <summary>Boot helper: create default package and initialize it.</summary>
        public static InitializationOperation EnsureInitializedAsync(
            string packageName = GameAssetLocations.DefaultPackage,
            ResourceInitParameters parameters = null)
        {
            Initialize();
            var package = CreatePackage(packageName);
            if (!package.InitializeStatus)
            {
                var op = package.InitializeAsync(parameters ?? DefaultInitParameters());
                SetDefaultPackage(package);
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

        public static AssetHandle LoadAssetSync<TObject>(string location)
            where TObject : UnityEngine.Object =>
            RequireDefault().LoadAssetSync<TObject>(location);

        public static SceneHandle LoadSceneAsync(
            string location,
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            bool allowSceneActivation = true,
            uint priority = 0) =>
            RequireDefault().LoadSceneAsync(location, sceneMode, physicsMode, allowSceneActivation, priority);

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
                PlayMode = Application.isEditor ? EPlayMode.EditorSimulateMode : EPlayMode.OfflinePlayMode
            };
        }
    }
}
