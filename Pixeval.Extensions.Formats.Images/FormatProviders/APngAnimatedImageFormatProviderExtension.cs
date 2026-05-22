using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using FluentIcons.Common;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class APngAnimatedImageFormatProviderExtension : AnimatedImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".png";

    public override string FormatDescription => "APNG";

    public override Symbol Icon => Symbol.Gif;

    public override string Label => "APNG";

    public override string Description => "Exports animated images as animated PNG files.";

    public override Task FormatImageAsync(IReadOnlyList<Stream> imageStreams, IReadOnlyList<int> delays, string destinationPath) =>
        AnimatedPngWriter.WriteAsync(imageStreams, delays, destinationPath);
}
