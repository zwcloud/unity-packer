using System;
using System.Collections.Generic;

namespace UnityPacker {

    class PackProgram {

        static void Main(string[] args) {

            try {
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
                    throw new ArgumentException($"Argument '{key}' is missing !");
                }

                switch (getArg("mode").ToLower()) {
                    case "p":
                    case "pck":
                    case "pack":
                        Pack(getArg("folder"), getArg("package"), getArg("root"), getArg("ignore"));
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

            } catch (ArgumentException argException) {
                Console.WriteLine("Issue with input parameters : " + argException);
                PrintHelp();
            }
        }

        static void PrintHelp() {
            Console.WriteLine("Usage:");
            Console.WriteLine("UnityPacker mode=pack folder=\"[Folder to pack]\" package=\"[Package path]\" root=\"[Root to prepend folder with]\" ignore=\"[Paths to ignore (regex)]\"");
        }

        static void Pack(string folderToPack, string packagePath, string rootDir, string ignoreRegex) {
            var pack = Packer.PackDirectory(folderToPack, true, ignoreRegex);
            pack.SaveAs(packagePath, rootDir);
        }

        static void Unpack(string packagePath, string destinationFolder) {
            var package = Packer.ReadPackage(packagePath);
            package.Extract(destinationFolder ?? "");
        }
    }
}