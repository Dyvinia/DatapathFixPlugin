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

namespace DatapathFixPlugin.Actions {
    public class LaunchExecutionAction : ExecutionAction {
        public string FSBasePath {
            get {
                dynamic fileSystem = typeof(App).GetField("FileSystem")?.GetValue(this) ?? typeof(App).GetField("FileSystemManager")?.GetValue(this);
                return fileSystem.BasePath;
            }
        }

        public string Game => Path.Combine(FSBasePath, $"{ProfilesLibrary.ProfileName}.exe");
        public string Par => Path.Combine(FSBasePath, $"{ProfilesLibrary.ProfileName}.par");

        public static string DatapathFix {
            get {
                string fileName = "DatapathFix.exe";
                if (Config.Get("DatapathFixDebugMode", false)) {
                    fileName = "DatapathFix(DEBUG).exe";
                }
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DatapathFix", fileName);
            }
        }

        public Version CurrentVersion = new Version(Assembly.GetExecutingAssembly().GetCustomAttribute<PluginVersionAttribute>().Version);

        public override Action<ILogger, PluginManagerType, CancellationToken> PreLaunchAction => new Action<ILogger, PluginManagerType, CancellationToken>((ILogger logger, PluginManagerType type, CancellationToken cancelToken) => {
            if (Config.Get("DatapathFixEnabled", true) && File.Exists(DatapathFix)) {
                ResetGameDirectory();

                Thread.Sleep(1000);

                string cmdArgs = $"-dataPath \"{Path.Combine(FSBasePath, $"ModData\\{App.SelectedPack}")}\" ";
                cmdArgs += Config.Get("CommandLineArgs", "", ConfigScope.Game);

                try {
                    File.WriteAllText(Path.Combine(FSBasePath, "tmp"), cmdArgs);
                    File.Move(Game, Game.Replace(".exe", ".orig.exe"));
                    if (File.Exists(Par))
                        File.Copy(Par, Par.Replace(".par", ".orig.par"), true);
                    File.Copy(DatapathFix, Game, true);
                }
                catch (Exception ex) {
                    App.Logger.LogError(ex.Message);
                }

                Thread.Sleep(1000);
            }
            else if (!File.Exists(DatapathFix)) {
                App.Logger.LogError($"Cannot find {DatapathFix}");

                Task.Run(() => {
                    SystemSounds.Exclamation.Play();
                    FrostyMessageBox.Show($"Cannot find \\Plugins\\DatapathFix\\{Path.GetFileName(DatapathFix)}", "DatapathFixPlugin", MessageBoxButton.OK);
                });
            }
        });

        private void ResetGameDirectory() {
            try {
                File.Delete(Path.Combine(FSBasePath, "tmp"));
                File.Delete(Par.Replace(".par", ".orig.par"));

                // only delete game.old if it is less than 1MB to ensure it does not delete the actual game
                string gameOld = Game.Replace(".exe", ".old");
                if (File.Exists(gameOld) && new FileInfo(gameOld).Length < 1000000)
                    File.Delete(gameOld);
            }
            catch (Exception ex) {
                App.Logger.LogWarning(ex.Message);
            }

            try {
                if (File.Exists(Game.Replace(".exe", ".orig.exe")) && new FileInfo(Game).Length < 1000000) {
                    File.Delete(Game);
                    File.Move(Game.Replace(".exe", ".orig.exe"), Game);
                }
            }
            catch (Exception ex) {
                App.Logger.LogWarning(ex.Message);
            }
        }

        private class Release {
            [JsonProperty(PropertyName = "name")]
            public string Name;

            [JsonProperty(PropertyName = "tag_name")]
            public string Tag;
        }

        private async void CheckUpdates() {
            try {
                using (HttpClient client = new HttpClient()) {
                    client.DefaultRequestHeaders.Add("User-Agent", "request");

                    Version latestVersion = new Version(JsonConvert.DeserializeObject<Release>(await client.GetStringAsync($"https://api.github.com/repos/Dyvinia/DatapathFixPlugin/releases/latest")).Tag.Substring(1));

                    if (CurrentVersion.CompareTo(latestVersion) < 0) {
                        await Task.Run(() => {
                            SystemSounds.Exclamation.Play();
                            MessageBoxResult mbResult = FrostyMessageBox.Show("You are using an outdated version of DatapathFix." + Environment.NewLine + "Would you like to download the latest version?", "DatapathFixPlugin", MessageBoxButton.YesNo);
                            if (mbResult == MessageBoxResult.Yes)
                                Process.Start("https://github.com/Dyvinia/DatapathFixPlugin/releases/latest");
                        });
                    }
                }
            }
            catch { }
        }

        public LaunchExecutionAction() {
            App.Logger.Log($"DatapathFix v{CurrentVersion} by Dyvinia");
            App.Logger.Log(@"Github: https://github.com/Dyvinia/DatapathFixPlugin");
            App.Logger.Log(@"Donate: https://ko-fi.com/Dyvinia");
            App.Logger.Log(@"Note: This is only needed for Steam or Epic Games Store; no longer needed when using only the EA App");

            if (Config.Get("DatapathFixUpdateCheck", true))
                CheckUpdates();

            ExtractDatapathFix();
        }

        public static void ExtractDatapathFix() {
            if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DatapathFix")))
                Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DatapathFix"));

            if (File.Exists(DatapathFix))
                File.Delete(DatapathFix);

            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("DatapathFixPlugin.DatapathFix.DatapathFix.exe")) {
                using (FileStream f = File.OpenWrite(DatapathFix)) {
                    s.CopyTo(f);
                    s.Close();
                    f.Close();
                }
            }
        }
    }
}
