using System;
using System.Collections.Generic;

namespace UnityPacker {

    class PackProgram {

        static void Main(string[] args) {

            if (args.Length < 2) {
                Console.WriteLine("Usage:");
                Console.WriteLine("UnityPacker mode=pack folder=\"\" name=\"Package\" root=\"\" exts=\"\" dirs=\"\"");
                return;
            }

            var parsedArgs = new Dictionary<string, string>();
            foreach (string arg in args) {
                var split = arg.Split(new[] { '=' });
                if (split.Length == 2) {
                    parsedArgs.Add(split[0], split[1].Trim(new[] { '"', '\'' }));
                }
            }

            string getArg(string key) {
                if (parsedArgs.ContainsKey(key)) {
                    return parsedArgs[key];
                }
                throw new Exception($"Argument '{key}' is missing !");
            }
            
            switch (getArg("mode").ToLower()) {
                case "p":
                case "pck":
                case "pack":
                    Pack(getArg("folder"), getArg("name"), getArg("root"), getArg("ignore"));
                    break;
                case "u":
                case "upck":
                case "unpack":
                    Unpack(getArg("package"), getArg("destination"));
                    break;
                default:
                    Console.WriteLine("mode should be either 'pack' or 'unpack'");
                    break;
            }
        }

        static void Pack(string folderToPack, string packageName, string rootDir, string ignoreRegex) {
            var pack = Package.FromDirectory(folderToPack, packageName, true, ignoreRegex);
            pack.GeneratePackage(rootDir);
        }

        static void Unpack(string packagePath, string destinationFolder) {
            var p = Package.FromPackage(packagePath);
            p.GenerateFolder(destinationFolder ?? "");
        }
    }
}