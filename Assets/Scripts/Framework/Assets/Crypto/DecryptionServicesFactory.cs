using System;

namespace CoinFlip.Assets
{
    /// <summary>Factory for IDecryptionServices.</summary>
    public static class DecryptionServicesFactory
    {
        static Func<ResourceSettings, IDecryptionServices> _override;

        public static void SetFactory(Func<ResourceSettings, IDecryptionServices> factory) =>
            _override = factory;

        public static IDecryptionServices Create(ResourceSettings settings)
        {
            if (_override != null)
            {
                return _override(settings);
            }

            if (settings == null || !settings.enableEncryption)
            {
                return NullDecryptionServices.Instance;
            }

            switch (settings.decryptionType)
            {
                case EDecryptionType.Offset:
                    return new OffsetDecryptionServices(settings.encryptionOffset);
                case EDecryptionType.Xor:
                    return new XorDecryptionServices(settings.xorKey);
                case EDecryptionType.Custom:
                    return NullDecryptionServices.Instance;
                default:
                    return NullDecryptionServices.Instance;
            }
        }
    }
}
