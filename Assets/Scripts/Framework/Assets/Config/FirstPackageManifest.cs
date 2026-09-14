using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoinFlip.Assets
{
    [Serializable]
    public sealed class FirstPackageEntry
    {
        public string location;
        public string assetPath;
        public string bundleName;
        public string[] tags;
    }

    [CreateAssetMenu(fileName = "FirstPackageManifest", menuName = "CoinFlip/First Package Manifest", order = 3)]
    public sealed class FirstPackageManifest : ScriptableObject
    {
        public string tagsFilter = "first";
        public string generatedAt = string.Empty;
        public List<FirstPackageEntry> entries = new List<FirstPackageEntry>();

        public bool ContainsLocation(string location)
        {
            if (string.IsNullOrEmpty(location) || entries == null)
            {
                return false;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e != null && string.Equals(e.location, location, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
