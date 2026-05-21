using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using FluentIcons.Common;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Pdf.FormatProviders;

[GeneratedComClass]
public partial class PdfNovelFormatProviderExtension : NovelFormatProviderExtensionBase
{
    public override string FormatExtension => ".pdf";

    public override string FormatDescription => "PDF";

    public override Symbol Icon => Symbol.DocumentPdf;

    public override string Label => "PDF";

    public override string Description => "Exports Pixiv novels as PDF files.";

    public override Task FormatNovelAsync(string novelInput, string destinationPath, IReadOnlyDictionary<string, Stream> images)
    {
        var writer = new PixivNovelPdfWriter(images);
        writer.Write(novelInput, destinationPath);
        return Task.CompletedTask;
    }
}
