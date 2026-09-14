using System;
using System.IO;

namespace CoinFlip.Assets
{
    /// <summary>Editor/build helper mirroring runtime decryption.</summary>
    public static class BundleEncryptionUtility
    {
        public static byte[] Encrypt(byte[] plain, ResourceSettings settings)
        {
            if (plain == null || settings == null || !settings.enableEncryption)
            {
                return plain;
            }

            switch (settings.decryptionType)
            {
                case EDecryptionType.Offset:
                {
                    var offset = Math.Max(0, settings.encryptionOffset);
                    var result = new byte[plain.Length + offset];
                    var rng = new Random(settings.xorKey);
                    for (var i = 0; i < offset; i++)
                    {
                        result[i] = (byte)rng.Next(0, 256);
                    }

                    Buffer.BlockCopy(plain, 0, result, offset, plain.Length);
                    return result;
                }
                case EDecryptionType.Xor:
                {
                    var result = new byte[plain.Length];
                    for (var i = 0; i < plain.Length; i++)
                    {
                        result[i] = (byte)(plain[i] ^ settings.xorKey);
                    }

                    return result;
                }
                default:
                    return plain;
            }
        }

        public static void EncryptFileInPlace(string path, ResourceSettings settings)
        {
            if (!File.Exists(path) || settings == null || !settings.enableEncryption)
            {
                return;
            }

            var plain = File.ReadAllBytes(path);
            var encrypted = Encrypt(plain, settings);
            File.WriteAllBytes(path, encrypted);
        }
    }
}
