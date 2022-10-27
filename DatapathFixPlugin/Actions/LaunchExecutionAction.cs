using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Frosty.Core;
using FrostySdk;
using FrostySdk.Interfaces;
using Frosty.Core.Attributes;
using Newtonsoft.Json;
using Frosty.Controls;
using System.Media;

namespace DatapathFixPlugin.Actions
{
    public class LaunchExecutionAction : ExecutionAction
    {
        public string Game => Path.Combine(App.FileSystem.BasePath, $"{ProfilesLibrary.ProfileName}.exe");

        public string DatapathFix = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DatapathFix.exe");

        public Version CurrentVersion = new Version(Assembly.GetExecutingAssembly().GetCustomAttribute<PluginVersionAttribute>().Version);

        public override Action<ILogger, PluginManagerType, CancellationToken> PreLaunchAction => new Action<ILogger, PluginManagerType, CancellationToken>((ILogger logger, PluginManagerType type, CancellationToken cancelToken) =>
        {
            if (Config.Get("DatapathFixEnabled", true) && File.Exists(DatapathFix))
            {
                ResetGameDirectory();

                Thread.Sleep(1000);

                string cmdArgs = $"-dataPath \"{Path.Combine(App.FileSystem.BasePath, $"ModData\\{App.SelectedPack}")}\" ";
                cmdArgs += Config.Get("CommandLineArgs", "", ConfigScope.Game);

                try
                {
                    File.WriteAllText(Path.Combine(App.FileSystem.BasePath, "tmp"), cmdArgs);
                    File.Move(Game, Game.Replace(".exe", ".orig.exe"));
                    File.Copy(DatapathFix, Game, true);
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
            if (Config.Get("DatapathFixUpdateCheck", true) && CheckUpdates().Result)
            {
                Task.Run(() =>
                {
                    SystemSounds.Exclamation.Play();
                    MessageBoxResult mbResult = FrostyMessageBox.Show("You are using an outdated version of DatapathFix." + Environment.NewLine + "Would you like to download the latest version?", "DatapathFixPlugin", MessageBoxButton.YesNo);
                    if (mbResult == MessageBoxResult.Yes)
                    {
                        Process.Start("https://github.com/Dyvinia/DatapathFixPlugin/releases/latest");
                    }
                });
            }
        });

        private void ResetGameDirectory()
        {
            try
            {
                File.Delete(Game.Replace(".exe", ".old"));
                File.Delete(Path.Combine(App.FileSystem.BasePath, "tmp"));
            }
            catch (Exception ex)
            {
                App.Logger.LogWarning(ex.Message);
            }

            try
            {
                if (File.Exists(Game.Replace(".exe", ".orig.exe")))
                {
                    File.Delete(Game);
                    File.Move(Game.Replace(".exe", ".orig.exe"), Game);
                }
            }
            catch (Exception ex)
            {
                App.Logger.LogWarning(ex.Message);
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

        private class Release
        {
            public string Name;

            [JsonProperty(PropertyName = "tag_name")]
            public string Tag;
        }

        private async Task<bool> CheckUpdates()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "request");

                    Version latestVersion = new Version(JsonConvert.DeserializeObject<Release>(await client.GetStringAsync($"https://api.github.com/repos/Dyvinia/DatapathFixPlugin/releases/latest")).Tag.Substring(1));
                    if (CurrentVersion.CompareTo(latestVersion) < 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
