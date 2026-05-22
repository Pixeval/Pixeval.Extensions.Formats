using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

internal static class AnimatedPngWriter
{
    public static async Task WriteAsync(IReadOnlyList<Stream> imageStreams, IReadOnlyList<int> delays, string destinationPath)
    {
        ImageWriterHelper.EnsureDestinationDirectory(destinationPath);

        using var animatedImage = await AnimatedImageFactory.CreateAsync(
            imageStreams,
            delays,
            static image =>
            {
                var metadata = image.Metadata.GetPngMetadata();
                metadata.RepeatCount = 0;
                metadata.AnimateRootFrame = true;
            },
            ApplyFrameMetadata);
        await animatedImage.SaveAsPngAsync(destinationPath, new PngEncoder());
    }

    private static void ApplyFrameMetadata(ImageFrame frame, int index, IReadOnlyList<int> delays)
    {
        var metadata = frame.Metadata.GetPngMetadata();
        metadata.FrameDelay = new Rational((uint)ImageWriterHelper.GetDelayMilliseconds(delays, index), 1000);
        metadata.BlendMethod = PngBlendMethod.Source;
        metadata.DisposalMethod = PngDisposalMethod.DoNotDispose;
    }
}
