using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace UnityPacker {

    public class Package : IEnumerable<KeyValuePair<string, OnDiskFile>> {

        private readonly Dictionary<string, OnDiskFile> files = new Dictionary<string, OnDiskFile>();

        /// <summary>
        /// Adds the given file on disk to this package
        /// </summary>
        /// <param name="file">The file to be added</param>
        public void PushFile(OnDiskFile file) {
            files.Add(file.packPath, file);
        }

        public OnDiskFile GetFile(string path) {
            return files[path];
        }

        /// <summary>
        /// Generates a .unitypackage file from this package
        /// </summary>
        /// <param name="root">Root directory name, usually starts with Assets/</param>
        public void SaveAs(string packagePath, string root) {

            root = root.Standardize();
            packagePath = packagePath.Standardize();

            // Security
            // If packagePath is relative, we make it absolute in this method context
            if (!Path.IsPathRooted(packagePath)) {
                packagePath = Path.GetFullPath(packagePath);
            }

            // Security
            // If user forgot to specify unitypackage format, we do it for him
            if (!packagePath.ToLower().EndsWith(".unitypackage")) {
                packagePath += ".unitypackage";
            }

            // We create a temporary folder to recreate the whole file structure, ready to be zipped
            var tmpPath = PathUtils.Combine(Path.GetTempPath(), "UnityPacker_" + RandomUtils.RandomString(10));
            if (Directory.Exists(tmpPath)) {
                Directory.Delete(tmpPath, true);
            }
            Directory.CreateDirectory(tmpPath);

            foreach (KeyValuePair<string, OnDiskFile> file in files) {
                /*
                 * For every file there exists a directory named file.guid in the tar archive that looks like:
                 *     + /asset -> actual asset data
                 *     + /asset.meta -> meta file
                 *     + /pathname -> actual path in the package
                 *
                 * There can be more files such as preview but are optional.
                 */

                string fdirpath = PathUtils.Combine(tmpPath, file.Value.GetHash());
                Directory.CreateDirectory(fdirpath);

                // Copy to asset file
                File.Copy(file.Value.diskPath, PathUtils.Combine(fdirpath, "asset"), true);

                // The pathname file
                using (var writer = new StreamWriter(PathUtils.Combine(fdirpath, "pathname"))) {
                    string rootedPath = PathUtils.Combine(root, file.Value.packPath);
                    writer.Write(rootedPath);
                }

                // The meta file
                using (var writer = new StreamWriter(PathUtils.Combine(fdirpath, "asset.meta")))  {
                    var doc = new YamlDocument(file.Value.meta);
                    var ys = new YamlStream(doc);
                    ys.Save(writer, false);
                }
                var fi = new FileInfo(PathUtils.Combine(fdirpath, "asset.meta"));
                using (FileStream fs = fi.Open(FileMode.Open)) {
                    fs.SetLength(fi.Length - 3 - Environment.NewLine.Length);
                }
            }

            // We create the package at the given location
            Packer.CreateTarGZ(packagePath, tmpPath);

            // We remove the whole temporary folder
            Directory.Delete(tmpPath, true);
        }

        /// <summary>
        /// Creates a folder from this package with the correct structure
        /// </summary>
        public void Extract(string directory)
        {
            foreach (var file in this)
            {
                var outPath = PathUtils.Combine(directory, file.Value.packPath);
                var metaPath = outPath + ".meta";
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                File.Copy(file.Value.diskPath, outPath);
                using (var writer = new StreamWriter(metaPath)) // the meta file
                {
                    var doc = new YamlDocument(file.Value.meta);
                    var ys = new YamlStream(doc);
                    ys.Save(writer);
                }
                var fi = new FileInfo(metaPath);
                using (var fs = fi.Open(FileMode.Open))
                {
                    fs.SetLength(fi.Length - 3 - Environment.NewLine.Length);
                }
            }
        }

        public IEnumerator<KeyValuePair<string, OnDiskFile>> GetEnumerator()
        {
            return files.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}