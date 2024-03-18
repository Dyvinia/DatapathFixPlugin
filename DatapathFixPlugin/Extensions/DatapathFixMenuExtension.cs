using System;
using System.IO;
using Frosty.Core;
using FrostySdk;

namespace DatapathFixPlugin.Extensions {
    public class DatapathFixMenuExtension : MenuExtension {
        public string FSBasePath {
            get {
                dynamic fileSystem = typeof(App).GetField("FileSystem")?.GetValue(this) ?? typeof(App).GetField("FileSystemManager")?.GetValue(this);
                return fileSystem.BasePath;
            }
        }

        public string Game => Path.Combine(FSBasePath, $"{ProfilesLibrary.ProfileName}.exe");
        public string Par => Path.Combine(FSBasePath, $"{ProfilesLibrary.ProfileName}.par");

        public override string TopLevelMenuName => "Tools";
        public override string SubLevelMenuName => "DatapathFix";

        public override string MenuItemName => "Reset Game Installation";

        public override RelayCommand MenuItemClicked => new RelayCommand((o) => {
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
        });
    }
}
