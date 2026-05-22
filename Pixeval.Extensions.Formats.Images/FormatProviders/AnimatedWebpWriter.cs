using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using Pixeval.Extensions.Formats.Images.Settings;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

internal static class AnimatedWebpWriter
{
    public static async Task WriteAsync(IReadOnlyList<Stream> imageStreams, IReadOnlyList<int> delays, string destinationPath)
    {
        ImageWriterHelper.EnsureDestinationDirectory(destinationPath);

        using var animatedImage = await AnimatedImageFactory.CreateAsync(
            imageStreams,
            delays,
            static image => image.Metadata.GetWebpMetadata().RepeatCount = 0,
            ApplyFrameMetadata);
        await animatedImage.SaveAsWebpAsync(destinationPath, CreateEncoder());
    }

    private static void ApplyFrameMetadata(ImageFrame frame, int index, IReadOnlyList<int> delays)
    {
        var delay = ImageWriterHelper.GetDelayMilliseconds(delays, index);
        var metadata = frame.Metadata.GetWebpMetadata();
        metadata.FrameDelay = (uint) delay;
        metadata.BlendMethod = WebpBlendMethod.Source;
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
