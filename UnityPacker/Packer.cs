using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using YamlDotNet.RepresentationModel;

namespace UnityPacker {

    public static class Packer {

        static YamlMappingNode ReadMeta(string file)
        {
            YamlStream metaYaml;
            if (file.EndsWith(".dll.meta"))
            {
                //fix the empty ` : Any` issue happens on .dll.meta files
                var fileContent = File.ReadAllText(file);
                fileContent = fileContent.Replace(": Any", "Any : Any");
                using TextReader reader = new StringReader(fileContent);
                metaYaml = new YamlStream();
                metaYaml.Load(reader);
                return (YamlMappingNode) metaYaml.Documents[0].RootNode;
            }
            else
            {
                using var reader = new StreamReader(file);
                metaYaml = new YamlStream();
                metaYaml.Load(reader);
            }

            return (YamlMappingNode) metaYaml.Documents[0].RootNode;
        }

        /// <summary>
        /// Creates a Package object from the given directory
        /// </summary>
        /// <param name="folderToPack">Directory to pack</param>
        /// <param name="packageName">Name of the package</param>
        /// <param name="respectMeta">Whether metadata files should be kept or not</param>
        /// <param name="omitExts">Extensions to omit</param>
        /// <param name="omitDirs">Directories to omit</param>
        /// <returns></returns>
        public static Package PackDirectory(string folderToPack, bool respectMeta, string ignoreRegex) {

            folderToPack = folderToPack.Standardize();

            string folderAbsolute = folderToPack;
            if (!Path.IsPathRooted(folderToPack)) {
                folderAbsolute = PathUtils.Combine(Environment.CurrentDirectory, folderToPack);
            }

            var directories = new Stack<string>();
            directories.Push(folderAbsolute);

            // Create a package object from the given directory
            var pack = new Package();

            while (directories.Count != 0) {

                string currentDirectory = directories.Pop();

                foreach (var directory in Directory.GetDirectories(currentDirectory)) {
                    if (!Regex.IsMatch(directory, ignoreRegex)) {
                        directories.Push(directory);
                    }
                }

                string[] files = Directory.GetFiles(currentDirectory);
                for (int i = 0; i < files.Length; i++) {

                    string filePath = files[i].Standardize();
                    string pathInFolder = filePath.Replace(folderAbsolute, "");
                    string extension = Path.GetExtension(filePath).ToLower();
                    bool skip = Regex.IsMatch(filePath, ignoreRegex) || extension == ".meta";

                    if (skip)
                        continue;

                    // Meta
                    YamlMappingNode meta;
                    string metaFile = filePath + ".meta";
                    bool useMetadata = respectMeta && File.Exists(metaFile);
                    if (useMetadata) {
                        // Get existing meta
                        meta = ReadMeta(metaFile);
                    } else {
                        // Create new basic meta
                        meta = new YamlMappingNode {
                            {"guid", RandomUtils.RandomHash()},
                            {"fileFormatVersion", "2"}
                        };
                    }

                    Logger.Log($"path:{filePath} meta:{(useMetadata ? "Yes" : "No")} guid:{meta["guid"]}");

                    pack.PushFile(new OnDiskFile(pathInFolder, filePath, meta));
                }
            }

            return pack;
        }

        public static Package ReadPackage(string pathToPack)
        {
            pathToPack = pathToPack.Standardize();

            Stream inStream = File.Open(pathToPack, FileMode.Open, FileAccess.Read);
            Stream gziStream = new GZipInputStream(inStream);
            TarArchive ar = TarArchive.CreateInputTarArchive(gziStream);

            var name = Path.GetFileNameWithoutExtension(pathToPack);

            var tmpPath = PathUtils.Combine(Path.GetTempPath(), "packUnity" + name);
            if (Directory.Exists(tmpPath))
            {
                Directory.Delete(tmpPath, true);
            }

            ar.ExtractContents(tmpPath);
            ar.Close();
            gziStream.Close();
            inStream.Close();

            var dirs = Directory.GetDirectories(tmpPath);

            var pack = new Package();
            foreach (var dir in dirs)
            {
                string assetPath = File.ReadAllText(PathUtils.Combine(dir, "pathname"));
                YamlMappingNode meta = ReadMeta(PathUtils.Combine(dir, "asset.meta"));
                string diskPath = PathUtils.Combine(dir, "asset");
                string guid = Path.GetFileName(dir);
                if (((YamlScalarNode) meta["guid"]).Value != guid)
                {
                    using (var stderr = new StreamWriter(Console.OpenStandardError()))
                    {
                        stderr.WriteLine("Erroneous File In Package! {0}, {1}", assetPath, guid);
                    }
                    continue;
                }
                var file = new OnDiskFile(assetPath, diskPath, meta);
                pack.PushFile(file);
            }

            return pack;
        }

        internal static void CreateTarGZ(string tgzPath, string sourceDirectory)
        {
            tgzPath = tgzPath.Standardize();
            sourceDirectory = sourceDirectory.Standardize();
            var tgzDir = Path.GetDirectoryName(tgzPath);
            if (!Directory.Exists(tgzDir))
            {
                Directory.CreateDirectory(tgzDir);
            }
            Stream outStream = File.Create(tgzPath);
            Stream gzoStream = new GZipOutputStream(outStream);
            var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            tarArchive.RootPath = sourceDirectory.Standardize();

            tarArchive.AddDirectoryFilesToTar(sourceDirectory, true);

            tarArchive.Close();
        }

        private static void AddDirectoryFilesToTar(this TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            var filenames = Directory.GetFiles(sourceDirectory);
            foreach (var filename in filenames)
            {
                var tarEntry = TarEntry.CreateEntryFromFile(filename);
                tarEntry.Name = filename.Remove(0, tarArchive.RootPath.Length + 1);
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (!recurse) return;

            var directories = Directory.GetDirectories(sourceDirectory);
            foreach (var directory in directories)
            {
                AddDirectoryFilesToTar(tarArchive, directory, true);
            }
        }
    }
}