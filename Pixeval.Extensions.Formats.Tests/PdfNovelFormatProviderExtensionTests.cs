using System.Text;
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
            ["cover.png"] = imageStream,
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

    [TestMethod]
    public void CreateDocument_WithCover_GeneratesSeparateFirstPage()
    {
        using var coverStream = new MemoryStream(Convert.FromBase64String(SamplePngBase64));
        var images = new Dictionary<string, Stream>
        {
            ["cover.png"] = coverStream
        };

        var document = PdfNovelFormatProviderExtension.CreateDocument("Novel text", images);
        var pdf = document.GeneratePdf();
        var pdfText = Encoding.ASCII.GetString(pdf);

        Assert.IsTrue(CountPdfPages(pdfText) >= 2);
    }

    private static int CountPdfPages(string pdfText)
    {
        const string pageType = "/Type /Page";
        var count = 0;
        var index = 0;
        while ((index = pdfText.IndexOf(pageType, index, StringComparison.Ordinal)) >= 0)
        {
            var next = index + pageType.Length;
            if (next == pdfText.Length || char.IsWhiteSpace(pdfText[next]))
                ++count;
            index = next;
        }

        return count;
    }

    private const string SamplePngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=";
}
