using System;
using System.IO;

namespace UnityPacker {

    public static class Logger {

        public static bool Enabled { get; set; }

        private static StreamWriter streamWriter;
        private static StreamWriter StreamWriter {
            get {
                if (streamWriter == null) {
                    string directory = Environment.ExpandEnvironmentVariables(@"%appdata%/UnityPacker");
                    Directory.CreateDirectory(directory);
                    var fs = new FileStream(Path.Combine(directory, "session.log"), FileMode.Create);
                    streamWriter = new StreamWriter(fs);
                    streamWriter.AutoFlush = true;
                }
                return streamWriter;
            }
        }

        public static void Log(object message) {
            if (message == null)
                return;
            Console.WriteLine(message);
            if (Enabled) {
                StreamWriter.WriteLine(message);
            }
        }
    }
}
