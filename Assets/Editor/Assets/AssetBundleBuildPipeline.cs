#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CoinFlip.Assets;
using UnityEditor;
using UnityEngine;

namespace CoinFlip.EditorTools
{
    /// <summary>Builder pattern: Collect → Pack → Catalog → FirstPackage → AssetBundleBuild[] → Build → Copy.</summary>
    public sealed class AssetBundleBuildPipeline
    {
        readonly ResourceSettings _settings;
        readonly List<AddressEntry> _entries = new List<AddressEntry>();
        readonly Dictionary<string, List<string>> _bundleToAssets =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, List<string>> _bundleToAddresses =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, HashSet<string>> _bundleTags =
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        public AssetBundleBuildPipeline(ResourceSettings settings)
        {
            _settings = settings ?? ResourceSettings.CreateDefaultInstance();
        }

        public AssetBundleBuildPipeline CollectAndAssign()
        {
            _entries.Clear();
            _bundleToAssets.Clear();
            _bundleToAddresses.Clear();
            _bundleTags.Clear();

            var firstTags = new HashSet<string>(_settings.ParseFirstPackageTags(), StringComparer.OrdinalIgnoreCase);
            var groups = _settings.groups;
            if (groups == null || groups.Count == 0)
            {
                groups = ResourceSettings.CreateDefaultInstance().groups;
            }

            foreach (var group in groups)
            {
                if (group?.collectors == null)
                {
                    continue;
                }

                foreach (var collector in group.collectors)
                {
                    if (collector == null)
                    {
                        continue;
                    }

                    var collectPath = string.IsNullOrEmpty(collector.collectPath)
                        ? _settings.collectRoot
                        : collector.collectPath;
                    collectPath = collectPath.Replace("\\", "/");
                    if (!ResRoot.Contains(collectPath) && collectPath != ResRoot.Folder)
                    {
                        Debug.LogWarning($"[AssetBundleBuild] Skip path outside Res: {collectPath}");
                        continue;
                    }

                    var pack = PackRuleFactory.Create(collector.packRule);
                    var addressRule = AddressRuleFactory.Create(collector.addressRule);
                    var guids = AssetDatabase.FindAssets(string.Empty, new[] { collectPath });
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid).Replace("\\", "/");
                        if (string.IsNullOrEmpty(path) || Directory.Exists(path))
                        {
                            continue;
                        }

                        if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                            path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        // Skip settings/catalog themselves from raw packing confusion but include as assets.
                        var isScene = path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase);
                        var tags = MergeTags(group.assetTags, collector.assetTags, collector.includeInFirstPackage, firstTags);
                        var isFirst = tags.Any(t => firstTags.Contains(t));
                        var isRaw = collector.packRule == EPackRule.PackRawFile;
                        var location = addressRule.GetAddress(path);
                        var bundleName = isScene
                            ? string.Empty
                            : pack.GetBundleName(path, collectPath, group.groupName, collector.groupBundleName);

                        if (collector.collectorType == ECollectorType.MainAssetCollector)
                        {
                            _entries.Add(new AddressEntry
                            {
                                location = location,
                                assetPath = isScene ? string.Empty : path,
                                sceneName = isScene ? Path.GetFileNameWithoutExtension(path) : string.Empty,
                                bundleName = bundleName,
                                tags = tags.ToArray(),
                                isFirstPackage = isFirst,
                                isRawFile = isRaw
                            });
                        }

                        if (isScene || string.IsNullOrEmpty(bundleName) || isRaw)
                        {
                            continue;
                        }

                        if (!_bundleToAssets.TryGetValue(bundleName, out var assets))
                        {
                            assets = new List<string>();
                            _bundleToAssets[bundleName] = assets;
                            _bundleToAddresses[bundleName] = new List<string>();
                            _bundleTags[bundleName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }

                        assets.Add(path);
                        _bundleToAddresses[bundleName].Add(location);
                        foreach (var t in tags)
                        {
                            _bundleTags[bundleName].Add(t);
                        }
                    }
                }
            }

            // Ensure Main scene catalog entry exists.
            if (_entries.All(e => e == null || !string.Equals(e.location, "Main", StringComparison.OrdinalIgnoreCase)))
            {
                _entries.Add(new AddressEntry
                {
                    location = "Main",
                    sceneName = "Main",
                    tags = _settings.ParseFirstPackageTags(),
                    isFirstPackage = true
                });
            }

