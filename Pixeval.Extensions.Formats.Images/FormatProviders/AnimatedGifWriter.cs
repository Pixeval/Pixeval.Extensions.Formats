using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

internal static class AnimatedGifWriter
{
    public static async Task WriteAsync(IReadOnlyList<Stream> imageStreams, IReadOnlyList<int> delays, string destinationPath)
    {
        ImageWriterHelper.EnsureDestinationDirectory(destinationPath);

        using var animatedImage = await AnimatedImageFactory.CreateAsync(
            imageStreams,
            delays,
            static image => image.Metadata.GetGifMetadata().RepeatCount = 0,
            ApplyFrameMetadata);
        await animatedImage.SaveAsGifAsync(destinationPath, new GifEncoder());
    }

    private static void ApplyFrameMetadata(ImageFrame frame, int index, IReadOnlyList<int> delays)
    {
        var metadata = frame.Metadata.GetGifMetadata();
        metadata.FrameDelay = ImageWriterHelper.GetGifDelayCentiseconds(delays, index);
        metadata.DisposalMethod = GifDisposalMethod.NotDispose;
    }
}
