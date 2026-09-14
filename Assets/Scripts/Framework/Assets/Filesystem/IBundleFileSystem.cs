using System;
using System.Collections;

namespace CoinFlip.Assets
{
    /// <summary>Strategy: platform file access (all async via coroutines).</summary>
    public interface IBundleFileSystem
    {
        string GetBuiltinRootUrl();
        string GetCacheRootPath();
        IEnumerator ExistsAsync(string relativeOrUrl, Action<bool> onDone);
        IEnumerator ReadAllBytesAsync(string absoluteOrUrl, Action<byte[], string> onDone);
        IEnumerator WriteCacheAsync(string relativePath, byte[] data, Action<bool, string> onDone);
        IEnumerator DeleteCacheAsync(string relativePath, Action<bool> onDone);
        IEnumerator LoadBundleAsync(
            string urlOrPath,
            IDecryptionServices decryption,
            Action<UnityEngine.AssetBundle, string> onDone);
    }
}
