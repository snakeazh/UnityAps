using System;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>Location metadata, aligned with YooAsset <c>AssetInfo</c>.</summary>
    public sealed class AssetInfo
    {
        public string Location { get; }
        public string AssetPath { get; }
        public Type AssetType { get; }
        public bool IsInvalid => string.IsNullOrEmpty(Location) && string.IsNullOrEmpty(AssetPath);

        public AssetInfo(string location, string assetPath, Type assetType)
        {
            Location = location ?? string.Empty;
            AssetPath = assetPath ?? string.Empty;
            AssetType = assetType ?? typeof(UnityEngine.Object);
        }
    }
}
