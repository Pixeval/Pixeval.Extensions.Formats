using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Formats.Images.Strings;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class APngAnimatedImageFormatProviderExtension : AnimatedImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".png";

    public override string FormatDescription => Resource.APngAnimatedImageFormatLabel;

    public override Task FormatImageAsync(IReadOnlyDictionary<Stream, int> images, string destinationPath) =>
        AnimatedPngWriter.WriteAsync(images, destinationPath);
}
