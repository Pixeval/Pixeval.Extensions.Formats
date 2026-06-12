using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Pixeval.Extensions.Formats.Pdf.Strings;
using Pixeval.Extensions.SDK.FormatProviders;
using QuestPDF.Fluent;

namespace Pixeval.Extensions.Formats.Pdf.FormatProviders;

[GeneratedComClass]
public partial class PdfNovelFormatProviderExtension : NovelFormatProviderExtensionBase
{
    public override string FormatExtension => ".pdf";

    public override string FormatDescription => Resource.PdfNovelFormatLabel;

    public override Task FormatNovelAsync(string novelInput, string destinationPath, IReadOnlyDictionary<string, Stream> images)
    {
        var document = CreateDocument(novelInput, images);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        document.GeneratePdf(destinationPath);
        return Task.CompletedTask;
    }

    public static Document CreateDocument(string novelInput, IReadOnlyDictionary<string, Stream> images)
    {
        var writer = new PixivNovelPdfWriter(images);
        return writer.CreateDocument(novelInput);
    }
}
