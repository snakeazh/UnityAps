using System;
using System.Collections.Generic;

namespace CoinFlip.Assets
{
    [Serializable]
    public sealed class VersionBundleInfo
    {
        public string name;
        public string hash;
        public long size;
        public string tags;
    }

    [Serializable]
    public sealed class VersionManifest
    {
        public string version = "1.0.0";
        public List<VersionBundleInfo> bundles = new List<VersionBundleInfo>();

        public VersionBundleInfo Find(string bundleName)
        {
            if (bundles == null || string.IsNullOrEmpty(bundleName))
            {
                return null;
            }

            for (var i = 0; i < bundles.Count; i++)
            {
                var b = bundles[i];
                if (b != null && string.Equals(b.name, bundleName, StringComparison.OrdinalIgnoreCase))
                {
                    return b;
                }
            }

            return null;
        }

        public static VersionManifest FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new VersionManifest();
            }

            return UnityEngine.JsonUtility.FromJson<VersionManifest>(json) ?? new VersionManifest();
        }

        public string ToJson(bool pretty = true) => UnityEngine.JsonUtility.ToJson(this, pretty);
    }
}
