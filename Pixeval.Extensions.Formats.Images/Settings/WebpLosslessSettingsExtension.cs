using System.Runtime.InteropServices.Marshalling;
using FluentIcons.Common;
using Pixeval.Extensions.SDK.Settings;
using Pixeval.Extensions.Formats.Images.Strings;

namespace Pixeval.Extensions.Formats.Images.Settings;

[GeneratedComClass]
public partial class WebpLosslessSettingsExtension : BoolSettingsExtensionBase
{
    public override Symbol Icon => Symbol.ImageSparkle;

    public override string Token => "ImagesWebpLossless";

    public override string Label => Resource.WebpLosslessSettingsLabel;

    public override string Description => Resource.WebpLosslessSettingsDescription;

    public override bool DefaultValue => true;

    public override void OnValueChanged(bool value)
    {
        ImageFormatsSettings.WebpLossless = value;
    }
}
