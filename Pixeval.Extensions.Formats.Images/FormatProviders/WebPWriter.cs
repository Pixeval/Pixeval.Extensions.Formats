using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using Pixeval.Extensions.Formats.Images.Settings;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

internal static class WebPWriter
{
    public static async Task WriteAsync(Stream imageStream, string destinationPath)
    {
        ImageWriterHelper.EnsureDestinationDirectory(destinationPath);

        imageStream.Position = 0;
        using var image = await Image.LoadAsync(imageStream);
        await image.SaveAsWebpAsync(destinationPath, CreateEncoder());
    }

    private static WebpEncoder CreateEncoder() =>
        ImageFormatsSettings.WebpLossless
            ? new WebpEncoder
            {
                FileFormat = WebpFileFormatType.Lossless
            }
            : new WebpEncoder
            {
                FileFormat = WebpFileFormatType.Lossy,
                Quality = 75
            };
}
