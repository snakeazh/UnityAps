namespace CoinFlip.Assets
{
    public sealed class NullDecryptionServices : IDecryptionServices
    {
        public static readonly NullDecryptionServices Instance = new NullDecryptionServices();
        public byte[] DecryptData(byte[] encrypted) => encrypted;
    }
}
