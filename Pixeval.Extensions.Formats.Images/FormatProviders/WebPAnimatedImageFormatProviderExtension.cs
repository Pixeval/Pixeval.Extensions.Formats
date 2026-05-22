using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using FluentIcons.Common;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class WebPAnimatedImageFormatProviderExtension : AnimatedImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".webp";

    public override string FormatDescription => "Animated WebP";

    public override Symbol Icon => Symbol.Gif;

    public override string Label => "Animated WebP";

    public override string Description => "Exports animated images as WebP files.";

    public override Task FormatImageAsync(IReadOnlyList<Stream> imageStreams, IReadOnlyList<int> delays, string destinationPath) =>
        AnimatedWebpWriter.WriteAsync(imageStreams, delays, destinationPath);
}
