using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Formats.Images.Strings;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class WebPAnimatedImageFormatProviderExtension : AnimatedImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".webp";

    public override string FormatDescription => Resource.WebPAnimatedImageFormatLabel;

    public override Task FormatImageAsync(IReadOnlyDictionary<Stream, int> images, string destinationPath) =>
        AnimatedWebpWriter.WriteAsync(images, destinationPath);
}
