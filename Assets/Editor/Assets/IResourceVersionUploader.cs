#if UNITY_EDITOR
using System;
using UnityEditor;

namespace CoinFlip.EditorTools
{
    /// <summary>
    /// Mark concrete uploader types for discovery in the publish panel.
    /// Types without this attribute are ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ResourceVersionUploaderAttribute : Attribute
    {
        public string DisplayName { get; }
        public int Order { get; }

        public ResourceVersionUploaderAttribute(string displayName, int order = 100)
        {
            DisplayName = displayName ?? string.Empty;
            Order = order;
        }
    }

    public sealed class ResourceVersionUploadContext
    {
        public string Version;
        public string LocalCdnRoot;
        public string LocalVersionDir;
        public string LatestJsonPath;
        public bool ActivateLatest;
        public BuildTarget BuildTarget;
    }

    public interface IResourceVersionUploader
    {
        string DisplayName { get; }
        bool Upload(ResourceVersionUploadContext context, out string error);
    }

    /// <summary>Template Method base for custom uploaders (inherit + attribute).</summary>
    public abstract class ResourceVersionUploaderBase : IResourceVersionUploader
    {
        public abstract string DisplayName { get; }

        public bool Upload(ResourceVersionUploadContext context, out string error)
        {
            error = null;
            if (context == null)
            {
                error = "Upload context is null.";
                return false;
            }

            if (!UploadVersionFiles(context, out error))
            {
                return false;
            }

            if (context.ActivateLatest)
            {
                return UploadLatest(context, out error);
            }

            return true;
        }

        protected abstract bool UploadVersionFiles(ResourceVersionUploadContext context, out string error);

        protected virtual bool UploadLatest(ResourceVersionUploadContext context, out string error)
        {
            error = null;
            return true;
        }
    }
}
#endif
