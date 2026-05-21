using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Formats.Pdf.FormatProviders;
using Pixeval.Extensions.SDK;

namespace Pixeval.Extensions.Formats.Pdf;

[GeneratedComClass]
public partial class ExtensionsHost : ExtensionsHostBase
{
    public override string ExtensionName => "Pixeval PDF Format";

    public override string AuthorName => "Poker";

    public override string ExtensionLink => "https://github.com/Pixeval/Pixeval";

    public override string HelpLink => ExtensionLink;

    public override string Description => "Provides PDF output for Pixiv novel downloads.";

    public override string Version => "1.0.0";

    public override IExtension[] Extensions { get; } =
    [
        new PdfNovelFormatProviderExtension()
    ];

    public override byte[]? Icon => null;

    public static ExtensionsHost Current { get; } = new();

    [UnmanagedCallersOnly(EntryPoint = nameof(DllGetExtensionsHost))]
    private static unsafe int DllGetExtensionsHost(void** ppv) => DllGetExtensionsHost(ppv, Current);
}
