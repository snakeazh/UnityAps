#if UNITY_EDITOR
using System.IO;
using CoinFlip.Assets;
using UnityEditor;
using UnityEngine;

namespace CoinFlip.EditorTools
{
    public static class ResourceEditorMenu
    {
        const string SettingsPath = ResourceSettings.DefaultAssetPath;

        [MenuItem("CoinFlip/Resource Settings", priority = 20)]
        public static void OpenOrCreateSettings()
        {
            var settings = LoadOrCreateSettings();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        [MenuItem("CoinFlip/Rebuild Address Catalog", priority = 21)]
        public static void RebuildCatalog()
        {
            var settings = LoadOrCreateSettings();
            new AssetBundleBuildPipeline(settings)
                .CollectAndAssign()
                .WriteCatalog();
        }

        [MenuItem("CoinFlip/Collect First Package", priority = 22)]
        public static void CollectFirstPackage()
        {
            var settings = LoadOrCreateSettings();
            new AssetBundleBuildPipeline(settings)
                .CollectAndAssign()
                .WriteCatalog()
                .WriteFirstPackageManifest();
        }

        [MenuItem("CoinFlip/Build AssetBundles", priority = 23)]
        public static void BuildAssetBundles()
        {
            var settings = LoadOrCreateSettings();
            var target = EditorUserBuildSettings.activeBuildTarget;
            new AssetBundleBuildPipeline(settings)
                .CollectAndAssign()
                .WriteCatalog()
                .WriteFirstPackageManifest()
                .BuildBundles(target)
                .CopyFirstPackageToStreaming(target);
        }

        public static ResourceSettings LoadOrCreateSettings()
        {
            EnsureResFolder();
            var settings = AssetDatabase.LoadAssetAtPath<ResourceSettings>(SettingsPath);
            if (settings != null)
            {
                return settings;
            }

            settings = ResourceSettings.CreateDefaultInstance();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Resource] Created " + SettingsPath);
            return settings;
        }

        static void EnsureResFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Res"))
            {
                AssetDatabase.CreateFolder("Assets", "Res");
            }
        }
    }
}
#endif
