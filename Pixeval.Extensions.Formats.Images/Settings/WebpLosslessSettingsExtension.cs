using System.Runtime.InteropServices.Marshalling;
using FluentIcons.Common;
using Pixeval.Extensions.SDK.Settings;

namespace Pixeval.Extensions.Formats.Images.Settings;

[GeneratedComClass]
public partial class WebpLosslessSettingsExtension : BoolSettingsExtensionBase
{
    public override Symbol Icon => Symbol.ImageSparkle;

    public override string Token => "ImagesWebpLossless";

    public override string Label => "WebP Lossless";

    public override string Description => "Controls whether generated WebP files use lossless compression. Applies to both static and animated WebP exports.";

    public override bool DefaultValue => true;

    public override void OnValueChanged(bool value)
    {
        ImageFormatsSettings.WebpLossless = value;
    }
}
