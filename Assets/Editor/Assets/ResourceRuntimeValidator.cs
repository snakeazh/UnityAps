#if UNITY_EDITOR
using System.Linq;
using CoinFlip.Assets;
using UnityEditor;
using UnityEngine;

namespace CoinFlip.EditorTools
{
    /// <summary>Editor-side smoke checks for the asset runtime (no Play Mode required).</summary>
    public static class ResourceRuntimeValidator
    {
        [MenuItem("CoinFlip/Validate Resource Runtime", priority = 24)]
        public static void Validate()
        {
            var settings = ResourceEditorMenu.LoadOrCreateSettings();
            if (settings == null)
            {
                Debug.LogError("[Validate] ResourceSettings missing.");
                return;
            }

            var pipeline = new AssetBundleBuildPipeline(settings)
                .CollectAndAssign()
                .WriteCatalog()
                .WriteFirstPackageManifest();

            var catalog = AssetDatabase.LoadAssetAtPath<AddressCatalog>(settings.catalogAssetPath);
            var first = AssetDatabase.LoadAssetAtPath<FirstPackageManifest>(settings.firstPackageManifestPath);
            var builds = pipeline.BuildAssetBundleBuildArray();

            var ok = catalog != null &&
                     catalog.entries != null &&
                     catalog.entries.Count > 0 &&
                     first != null &&
                     builds != null;

            if (!ok)
            {
                Debug.LogError("[Validate] Catalog/FirstPackage/AssetBundleBuild generation failed.");
                return;
            }

            var grouped = builds.GroupBy(b => b.assetBundleName).Any(g => g.Count() > 1);
            if (grouped)
            {
                Debug.LogError("[Validate] Duplicate assetBundleName in AssetBundleBuild[].");
                return;
            }

            Debug.Log(
                $"[Validate] OK — catalog={catalog.entries.Count}, first={first.entries.Count}, " +
                $"builds={builds.Length}, playModeEditor={settings.editorPlayMode}, " +
                $"pack collectors={settings.groups?.Sum(g => g.collectors?.Count ?? 0) ?? 0}");
        }
    }
}
#endif
