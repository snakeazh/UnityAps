using System;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>
    /// Exclusive game-resource root (YooAsset collector folder).
    /// Asset loads are rejected unless the resolved path is under this directory.
    /// </summary>
    public static class ResRoot
    {
        public const string Folder = "Assets/Res";
        public const string CatalogAssetPath = Folder + "/AddressCatalog.asset";

        public static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path.Replace('\\', '/').Trim();
        }

        public static bool Contains(string assetPath)
        {
            var path = Normalize(assetPath);
            if (path.Length == 0)
            {
                return false;
            }

            if (path.StartsWith(Folder + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(path, Folder, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Resources.Load key for a path under Assets/Res (no extension).</summary>
        public static bool TryToResourcesKey(string assetPath, out string resourcesKey)
        {
            resourcesKey = null;
            var path = Normalize(assetPath);
            if (!Contains(path))
            {
                return false;
            }

            var relative = path.Substring(Folder.Length).TrimStart('/');
            if (string.IsNullOrEmpty(relative))
            {
                return false;
            }

            resourcesKey = "Res/" + System.IO.Path.ChangeExtension(relative, null);
            return !string.IsNullOrEmpty(resourcesKey);
        }
    }
}
