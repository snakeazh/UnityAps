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

        [Tooltip("Full project path under Assets/Res (the only loadable root).")]
        public string assetPath;

        [Tooltip("Build Settings scene name, when this address is a scene.")]
        public string sceneName;
    }

    /// <summary>
    /// Maps locations to paths under <see cref="ResRoot.Folder"/> (or scene names).
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

        public static AddressCatalog CreateBuiltin()
        {
            var catalog = CreateInstance<AddressCatalog>();
            catalog.name = "AddressCatalog (Builtin)";
            catalog.entries.Add(new AddressEntry
            {
                location = GameAssetLocations.GameTuning,
                assetPath = ResRoot.Folder + "/GameTuning.asset"
            });
            return catalog;
        }
    }
}
