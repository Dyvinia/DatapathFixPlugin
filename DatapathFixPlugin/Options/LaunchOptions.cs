using Frosty.Core;
using FrostySdk.Attributes;
using FrostySdk.IO;

namespace DatapathFixPlugin.Options {

    [DisplayName("DatapathFix Options")]
    public class LaunchOptions : OptionsExtension {
        [Category("DatapathFix")]
        [DisplayName("Enabled")]
        [Description("Enables DatapathFix")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool DatapathFixEnabled { get; set; } = true;

        [Category("DatapathFix")]
        [DisplayName("Check for Updates")]
        [Description("Check Github for DatapathFix updates")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool DatapathFixUpdateCheck { get; set; } = true;

        [Category("Debugging")]
        [DisplayName("Debug Mode")]
        [Description("Used for Debugging\nDo not use unless directed to by a developer/etc")]
        [EbxFieldMeta(EbxFieldType.Boolean)]
        public bool DatapathFixDebugMode { get; set; } = false;

        public override void Load() {
            DatapathFixEnabled = Config.Get("DatapathFixEnabled", true);
            DatapathFixUpdateCheck = Config.Get("DatapathFixUpdateCheck", true);
            DatapathFixDebugMode = Config.Get("DatapathFixDebugMode", false);
        }

        public override void Save() {
            Config.Add("DatapathFixEnabled", DatapathFixEnabled);
            Config.Add("DatapathFixUpdateCheck", DatapathFixUpdateCheck);
            Config.Add("DatapathFixDebugMode", DatapathFixDebugMode);

            // Prevent LaunchPlatformPlugin from being used at the same time at DPFix
            if (DatapathFixEnabled && Config.Get("PlatformLaunchingEnabled", false, ConfigScope.Game)) {
                Config.Add("PlatformLaunchingEnabled", false, ConfigScope.Game);
            }
        }
    }
}
