using System;

namespace CoinFlip.Assets
{
    public sealed class OffsetDecryptionServices : IDecryptionServices
    {
        readonly int _offset;

        public OffsetDecryptionServices(int offset)
        {
            _offset = Math.Max(0, offset);
        }

        public byte[] DecryptData(byte[] encrypted)
        {
            if (encrypted == null || encrypted.Length <= _offset)
            {
                return Array.Empty<byte>();
            }

            var result = new byte[encrypted.Length - _offset];
            Buffer.BlockCopy(encrypted, _offset, result, 0, result.Length);
            return result;
        }
    }
}