            return this;
        }

        public AssetBundleBuildPipeline WriteCatalog()
        {
            var path = string.IsNullOrEmpty(_settings.catalogAssetPath)
                ? ResRoot.CatalogAssetPath
                : _settings.catalogAssetPath;
            EnsureFolder(Path.GetDirectoryName(path));
            var catalog = AssetDatabase.LoadAssetAtPath<AddressCatalog>(path);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<AddressCatalog>();
                AssetDatabase.CreateAsset(catalog, path);
            }

            catalog.entries = new List<AddressEntry>(_entries);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AssetBundleBuild] Catalog written: {path} ({_entries.Count} entries)");
            return this;
        }

        public AssetBundleBuildPipeline WriteFirstPackageManifest()
        {
            var path = string.IsNullOrEmpty(_settings.firstPackageManifestPath)
                ? ResRoot.Folder + "/FirstPackageManifest.asset"
                : _settings.firstPackageManifestPath;
            EnsureFolder(Path.GetDirectoryName(path));
            var manifest = AssetDatabase.LoadAssetAtPath<FirstPackageManifest>(path);
            if (manifest == null)
            {
                manifest = ScriptableObject.CreateInstance<FirstPackageManifest>();
                AssetDatabase.CreateAsset(manifest, path);
            }

            manifest.tagsFilter = _settings.firstPackageTags;
            manifest.generatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            manifest.entries = _entries
                .Where(e => e != null && e.isFirstPackage)
                .Select(e => new FirstPackageEntry
                {
                    location = e.location,
                    assetPath = e.assetPath,
                    bundleName = e.bundleName,
                    tags = e.tags
                })
                .ToList();
            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AssetBundleBuild] FirstPackageManifest: {manifest.entries.Count} entries");
            return this;
        }

        public AssetBundleBuild[] BuildAssetBundleBuildArray()
        {
            var builds = new List<AssetBundleBuild>();
            foreach (var kv in _bundleToAssets)
            {
                builds.Add(new AssetBundleBuild
                {
                    assetBundleName = kv.Key,
                    assetNames = kv.Value.ToArray(),
                    addressableNames = _bundleToAddresses[kv.Key].ToArray()
                });
            }

            return builds.ToArray();
        }

        public AssetBundleBuildPipeline BuildBundles(BuildTarget target)
        {
            var builds = BuildAssetBundleBuildArray();
            var output = Path.Combine(_settings.bundleOutputRoot, target.ToString()).Replace("\\", "/");
            Directory.CreateDirectory(output);
            if (builds.Length == 0)
            {
                Debug.LogWarning("[AssetBundleBuild] No bundles to build.");
                WriteVersionManifest(output, target);
                return this;
            }

            var manifest = BuildPipeline.BuildAssetBundles(
                output,
                builds,
                BuildAssetBundleOptions.ChunkBasedCompression,
                target);
            if (manifest == null)
            {
                Debug.LogError("[AssetBundleBuild] BuildAssetBundles failed.");
                return this;
            }

            if (_settings.enableEncryption)
            {
                foreach (var kv in _bundleToAssets)
                {
                    var file = Path.Combine(output, kv.Key);
                    BundleEncryptionUtility.EncryptFileInPlace(file, _settings);
                }
            }

            WriteVersionManifest(output, target);
            Debug.Log($"[AssetBundleBuild] Built {builds.Length} bundles → {output}");
            return this;
        }

        public AssetBundleBuildPipeline CopyFirstPackageToStreaming(BuildTarget target)
        {
            if (_settings.buildinCopyOption == EBuildinCopyOption.None)
            {
                return this;
            }

            var output = Path.Combine(_settings.bundleOutputRoot, target.ToString()).Replace("\\", "/");
            var streaming = _settings.streamingBundleRoot.Replace("\\", "/");
            EnsureFolder(streaming);

            if (_settings.buildinCopyOption == EBuildinCopyOption.ClearAndCopyAll ||
                _settings.buildinCopyOption == EBuildinCopyOption.ClearAndCopyByTags)
            {
                if (Directory.Exists(streaming))
                {
                    foreach (var file in Directory.GetFiles(streaming))
                    {
                        File.Delete(file);
                    }
                }
            }

            var firstTags = new HashSet<string>(_settings.ParseFirstPackageTags(), StringComparer.OrdinalIgnoreCase);
            var copyAll = _settings.buildinCopyOption == EBuildinCopyOption.ClearAndCopyAll;
            foreach (var kv in _bundleTags)
            {
                if (!copyAll && !kv.Value.Any(t => firstTags.Contains(t)))
                {
                    continue;
                }

                var src = Path.Combine(output, kv.Key);
                if (!File.Exists(src))
                {
                    continue;
                }

                var dst = Path.Combine(streaming, kv.Key);
                File.Copy(src, dst, true);
            }

            var verSrc = Path.Combine(output, "version.json");
            if (File.Exists(verSrc))
            {
                File.Copy(verSrc, Path.Combine(streaming, "version.json"), true);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[AssetBundleBuild] First-package bundles copied → {streaming}");
            return this;
        }

        void WriteVersionManifest(string output, BuildTarget target)
        {
            var manifest = new VersionManifest { version = _settings.packageVersion };
            foreach (var kv in _bundleToAssets)
            {
                var file = Path.Combine(output, kv.Key);
                long size = 0;
                string hash = string.Empty;
                if (File.Exists(file))
                {
                    var bytes = File.ReadAllBytes(file);
                    size = bytes.LongLength;
                    using (var md5 = MD5.Create())
                    {
                        hash = BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
                    }
                }

                var tags = _bundleTags.TryGetValue(kv.Key, out var set)
                    ? string.Join(";", set)
                    : string.Empty;
                manifest.bundles.Add(new VersionBundleInfo
                {
                    name = kv.Key,
                    hash = hash,
                    size = size,
                    tags = tags
                });
            }

            File.WriteAllText(Path.Combine(output, "version.json"), manifest.ToJson(true), Encoding.UTF8);
        }

        static HashSet<string> MergeTags(
            string groupTags,
            string collectorTags,
            bool includeInFirst,
            HashSet<string> firstTags)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddTags(set, groupTags);
            AddTags(set, collectorTags);
            if (includeInFirst && firstTags.Count > 0)
            {
                set.Add(firstTags.First());
            }

            return set;
        }

        static void AddTags(HashSet<string> set, string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return;
            }

            foreach (var p in tags.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                set.Add(p.Trim());
            }
        }

        static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            folder = folder.Replace("\\", "/");
            if (folder.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) &&
                !AssetDatabase.IsValidFolder(folder))
            {
                var parts = folder.Split('/');
                var current = parts[0];
                for (var i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(current, parts[i]);
                    }

                    current = next;
                }
            }
            else if (!folder.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                Directory.CreateDirectory(folder);
            }
        }
    }
}
#endif
