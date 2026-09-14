using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CoinFlip.Assets
{
    /// <summary>Editor / Standalone file system.</summary>
    public sealed class DesktopBundleFileSystem : IBundleFileSystem
    {
        readonly string _cacheRoot;

        public DesktopBundleFileSystem(string cacheRootRelative)
        {
            _cacheRoot = Path.Combine(Application.persistentDataPath, cacheRootRelative ?? "BundleCache");
            Directory.CreateDirectory(_cacheRoot);
        }

        public string GetBuiltinRootUrl() =>
            "file://" + Application.streamingAssetsPath.Replace("\\", "/");

        public string GetCacheRootPath() => _cacheRoot.Replace("\\", "/");

        public IEnumerator ExistsAsync(string relativeOrUrl, Action<bool> onDone)
        {
            var path = ResolveLocalPath(relativeOrUrl);
            onDone?.Invoke(File.Exists(path));
            yield break;
        }

        public IEnumerator ReadAllBytesAsync(string absoluteOrUrl, Action<byte[], string> onDone)
        {
            if (absoluteOrUrl != null &&
                (absoluteOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                 absoluteOrUrl.StartsWith("jar:", StringComparison.OrdinalIgnoreCase) ||
                 absoluteOrUrl.StartsWith("file:", StringComparison.OrdinalIgnoreCase)))
            {
                using (var req = UnityWebRequest.Get(absoluteOrUrl))
                {
                    yield return req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                    if (req.result != UnityWebRequest.Result.Success)
#else
                    if (req.isNetworkError || req.isHttpError)
#endif
                    {
                        onDone?.Invoke(null, req.error);
                        yield break;
                    }

                    onDone?.Invoke(req.downloadHandler.data, null);
                }

                yield break;
            }

            var path = ResolveLocalPath(absoluteOrUrl);
            try
            {
                onDone?.Invoke(File.ReadAllBytes(path), null);
            }
            catch (Exception ex)
            {
                onDone?.Invoke(null, ex.Message);
            }

            yield break;
        }

        public IEnumerator WriteCacheAsync(string relativePath, byte[] data, Action<bool, string> onDone)
        {
            try
            {
                var path = Path.Combine(_cacheRoot, relativePath);
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllBytes(path, data);
                onDone?.Invoke(true, null);
            }
            catch (Exception ex)
            {
                onDone?.Invoke(false, ex.Message);
            }

            yield break;
        }

        public IEnumerator DeleteCacheAsync(string relativePath, Action<bool> onDone)
        {
            try
            {
                var path = Path.Combine(_cacheRoot, relativePath);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                onDone?.Invoke(true);
            }
            catch
            {
                onDone?.Invoke(false);
            }

            yield break;
        }

        public IEnumerator LoadBundleAsync(
            string urlOrPath,
            IDecryptionServices decryption,
            Action<AssetBundle, string> onDone)
        {
            byte[] bytes = null;
            string err = null;
            yield return ReadAllBytesAsync(urlOrPath, (b, e) => { bytes = b; err = e; });
            if (bytes == null)
            {
                onDone?.Invoke(null, err ?? "Read failed");
                yield break;
            }

            if (decryption != null && !(decryption is NullDecryptionServices))
            {
                bytes = decryption.DecryptData(bytes);
            }

            var req = AssetBundle.LoadFromMemoryAsync(bytes);
            yield return req;
            if (req.assetBundle == null)
            {
                onDone?.Invoke(null, "LoadFromMemoryAsync failed");
                yield break;
            }

            onDone?.Invoke(req.assetBundle, null);
        }

        string ResolveLocalPath(string pathOrUrl)
        {
            if (string.IsNullOrEmpty(pathOrUrl))
            {
                return pathOrUrl;
            }

            if (pathOrUrl.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                return pathOrUrl.Substring(7);
            }

            if (Path.IsPathRooted(pathOrUrl) || pathOrUrl.Contains(":/") || pathOrUrl.Contains(":\\"))
            {
                return pathOrUrl;
            }

            var cache = Path.Combine(_cacheRoot, pathOrUrl);
            if (File.Exists(cache))
            {
                return cache;
            }

            return Path.Combine(Application.streamingAssetsPath, pathOrUrl);
        }
    }
}
