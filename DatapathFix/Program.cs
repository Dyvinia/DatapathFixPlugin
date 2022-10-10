using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DatapathFix {
    internal class Program {
        static void Main(string[] args) {
            string currentPath = Assembly.GetExecutingAssembly().Location;
            string origPath = currentPath.Replace(".exe", ".orig.exe");

            if (File.Exists("tmp") && File.Exists(origPath)) {
                string dataPathArg = File.ReadAllText("tmp");

                if (args.Length == 0) {
                    File.Move(currentPath, currentPath.Replace(".exe", ".old"));
                    File.Move(origPath, currentPath);

                    Process.Start(new ProcessStartInfo {
                        FileName = currentPath,
                        WorkingDirectory = Environment.CurrentDirectory,
                        Arguments = dataPathArg,
                        UseShellExecute = false
                    });
                }
                else {
                    Process.Start(new ProcessStartInfo {
                        FileName = origPath,
                        WorkingDirectory = Environment.CurrentDirectory,
                        UseShellExecute = false
                    });
                }
            }
        }
    }
}
