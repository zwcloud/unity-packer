namespace UnityPacker {

    internal static class PathUtils {

        public static string Standardize(this string path) {
            return path?
                .Replace("/", "/")
                .Replace('\\', '/')
                .Trim('/');
        }

        public static string Combine(this string a, string b) {
            return a.Standardize() + '/' + b.Standardize();
        }
    }
}