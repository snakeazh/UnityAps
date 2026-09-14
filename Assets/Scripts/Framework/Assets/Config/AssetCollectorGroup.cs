using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoinFlip.Assets
{
    [Serializable]
    public sealed class AssetCollectorGroup
    {
        public string groupName = "Default";
        public string assetTags = "first";
        public List<AssetCollectorEntry> collectors = new List<AssetCollectorEntry>();
    }
}
