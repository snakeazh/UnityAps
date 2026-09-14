using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace CoinFlip.Assets
{
    /// <summary>Android: StreamingAssets via jar URL + UWR; cache via persistentDataPath.</summary>
    public sealed class AndroidBundleFileSystem : IBundleFileSystem
    {
        readonly DesktopBundleFileSystem _cacheIo;
        readonly string _streaming;

        public AndroidBundleFileSystem(string cacheRootRelative)
        {
            _cacheIo = new DesktopBundleFileSystem(cacheRootRelative);
            _streaming = Application.streamingAssetsPath;
        }

        public string GetBuiltinRootUrl() => _streaming;
        public string GetCacheRootPath() => _cacheIo.GetCacheRootPath();

        public IEnumerator ExistsAsync(string relativeOrUrl, Action<bool> onDone)
        {
            // Cache first
            var cachePath = Path.Combine(GetCacheRootPath(), relativeOrUrl);
            if (File.Exists(cachePath))
            {
                onDone?.Invoke(true);
                yield break;
            }

            var url = CombineStreaming(relativeOrUrl);
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
            var url = absoluteOrUrl;
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("jar:", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                var cachePath = Path.Combine(GetCacheRootPath(), absoluteOrUrl);
                if (File.Exists(cachePath))
                {
                    yield return _cacheIo.ReadAllBytesAsync(cachePath, onDone);
                    yield break;
                }

                url = CombineStreaming(absoluteOrUrl);
            }

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

        public IEnumerator WriteCacheAsync(string relativePath, byte[] data, Action<bool, string> onDone) =>
            _cacheIo.WriteCacheAsync(relativePath, data, onDone);

        public IEnumerator DeleteCacheAsync(string relativePath, Action<bool> onDone) =>
            _cacheIo.DeleteCacheAsync(relativePath, onDone);

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
            onDone?.Invoke(req.assetBundle, req.assetBundle == null ? "LoadFromMemoryAsync failed" : null);
        }

        string CombineStreaming(string relative)
        {
            relative = (relative ?? string.Empty).Replace("\\", "/").TrimStart('/');
            if (_streaming.EndsWith("/"))
            {
                return _streaming + relative;
            }

            return _streaming + "/" + relative;
        }
    }
}
