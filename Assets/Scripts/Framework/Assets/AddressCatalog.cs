using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoinFlip.Assets
{
    [Serializable]
    public sealed class AddressEntry
    {
        [Tooltip("YooAsset location / address.")]
        public string location;

        [Tooltip("Resources.Load path (no extension), used by the local backend.")]
        public string resourcesPath;

        [Tooltip("Full project path (YooAsset asset path).")]
        public string assetPath;

        [Tooltip("Build Settings scene name, when this address is a scene.")]
        public string sceneName;
    }

    /// <summary>
    /// Maps locations to Resources / scene names so the local backend behaves like
    /// YooAsset addressable loading. Swap the package implementation to real YooAsset later.
    /// </summary>
    [CreateAssetMenu(fileName = "AddressCatalog", menuName = "CoinFlip/Address Catalog", order = 1)]
    public sealed class AddressCatalog : ScriptableObject
    {
        public List<AddressEntry> entries = new List<AddressEntry>();

        public bool TryResolve(string location, out AddressEntry entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(location) || entries == null)
            {
                return false;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var item = entries[i];
                if (item == null)
                {
                    continue;
                }

                if (LocationsEqual(item.location, location) ||
                    LocationsEqual(item.resourcesPath, location) ||
                    LocationsEqual(item.sceneName, location) ||
                    PathsEqual(item.assetPath, location))
                {
                    entry = item;
                    return true;
                }
            }

            return false;
        }

        static bool LocationsEqual(string a, string b) =>
            !string.IsNullOrEmpty(a) &&
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        static bool PathsEqual(string assetPath, string location)
        {
            if (string.IsNullOrEmpty(assetPath) || string.IsNullOrEmpty(location))
            {
                return false;
            }

            if (string.Equals(assetPath, location, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // YooAsset: path without extension also matches.
            var noExt = System.IO.Path.ChangeExtension(assetPath, null);
            return string.Equals(noExt, location, StringComparison.OrdinalIgnoreCase);
        }
    }
}
