using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using FluentIcons.Common;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class GifAnimatedImageFormatProviderExtension : AnimatedImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".gif";

    public override string FormatDescription => "GIF";

    public override Symbol Icon => Symbol.Gif;

    public override string Label => "GIF";

    public override string Description => "Exports animated images as GIF files.";

    public override Task FormatImageAsync(IReadOnlyList<Stream> imageStreams, IReadOnlyList<int> delays, string destinationPath) =>
        AnimatedGifWriter.WriteAsync(imageStreams, delays, destinationPath);
}
