using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using FluentIcons.Common;
using Pixeval.Extensions.Formats.Pdf.Strings;
using Pixeval.Extensions.SDK.FormatProviders;

namespace Pixeval.Extensions.Formats.Pdf.FormatProviders;

[GeneratedComClass]
public partial class PdfNovelFormatProviderExtension : NovelFormatProviderExtensionBase
{
    public override string FormatExtension => ".pdf";

    public override string FormatDescription => Resource.PdfNovelFormatLabel;

    public override Task FormatNovelAsync(string novelInput, string destinationPath, IReadOnlyDictionary<string, Stream> images)
    {
        var writer = new PixivNovelPdfWriter(images);
        writer.Write(novelInput, destinationPath);
        return Task.CompletedTask;
    }
}
