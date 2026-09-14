using System.Collections.Generic;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>Editor + runtime shared settings (ScriptableObject config).</summary>
    [CreateAssetMenu(fileName = "ResourceSettings", menuName = "CoinFlip/Resource Settings", order = 2)]
    public sealed class ResourceSettings : ScriptableObject
    {
        public const string DefaultAssetPath = ResRoot.Folder + "/ResourceSettings.asset";

        [Header("Play Mode")]
        public EPlayMode editorPlayMode = EPlayMode.EditorSimulateMode;
        public EPlayMode runtimePlayMode = EPlayMode.OfflinePlayMode;

        [Header("Paths")]
        public string catalogAssetPath = ResRoot.CatalogAssetPath;
        public string firstPackageManifestPath = ResRoot.Folder + "/FirstPackageManifest.asset";
        public string collectRoot = ResRoot.Folder;
        public string bundleOutputRoot = "Bundles";
        public string streamingBundleRoot = "Assets/StreamingAssets/Bundles";

        [Header("First Package")]
        public string firstPackageTags = "first";
        public EBuildinCopyOption buildinCopyOption = EBuildinCopyOption.ClearAndCopyByTags;

        [Header("Host / Download")]
        public string remoteRootUrl = string.Empty;
        public string packageVersion = "1.0.0";
        public int downloadRetryCount = 3;
        public float downloadTimeoutSeconds = 30f;
        public string cacheRoot = "BundleCache";
        public List<PlatformRemoteEntry> platformRemoteUrls = new List<PlatformRemoteEntry>();

        [Header("Encryption")]
        public bool enableEncryption;
        public EDecryptionType decryptionType = EDecryptionType.None;
        public int encryptionOffset = 32;
        public byte xorKey = 0x5A;

        [Header("Collectors")]
        public List<AssetCollectorGroup> groups = new List<AssetCollectorGroup>();

        public EPlayMode ResolvePlayMode()
        {
            return Application.isEditor ? editorPlayMode : runtimePlayMode;
        }

        public string ResolveRemoteRootUrl()
        {
            if (platformRemoteUrls != null)
            {
                for (var i = 0; i < platformRemoteUrls.Count; i++)
                {
                    var e = platformRemoteUrls[i];
                    if (e != null && e.platform == Application.platform &&
                        !string.IsNullOrWhiteSpace(e.remoteRootUrl))
                    {
                        return e.remoteRootUrl.TrimEnd('/');
                    }
                }
            }

            return string.IsNullOrWhiteSpace(remoteRootUrl) ? string.Empty : remoteRootUrl.TrimEnd('/');
        }

        public string[] ParseFirstPackageTags()
        {
            if (string.IsNullOrWhiteSpace(firstPackageTags))
            {
                return new[] { "first" };
            }

            var parts = firstPackageTags.Split(new[] { ';', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            return parts;
        }

        public static ResourceSettings CreateDefaultInstance()
        {
            var s = CreateInstance<ResourceSettings>();
            s.name = "ResourceSettings";
            var group = new AssetCollectorGroup
            {
                groupName = "Default",
                assetTags = "first",
                collectors = new List<AssetCollectorEntry>
                {
                    new AssetCollectorEntry
                    {
                        collectPath = ResRoot.Folder,
                        packRule = EPackRule.PackDirectory,
                        addressRule = EAddressRule.AddressByFileName,
                        assetTags = "first",
                        includeInFirstPackage = true
                    }
                }
            };
            s.groups.Add(group);
            return s;
        }
    }
}
