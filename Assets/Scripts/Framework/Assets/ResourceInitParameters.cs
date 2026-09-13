namespace CoinFlip.Assets
{
    /// <summary>Init args aligned with YooAsset play-mode parameter objects.</summary>
    public sealed class ResourceInitParameters
    {
        public EPlayMode PlayMode = EPlayMode.OfflinePlayMode;
        public string CatalogResourcesPath = "AddressCatalog";
    }
}
