using System;
using System.IO;

namespace UnityPacker {

    class UnityPacker {

        static void Main(string[] args) {

            try
            {
                Logger.Log("Starting...");

                Logger.Log("Current Directory : " + Directory.GetCurrentDirectory());

                Logger.Log("Arguments : ");
                for (int i = 0; i < args.Length; i++)
                {
                    Logger.Log($"- {args[i]}");
                    args[i] = args[i].Standardize();
                }

                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "log":
                            Logger.Enabled = true;
                            break;
                        case "mode":
                            mode = args[++i];
                            break;
                        case "folder":
                            folderToPack = args[++i];
                            break;
                        case "package":
                            packagePath = args[++i];
                            break;
                        case "root":
                            rootDir = args[++i];
                            break;
                        case "ignore":
                            ignoreRegex = args[++i];
                            break;
                        case "destination":
                            destinationFolder = args[++i];
                            break;
                        default:
                            Logger.Log($"Unrecognized command : '{args[i]}'");
                            break;
                    }
                }

                switch (mode)
                {
                    case "pack":
                        Pack();
                        break;
                    case "unpack":
                        Unpack();
                        break;
                    default:
                        throw new ArgumentException();
                }

                Logger.Log("Done !");

            }
            catch (ArgumentException argException)
            {
                Logger.Log("Issue with input parameters : " + argException);
                PrintHelp();
                Environment.Exit(-1);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Environment.Exit(-2);
            }
        }

        static void PrintHelp() {
            Logger.Log("Usage:");
            Logger.Log("UnityPacker mode=pack folder=\"[Folder to pack]\" package=\"[Package path]\" root=\"[Root to prepend folder with]\" ignore=\"[Paths to ignore (regex)]\"");
        }

        static void Pack() {
            Logger.Log("Packing...");
            Package package = Packer.PackDirectory(folderToPack, true, ignoreRegex);
            package.SaveAs(packagePath, rootDir);
        }

        static void Unpack() {
            Logger.Log("Unpacking...");
            Package package = Packer.ReadPackage(packagePath);
            package.Extract(destinationFolder ?? "");
        }

        private static string mode;
        private static string folderToPack;
        private static string rootDir;
        private static string ignoreRegex;
        private static string packagePath;
        private static string destinationFolder;
    }
}