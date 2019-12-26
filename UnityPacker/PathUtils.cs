using System.IO;

namespace UnityPacker {

    internal static class PathUtils {

        public static string Standardize(this string path) {
            return path
                .Replace("/", "/")
                .Replace('\\', '/');
        }

        public static string Cleanup(this string a) {
            return a.Standardize().TrimStart('/');
        }

        public static string Combine(this string a, string b) {
            return a.Standardize().TrimEnd('/') + '/' + b.Standardize().TrimStart('/');
        }
    }
}