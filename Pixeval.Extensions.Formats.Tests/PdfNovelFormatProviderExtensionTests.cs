using Pixeval.Extensions.Formats.Pdf.FormatProviders;
using QuestPDF.Fluent;

namespace Pixeval.Extensions.Formats.Tests;

[TestClass]
public sealed class PdfNovelFormatProviderExtensionTests
{
    [TestMethod]
    public void CreateDocument_GeneratePdfAndShow()
    {
        using var imageStream = new MemoryStream(Convert.FromBase64String(SamplePngBase64));
        var images = new Dictionary<string, Stream>
        {
            ["1"] = imageStream
        };

        var document = PdfNovelFormatProviderExtension.CreateDocument(
            $"""
             [chapter:Prologue]
             Hello [[rb:Pixeval>ピクセバル]].
             [[jumpuri:Pixeval>https://github.com/Pixeval/Pixeval]]
             [uploadedimage:1]
             [newpage]
             [chapter:Second]
             Go back to [jump:1].
             """,
            images);

        document.GeneratePdfAndShow();
    }

    private const string SamplePngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=";
}
