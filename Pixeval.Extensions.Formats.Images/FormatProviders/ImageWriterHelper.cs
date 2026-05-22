using System;
using System.Collections.Generic;
using System.IO;

namespace Pixeval.Extensions.Formats.Images.FormatProviders;

internal static class ImageWriterHelper
{
    public static void EnsureDestinationDirectory(string destinationPath)
    {
        if (Path.GetDirectoryName(destinationPath) is { Length: > 0 } directory)
            _ = Directory.CreateDirectory(directory);
    }

    public static int GetDelayMilliseconds(IReadOnlyList<int> delays, int index) =>
        delays.Count > index && delays[index] > 0 ? delays[index] : 10;

    public static int GetGifDelayCentiseconds(IReadOnlyList<int> delays, int index) =>
        Math.Max(1, (GetDelayMilliseconds(delays, index) + 5) / 10);
}
