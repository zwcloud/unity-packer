using System.IO;
using YamlDotNet.RepresentationModel;

namespace UnityPacker {

    public class OnDiskFile {

        public readonly string diskPath;
        public readonly string packPath;

        public YamlMappingNode meta { get; }

        public OnDiskFile(string packPath, string diskPath, YamlMappingNode meta) {

            this.packPath = packPath;
            this.diskPath = diskPath;
            this.meta = meta;
        }

        public Stream GetFile() {
            return File.Open(diskPath, FileMode.Open, FileAccess.Read);
        }

        public string GetHash() {
            return ((YamlScalarNode) meta["guid"]).Value;
        }
    }
}