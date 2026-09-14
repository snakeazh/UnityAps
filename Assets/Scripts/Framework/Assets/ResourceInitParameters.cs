namespace CoinFlip.Assets
{
    public sealed class ResourceInitParameters
    {
        public EPlayMode PlayMode = EPlayMode.OfflinePlayMode;
        public bool PlayModeSpecified;
        public string CatalogAssetPath = ResRoot.CatalogAssetPath;
        public string SettingsAssetPath = ResourceSettings.DefaultAssetPath;
        public ResourceSettings Settings;
        public IBundleFileSystem FileSystem;
        public IDecryptionServices Decryption;
    }
}
