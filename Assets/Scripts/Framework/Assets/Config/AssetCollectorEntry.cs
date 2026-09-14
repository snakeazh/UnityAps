using System;
using UnityEngine;

namespace CoinFlip.Assets
{
    [Serializable]
    public sealed class AssetCollectorEntry
    {
        public string collectPath = ResRoot.Folder;
        public ECollectorType collectorType = ECollectorType.MainAssetCollector;
        public EPackRule packRule = EPackRule.PackDirectory;
        public EAddressRule addressRule = EAddressRule.AddressByFileName;
        public string assetTags = "first";
        public string groupBundleName = string.Empty;
        public bool includeInFirstPackage = true;
    }
}
