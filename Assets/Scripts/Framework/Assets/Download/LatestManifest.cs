using System;

namespace CoinFlip.Assets
{
    /// <summary>CDN root pointer for layout B: version directories + latest.json.</summary>
    [Serializable]
    public sealed class LatestManifest
    {
        public const string FileName = "latest.json";

        public string version = "1.0.0";
        public string path = "1.0.0";
        public bool forceUpdate;
        public string minAppVersion = string.Empty;

        public string ToJson(bool pretty = true) => UnityEngine.JsonUtility.ToJson(this, pretty);

        public static LatestManifest FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return UnityEngine.JsonUtility.FromJson<LatestManifest>(json);
        }

        public static LatestManifest Create(string version, bool forceUpdate = false, string minAppVersion = "")
        {
            version = string.IsNullOrWhiteSpace(version) ? "1.0.0" : version.Trim();
            return new LatestManifest
            {
                version = version,
                path = version,
                forceUpdate = forceUpdate,
                minAppVersion = minAppVersion ?? string.Empty
            };
        }
    }
}
