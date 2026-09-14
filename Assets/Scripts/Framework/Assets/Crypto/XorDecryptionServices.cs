namespace CoinFlip.Assets
{
    public sealed class XorDecryptionServices : IDecryptionServices
    {
        readonly byte _key;

        public XorDecryptionServices(byte key)
        {
            _key = key;
        }

        public byte[] DecryptData(byte[] encrypted)
        {
            if (encrypted == null)
            {
                return null;
            }

            var result = new byte[encrypted.Length];
            for (var i = 0; i < encrypted.Length; i++)
            {
                result[i] = (byte)(encrypted[i] ^ _key);
            }

            return result;
        }
    }
}
