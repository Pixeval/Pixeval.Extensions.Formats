using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using FluentIcons.Common;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class WebPImageFormatProviderExtension : StaticImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".webp";

    public override string FormatDescription => "WebP";

    public override Symbol Icon => Symbol.Image;

    public override string Label => "WebP";

    public override string Description => "Exports static images as WebP files.";

    public override Task FormatImageAsync(Stream imageStream, string destinationPath) =>
        WebPWriter.WriteAsync(imageStream, destinationPath);
}
