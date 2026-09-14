using System;
using UnityEngine;

namespace CoinFlip.Assets
{
    /// <summary>Factory Method: pick IBundleFileSystem by platform.</summary>
    public static class BundleFileSystemFactory
    {
        static Func<IBundleFileSystem> _override;

        public static void SetFactory(Func<IBundleFileSystem> factory) => _override = factory;

        public static IBundleFileSystem Create(string cacheRootRelative = "BundleCache")
        {
            if (_override != null)
            {
                return _override();
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            return new WebGlBundleFileSystem(cacheRootRelative);
#elif UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidBundleFileSystem(cacheRootRelative);
#elif UNITY_IOS && !UNITY_EDITOR
            return new MobileBundleFileSystem(cacheRootRelative);
#else
            return new DesktopBundleFileSystem(cacheRootRelative);
#endif
        }
    }
}
