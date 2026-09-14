namespace CoinFlip.Assets
{
    /// <summary>
    /// Play modes aligned with YooAsset <c>EPlayMode</c>.
    /// </summary>
    public enum EPlayMode
    {
        /// <summary>Editor / local simulate: location catalog + Resources / Build Settings scenes.</summary>
        EditorSimulateMode = 0,
        /// <summary>Packaged offline: same local backend until a real YooAsset package is wired.</summary>
        OfflinePlayMode = 1,
        /// <summary>Host / CDN. Requires the YooAsset plugin; otherwise falls back to offline.</summary>
        HostPlayMode = 2,
    }
}
