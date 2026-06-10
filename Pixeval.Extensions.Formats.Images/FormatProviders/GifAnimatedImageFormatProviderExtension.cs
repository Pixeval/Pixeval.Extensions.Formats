using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Formats.Images.Strings;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class GifAnimatedImageFormatProviderExtension : AnimatedImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".gif";

    public override string FormatDescription => Resource.GifAnimatedImageFormatLabel;

    public override Task FormatImageAsync(IReadOnlyDictionary<Stream, int> images, string destinationPath) =>
        AnimatedGifWriter.WriteAsync(images, destinationPath);
}
