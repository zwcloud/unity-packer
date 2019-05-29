using System.IO;
using YamlDotNet.RepresentationModel;

namespace UnityPacker {
    public class OnDiskFile
    {
        private readonly string _diskPath;
        private readonly YamlMappingNode _meta;

        public string PackPath { get; }

        public OnDiskFile(string packPath, string diskPath, YamlMappingNode meta)
        {
            PackPath = packPath;
            _diskPath = diskPath;
            _meta = meta;
        }

        public string GetDiskPath()
        {
            return _diskPath;
        }

        public Stream GetFile()
        {
            return File.Open(_diskPath, FileMode.Open, FileAccess.Read);
        }

        public YamlMappingNode GetMeta()
        {
            return _meta;
        }

        public string GetHash()
        {
            return ((YamlScalarNode) _meta["guid"]).Value;
        }
    }
}