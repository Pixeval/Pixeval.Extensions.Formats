using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Formats.Images.Strings;
using Pixeval.Extensions.SDK.FormatProviders;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

[GeneratedComClass]
public partial class JpegImageFormatProviderExtension : StaticImageFormatProviderExtensionBase
{
    public override string FormatExtension => ".jpg";

    public override string FormatDescription => Resource.JpegImageFormatLabel;

    public override async Task FormatImageAsync(Stream imageStream, string destinationPath)
    {
        ImageWriterHelper.EnsureDestinationDirectory(destinationPath);
        imageStream.Position = 0;
        using var image = await Image.LoadAsync(imageStream);
        await image.SaveAsJpegAsync(destinationPath, new JpegEncoder());
    }
}
