#if UNITY_EDITOR
using System.IO;

namespace CoinFlip.EditorTools
{
    /// <summary>
    /// Default uploader: local CDN layout is already written; validates paths only.
    /// Sync Publish/cdn to your CDN via CI (coscli / aws s3 sync / etc.).
    /// </summary>
    [ResourceVersionUploader("Local Export (CI sync)", order: 0)]
    public sealed class LocalExportResourceVersionUploader : ResourceVersionUploaderBase
    {
        public override string DisplayName => "Local Export (CI sync)";

        protected override bool UploadVersionFiles(ResourceVersionUploadContext context, out string error)
        {
            error = null;
            if (context == null || string.IsNullOrEmpty(context.LocalVersionDir) ||
                !Directory.Exists(context.LocalVersionDir))
            {
                error = "Local version directory missing: " + context?.LocalVersionDir;
                return false;
            }

            if (!File.Exists(Path.Combine(context.LocalVersionDir, "version.json")))
            {
                error = "version.json missing under " + context.LocalVersionDir;
                return false;
            }

            return true;
        }

        protected override bool UploadLatest(ResourceVersionUploadContext context, out string error)
        {
            error = null;
            if (!context.ActivateLatest)
            {
                return true;
            }

            if (string.IsNullOrEmpty(context.LatestJsonPath) || !File.Exists(context.LatestJsonPath))
            {
                error = "latest.json missing: " + context.LatestJsonPath;
                return false;
            }

            return true;
        }
    }
}
#endif
