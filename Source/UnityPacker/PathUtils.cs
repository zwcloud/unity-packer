using System.IO;

namespace UnityPacker {

    public static class PathUtils {

        public static string Standardize(this string path) {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }

        public static string Combine(this string a, string b) {
            return a.Standardize().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + b.Standardize().TrimStart(Path.DirectorySeparatorChar);
        }
    }
}