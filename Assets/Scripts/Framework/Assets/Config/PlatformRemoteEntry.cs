using System;
using UnityEngine;

namespace CoinFlip.Assets
{
    [Serializable]
    public sealed class PlatformRemoteEntry
    {
        public RuntimePlatform platform = RuntimePlatform.Android;
        public string remoteRootUrl = string.Empty;
    }
}
