using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Formats.Images.Strings;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class WebPImageFormatProviderExtension : StaticImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".webp";

    public override string FormatDescription => Resource.WebPImageFormatLabel;

    public override Task FormatImageAsync(Stream imageStream, string destinationPath) =>
        WebPWriter.WriteAsync(imageStream, destinationPath);
}
