using System;
using System.Collections.Generic;

namespace CoinFlip.Assets
{
    /// <summary>
    /// Player bootstrap payload written next to StreamingAssets bundles (no Resources required).
    /// </summary>
    [Serializable]
    public sealed class BootstrapAddressEntry
    {
        public string location;
        public string assetPath;
        public string sceneName;
        public string bundleName;
        public string tags;
        public bool isFirstPackage;
        public bool isRawFile;
        public string rawFileName;
    }

    [Serializable]
    public sealed class BootstrapManifest
    {
        public const string FileName = "bootstrap.json";
        public const string StreamingRelativePath = "Bundles/" + FileName;

        public string packageVersion = "1.0.0";
        public int editorPlayMode;
        public int runtimePlayMode;
        public string remoteRootUrl = string.Empty;
        public string cacheRoot = "BundleCache";
        public string firstPackageTags = "first";
        public bool enableEncryption;
        public int decryptionType;
        public int encryptionOffset = 32;
        public byte xorKey = 0x5A;
        public List<BootstrapAddressEntry> entries = new List<BootstrapAddressEntry>();

        public string ToJson(bool pretty = true) => UnityEngine.JsonUtility.ToJson(this, pretty);

        public static BootstrapManifest FromJson(string json) =>
            string.IsNullOrEmpty(json)
                ? new BootstrapManifest()
                : (UnityEngine.JsonUtility.FromJson<BootstrapManifest>(json) ?? new BootstrapManifest());
    }

    public static class StreamingBundlePaths
    {
        public const string FolderName = "Bundles";

        public static string Relative(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return FolderName;
            }

            fileName = fileName.Replace("\\", "/").TrimStart('/');
            if (fileName.StartsWith(FolderName + "/", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileName, FolderName, StringComparison.OrdinalIgnoreCase))
            {
                return fileName;
            }

            return FolderName + "/" + fileName;
        }
    }
}
