using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

internal static class AnimatedImageFactory
{
    public static async Task<Image> CreateAsync(
        IReadOnlyDictionary<Stream, int> images,
        Action<Image> initializeImageMetadata,
        Action<ImageFrame, int, IReadOnlyList<int>> applyFrameMetadata)
    {
        if (images.Count is 0)
            throw new ArgumentException("At least one image stream is required.", nameof(images));

        var imageStreams = new List<Stream>(images.Keys);
        var delays = new List<int>(images.Values);

        imageStreams[0].Position = 0;
        var image = await Image.LoadAsync(imageStreams[0]);
        applyFrameMetadata(image.Frames.RootFrame, 0, delays);
        initializeImageMetadata(image);
        try
        {
            for (var i = 1; i < imageStreams.Count; i++)
            {
                imageStreams[i].Position = 0;
                using var frameImage = await Image.LoadAsync(imageStreams[i]);
                var rootFrame = frameImage.Frames.RootFrame;
                applyFrameMetadata(rootFrame, i, delays);
                _ = image.Frames.AddFrame(rootFrame);
            }

            return image;
        }
        catch
        {
            image.Dispose();
            throw;
        }
    }
}
