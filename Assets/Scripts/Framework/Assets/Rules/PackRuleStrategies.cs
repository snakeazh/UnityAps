using System.IO;

namespace CoinFlip.Assets
{
    public static class PackRuleFactory
    {
        public static IPackRule Create(EPackRule rule)
        {
            switch (rule)
            {
                case EPackRule.PackSeparately: return PackSeparatelyRule.Instance;
                case EPackRule.PackTopDirectory: return PackTopDirectoryRule.Instance;
                case EPackRule.PackCollector: return PackCollectorRule.Instance;
                case EPackRule.PackGroup: return PackGroupRule.Instance;
                case EPackRule.PackRawFile: return PackSeparatelyRule.Instance;
                default: return PackDirectoryRule.Instance;
            }
        }

        public static string SanitizeBundleName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "bundle";
            }

            var s = path.Replace("\\", "/").Trim('/').ToLowerInvariant();
            s = s.Replace('/', '_').Replace('.', '_').Replace(' ', '_');
            if (!s.EndsWith(".bundle"))
            {
                s += ".bundle";
            }

            return s;
        }
    }

    public sealed class PackSeparatelyRule : IPackRule
    {
        public static readonly PackSeparatelyRule Instance = new PackSeparatelyRule();
        public string GetBundleName(string assetPath, string collectPath, string groupName, string groupBundleName) =>
            PackRuleFactory.SanitizeBundleName(Path.ChangeExtension(assetPath.Replace("\\", "/"), null));
    }

    public sealed class PackDirectoryRule : IPackRule
    {
        public static readonly PackDirectoryRule Instance = new PackDirectoryRule();
        public string GetBundleName(string assetPath, string collectPath, string groupName, string groupBundleName)
        {
            var dir = Path.GetDirectoryName(assetPath.Replace("\\", "/"))?.Replace("\\", "/") ?? "assets";
            return PackRuleFactory.SanitizeBundleName(dir);
        }
    }

    public sealed class PackTopDirectoryRule : IPackRule
    {
        public static readonly PackTopDirectoryRule Instance = new PackTopDirectoryRule();
        public string GetBundleName(string assetPath, string collectPath, string groupName, string groupBundleName)
        {
            var root = (collectPath ?? ResRoot.Folder).Replace("\\", "/").TrimEnd('/');
            var path = assetPath.Replace("\\", "/");
            if (path.StartsWith(root + "/"))
            {
                var rest = path.Substring(root.Length + 1);
                var slash = rest.IndexOf('/');
                var top = slash >= 0 ? rest.Substring(0, slash) : Path.GetFileNameWithoutExtension(rest);
                return PackRuleFactory.SanitizeBundleName(root + "/" + top);
            }

            return PackDirectoryRule.Instance.GetBundleName(assetPath, collectPath, groupName, groupBundleName);
        }
    }

    public sealed class PackCollectorRule : IPackRule
    {
        public static readonly PackCollectorRule Instance = new PackCollectorRule();
        public string GetBundleName(string assetPath, string collectPath, string groupName, string groupBundleName) =>
            PackRuleFactory.SanitizeBundleName(collectPath ?? "collector");
    }

    public sealed class PackGroupRule : IPackRule
    {
        public static readonly PackGroupRule Instance = new PackGroupRule();
        public string GetBundleName(string assetPath, string collectPath, string groupName, string groupBundleName) =>
            PackRuleFactory.SanitizeBundleName(
                string.IsNullOrEmpty(groupBundleName) ? groupName ?? "group" : groupBundleName);
    }
}
