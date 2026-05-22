using Pixeval.Extensions.Formats.Pdf.FormatProviders;

namespace Pixeval.Extensions.Formats.Pdf.Tests;

[TestClass]
public sealed class PdfNovelFormatProviderExtensionTests
{
    [TestMethod]
    public async Task FormatNovelAsync_GeneratesPdf()
    {
        var provider = new PdfNovelFormatProviderExtension();
        var destinationPath = Path.Combine(Path.GetTempPath(), $"pixeval-pdf-test-{Guid.NewGuid():N}", "novel.pdf");
        const string ruby = "ピクセバル";
        try
        {
            using var imageStream = new MemoryStream(Convert.FromBase64String(SamplePngBase64));
            var images = new Dictionary<string, Stream>
            {
                ["1"] = imageStream
            };

            await provider.FormatNovelAsync(
                $"""
                 [chapter:Prologue]
                 Hello [[rb:Pixeval>{ruby}]].
                 [[jumpuri:Pixeval>https://github.com/Pixeval/Pixeval]]
                 [uploadedimage:1]
                 [newpage]
                 [chapter:Second]
                 Go back to [jump:1].
                 """,
                destinationPath,
                images);

            Assert.IsTrue(File.Exists(destinationPath));
            var pdfBytes = await File.ReadAllBytesAsync(destinationPath);
            CollectionAssert.AreEqual("%PDF-"u8.ToArray(), pdfBytes[..5]);
            Assert.IsGreaterThan(1_000, pdfBytes.Length);
        }
        finally
        {
            var directory = Path.GetDirectoryName(destinationPath);
            if (directory is not null && Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }

    private const string SamplePngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=";
}
