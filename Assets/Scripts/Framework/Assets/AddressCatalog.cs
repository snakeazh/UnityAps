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

        [Tooltip("Logical AssetBundle name from PackRule.")]
        public string bundleName;

        public string[] tags;

        public bool isFirstPackage;

        public bool isRawFile;

        [Tooltip("Raw file name under StreamingAssets/Bundles when isRawFile.")]
        public string rawFileName;
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

        public bool HasAsset(string location) => TryResolve(location, out _);

        public List<AddressEntry> GetEntriesByTag(string tag)
        {
            var list = new List<AddressEntry>();
            if (entries == null || string.IsNullOrEmpty(tag))
            {
                return list;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var item = entries[i];
                if (item?.tags == null)
                {
                    continue;
                }

                for (var t = 0; t < item.tags.Length; t++)
                {
                    if (string.Equals(item.tags[t], tag, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(item);
                        break;
                    }
                }
            }

            return list;
        }

        public AssetInfo[] GetAssetInfosByTag(string tag, Type type)
        {
            var matched = GetEntriesByTag(tag);
            var infos = new AssetInfo[matched.Count];
            for (var i = 0; i < matched.Count; i++)
            {
                var e = matched[i];
                infos[i] = new AssetInfo(e.location, e.assetPath, type);
            }

            return infos;
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
                assetPath = ResRoot.Folder + "/GameTuning.asset",
                bundleName = "assets_res.bundle",
                tags = new[] { "first" },
                isFirstPackage = true
            });
            return catalog;
        }
    }
}
