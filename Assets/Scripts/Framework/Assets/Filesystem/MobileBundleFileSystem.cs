namespace CoinFlip.Assets
{
    /// <summary>iOS / similar mobile: file-capable StreamingAssets + cache.</summary>
    public sealed class MobileBundleFileSystem : DesktopBundleFileSystem
    {
        public MobileBundleFileSystem(string cacheRootRelative) : base(cacheRootRelative) { }
    }
}
