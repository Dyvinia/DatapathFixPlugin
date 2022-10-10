using Frosty.Core;
using FrostySdk.Attributes;

namespace DatapathFixPlugin.Options
{

    [DisplayName("Launch Options")]
    public class LaunchOptions : OptionsExtension
    {
        [Category("General")]
        [Description("Enables the DatapathFix system for EA Desktop.")]
        [DisplayName("DatapathFix Enabled")]
        [EbxFieldMeta(FrostySdk.IO.EbxFieldType.Boolean)]
        public bool DatapathFixEnabled { get; set; } = true;

        public override void Load()
        {
            DatapathFixEnabled = Config.Get("DatapathFixEnabled", true);
        }

        public override void Save()
        {
            Config.Add("DatapathFixEnabled", DatapathFixEnabled);
        }
    }
}
