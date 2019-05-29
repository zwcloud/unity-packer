using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace UnityPacker
{
    public class Package : IEnumerable<KeyValuePair<string, OnDiskFile>>
    {
        private readonly Dictionary<string, OnDiskFile> _files = new Dictionary<string, OnDiskFile>();

        /// <summary>
        /// Adds the given file on disk to this package
        /// </summary>
        /// <param name="file">The file to be added</param>
        public void PushFile(OnDiskFile file)
        {
            _files.Add(file.PackPath, file);
        }

        public OnDiskFile GetFile(string path)
        {
            return _files[path];
        }

        /// <summary>
        /// Generates a .unitypackage file from this package
        /// </summary>
        /// <param name="root">Root directory name, usually starts with Assets/</param>
        public void SaveAs(string packagePath, string root)
        {
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

            // Security
            // If user forgot prepend root folder with Assets/, we do it for him
            if (!root.ToLower().StartsWith("assets")) {
                root = "Assets/" + root;
            }

            // We create a temporary folder to recreate the whole file structure, ready to be zipped
            var tmpPath = Path.Combine(Path.GetTempPath(), "UnityPacker_" + RandomUtils.RandomString(10));
            if (Directory.Exists(tmpPath)) {
                Directory.Delete(tmpPath, true);
            }
            Directory.CreateDirectory(tmpPath);

            foreach (var file in _files)
            {
                /*
                 * For every file there exists a directory named file.guid in the tar archive that looks like:
                 *     + /asset -> actual asset data
                 *     + /asset.meta -> meta file
                 *     + /pathname -> actual path in the package
                 *
                 * There can be more files such as preview but are optional.
                 */

                string fdirpath = Path.Combine(tmpPath, file.Value.GetHash());
                Directory.CreateDirectory(fdirpath);

                File.Copy(file.Value.GetDiskPath(), Path.Combine(fdirpath, "asset"), true); // Copy to asset file

                using (var writer = new StreamWriter(Path.Combine(fdirpath, "pathname"))) // The pathname file
                {
                    var altName = file.Value.GetDiskPath();
                    if (altName.StartsWith("."))
                        altName = altName.Replace("." + Path.DirectorySeparatorChar, "");

                    writer.Write(Path.Combine(root, altName).Replace(Path.DirectorySeparatorChar, '/'));
                }
                using (var writer = new StreamWriter(Path.Combine(fdirpath, "asset.meta"))) // The meta file
                {
                    var doc = new YamlDocument(file.Value.GetMeta());
                    var ys = new YamlStream(doc);
                    ys.Save(writer);
                }
                var fi = new FileInfo(Path.Combine(fdirpath, "asset.meta"));
                using (var fs = fi.Open(FileMode.Open))
                {
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
                var outPath = Path.Combine(directory, file.Value.PackPath);
                var metaPath = outPath + ".meta";
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                File.Copy(file.Value.GetDiskPath(), outPath);
                using (var writer = new StreamWriter(metaPath)) // the meta file
                {
                    var doc = new YamlDocument(file.Value.GetMeta());
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
            return _files.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}