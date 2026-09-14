namespace CoinFlip.Assets
{
    public interface IPackRule
    {
        string GetBundleName(string assetPath, string collectPath, string groupName, string groupBundleName);
    }
}
