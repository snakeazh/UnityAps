using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CoinFlip.Assets
{
    /// <summary>WebGL: no sync File IO for StreamingAssets; UWR everywhere.</summary>
    public sealed class WebGlBundleFileSystem : IBundleFileSystem
    {
        readonly string _cacheRoot;

        public WebGlBundleFileSystem(string cacheRootRelative)
        {
            _cacheRoot = Path.Combine(Application.persistentDataPath, cacheRootRelative ?? "BundleCache")
                .Replace("\\", "/");
        }

        public string GetBuiltinRootUrl() => Application.streamingAssetsPath;
        public string GetCacheRootPath() => _cacheRoot;

        public IEnumerator ExistsAsync(string relativeOrUrl, Action<bool> onDone)
        {
            var url = ToUrl(relativeOrUrl);
            using (var req = UnityWebRequest.Head(url))
            {
                yield return req.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                onDone?.Invoke(req.result == UnityWebRequest.Result.Success);
#else
                onDone?.Invoke(!(req.isNetworkError || req.isHttpError));
#endif
            }
        }

        public IEnumerator ReadAllBytesAsync(string absoluteOrUrl, Action<byte[], string> onDone)
        {
            var url = ToUrl(absoluteOrUrl);
            using (var req = UnityWebRequest.Get(url))
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
        }

        public IEnumerator WriteCacheAsync(string relativePath, byte[] data, Action<bool, string> onDone)
        {
            // IDBFS-backed persistentDataPath allows File IO asynchronously after sync.
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
            if (decryption == null || decryption is NullDecryptionServices)
            {
                var url = ToUrl(urlOrPath);
                using (var req = UnityWebRequestAssetBundle.GetAssetBundle(url))
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

                    onDone?.Invoke(DownloadHandlerAssetBundle.GetContent(req), null);
                }

                yield break;
            }

            byte[] bytes = null;
            string err = null;
            yield return ReadAllBytesAsync(urlOrPath, (b, e) => { bytes = b; err = e; });
            if (bytes == null)
            {
                onDone?.Invoke(null, err ?? "Read failed");
                yield break;
            }

            bytes = decryption.DecryptData(bytes);
            var load = AssetBundle.LoadFromMemoryAsync(bytes);
            yield return load;
            onDone?.Invoke(load.assetBundle, load.assetBundle == null ? "LoadFromMemoryAsync failed" : null);
        }

        string ToUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            var relative = StreamingBundlePaths.Relative(path);
            var cache = Path.Combine(_cacheRoot, relative).Replace("\\", "/");
            if (File.Exists(cache))
            {
                return "file://" + cache;
            }

            var bareCache = Path.Combine(_cacheRoot, path).Replace("\\", "/");
            if (File.Exists(bareCache))
            {
                return "file://" + bareCache;
            }

            var root = Application.streamingAssetsPath.TrimEnd('/');
            return root + "/" + relative.TrimStart('/');
        }
    }
}
