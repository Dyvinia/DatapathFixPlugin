using Frosty.Core;
using FrostySdk.Attributes;

namespace DatapathFixPlugin.Options
{

    [DisplayName("DatapathFix Options")]
    public class LaunchOptions : OptionsExtension
    {
        [Category("DatapathFix")]
        [DisplayName("Enabled")]
        [Description("Enables DatapathFix")]
        [EbxFieldMeta(FrostySdk.IO.EbxFieldType.Boolean)]
        public bool DatapathFixEnabled { get; set; } = true;

        [Category("DatapathFix")]
        [DisplayName("Check for Updates")]
        [Description("Check Github for DatapathFix updates")]
        [EbxFieldMeta(FrostySdk.IO.EbxFieldType.Boolean)]
        public bool DatapathFixUpdateCheck { get; set; } = true;

        public override void Load()
        {
            DatapathFixEnabled = Config.Get("DatapathFixEnabled", true);
            DatapathFixUpdateCheck = Config.Get("DatapathFixUpdateCheck", true);
        }

        public override void Save()
        {
            Config.Add("DatapathFixEnabled", DatapathFixEnabled);
            Config.Add("DatapathFixUpdateCheck", DatapathFixUpdateCheck);
        }
    }
}
