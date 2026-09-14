using System.IO;

namespace CoinFlip.Assets
{
    public interface IAddressRule
    {
        string GetAddress(string assetPath);
    }

    public static class AddressRuleFactory
    {
        public static IAddressRule Create(EAddressRule rule)
        {
            return rule == EAddressRule.AddressByFolderAndFileName
                ? (IAddressRule)AddressByFolderAndFileNameRule.Instance
                : AddressByFileNameRule.Instance;
        }
    }

    public sealed class AddressByFileNameRule : IAddressRule
    {
        public static readonly AddressByFileNameRule Instance = new AddressByFileNameRule();
        public string GetAddress(string assetPath) => Path.GetFileNameWithoutExtension(assetPath);
    }

    public sealed class AddressByFolderAndFileNameRule : IAddressRule
    {
        public static readonly AddressByFolderAndFileNameRule Instance = new AddressByFolderAndFileNameRule();
        public string GetAddress(string assetPath)
        {
            var path = assetPath.Replace("\\", "/");
            var file = Path.GetFileNameWithoutExtension(path);
            var dir = Path.GetFileName(Path.GetDirectoryName(path) ?? string.Empty);
            return string.IsNullOrEmpty(dir) ? file : dir + "_" + file;
        }
    }
}
