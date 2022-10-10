using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Frosty.Core;
using FrostySdk;
using FrostySdk.Interfaces;

namespace DatapathFixPlugin.Actions
{

    public class LaunchExecutionAction : ExecutionAction
    {
        public string DatapathFix = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DatapathFix.exe");

        public override Action<ILogger, PluginManagerType, CancellationToken> PreLaunchAction => new Action<ILogger, PluginManagerType, CancellationToken>((ILogger logger, PluginManagerType type, CancellationToken cancelToken) =>
        {
            string game = Path.Combine(App.FileSystem.BasePath, $"{ProfilesLibrary.ProfileName}.exe");
            ResetGameDirectory(game);
            Thread.Sleep(1000);

            if (Config.Get("DatapathFixEnabled", true) && File.Exists(DatapathFix))
            {
                string cmdArgs = $"-dataPath \"{Path.Combine(App.FileSystem.BasePath, $"ModData\\{App.SelectedPack}")}\"";

                try
                {
                    File.WriteAllText(Path.Combine(App.FileSystem.BasePath, "tmp"), cmdArgs);
                    File.Move(game, game.Replace(".exe", ".orig.exe"));
                    File.Copy(DatapathFix, game, true);
                }
                catch (Exception ex)
                {
                    App.Logger.LogError(ex.Message);
                }

                Thread.Sleep(1000);
            }
            else if (!File.Exists(DatapathFix))
            {
                App.Logger.LogError("Cannot find DatapathFix.exe");
            }
        });

        public override Action<ILogger, PluginManagerType, CancellationToken> PostLaunchAction => new Action<ILogger, PluginManagerType, CancellationToken>((ILogger logger, PluginManagerType type, CancellationToken cancelToken) =>
        {
            if (Config.Get("DatapathFixEnabled", true) && File.Exists(DatapathFix))
            {
                string game = Path.Combine(App.FileSystem.BasePath, $"{ProfilesLibrary.ProfileName}.exe");

                logger.Log("Waiting For Game");
                Thread.Sleep(4000);
                WaitForProcess(game);

                ResetGameDirectory(game);
            }
        });

        private void ResetGameDirectory(string game)
        {
            try
            {
                File.Delete(game.Replace(".exe", ".old"));
                File.Delete(Path.Combine(App.FileSystem.BasePath, "tmp"));
            }
            catch (Exception ex)
            {
                App.Logger.LogError(ex.Message);
            }
        }

        private void WaitForProcess(string name)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            // Check only for the next 8 seconds to prevent lockup
            while (s.Elapsed < TimeSpan.FromSeconds(8))
            {
                Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(name));

                if (processes.Length > 0)
                {
                    return;
                }
            }
        }
    }
}
