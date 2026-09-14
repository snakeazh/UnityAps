#if UNITY_EDITOR
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CoinFlip.EditorTools
{
    public enum EVersionBump
    {
        None = 0,
        Patch = 1,
        Minor = 2,
        Major = 3,
    }

    /// <summary>SemVer-ish bump helper for ResourceSettings.packageVersion.</summary>
    public static class VersionBumpUtility
    {
        static readonly Regex SemVer = new Regex(
            @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?<suffix>.*)$",
            RegexOptions.Compiled);

        public static bool TryParse(string version, out int major, out int minor, out int patch, out string suffix)
        {
            major = minor = patch = 0;
            suffix = string.Empty;
            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            var m = SemVer.Match(version.Trim());
            if (!m.Success)
            {
                return false;
            }

            major = int.Parse(m.Groups["major"].Value, CultureInfo.InvariantCulture);
            minor = int.Parse(m.Groups["minor"].Value, CultureInfo.InvariantCulture);
            patch = int.Parse(m.Groups["patch"].Value, CultureInfo.InvariantCulture);
            suffix = m.Groups["suffix"].Value ?? string.Empty;
            return true;
        }

        public static string Bump(string version, EVersionBump bump)
        {
            if (bump == EVersionBump.None)
            {
                return string.IsNullOrWhiteSpace(version) ? "1.0.0" : version.Trim();
            }

            if (!TryParse(version, out var major, out var minor, out var patch, out var suffix))
            {
                return "1.0.0";
            }

            switch (bump)
            {
                case EVersionBump.Major:
                    major++;
                    minor = 0;
                    patch = 0;
                    break;
                case EVersionBump.Minor:
                    minor++;
                    patch = 0;
                    break;
                default:
                    patch++;
                    break;
            }

            return $"{major}.{minor}.{patch}{suffix}";
        }

        public static string Preview(string version, EVersionBump bump) => Bump(version, bump);
    }
}
#endif
