using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using FluentIcons.Common;
using Pixeval.Extensions.SDK.FormatProviders;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class JpegImageFormatProviderExtension : StaticImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".jpg";

    public override string FormatDescription => "JPG";

    public override Symbol Icon => Symbol.Image;

    public override string Label => "JPG";

    public override string Description => "Exports static images as JPEG files.";

    public override async Task FormatImageAsync(Stream imageStream, string destinationPath)
    {
        ImageWriterHelper.EnsureDestinationDirectory(destinationPath);
        imageStream.Position = 0;
        using var image = await Image.LoadAsync(imageStream);
        await image.SaveAsJpegAsync(destinationPath, new JpegEncoder());
    }
}
