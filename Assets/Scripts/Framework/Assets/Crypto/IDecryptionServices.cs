namespace CoinFlip.Assets
{
    /// <summary>Strategy: decrypt bundle/raw bytes before load.</summary>
    public interface IDecryptionServices
    {
        byte[] DecryptData(byte[] encrypted);
    }
}
