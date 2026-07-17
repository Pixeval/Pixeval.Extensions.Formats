using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Formats.Pdf.FormatProviders;
using Pixeval.Extensions.Formats.Pdf.Strings;
using Pixeval.Extensions.SDK;

namespace Pixeval.Extensions.Formats.Pdf;

[GeneratedComClass]
public partial class ExtensionsHost : ExtensionsHostBase
{
    public override string ExtensionName => Resource.ExtensionHostName;

    public override string AuthorName => "Poker";

    public override string ExtensionLink => "https://github.com/Pixeval/Pixeval.Extensions.Formats";

    public override string HelpLink => ExtensionLink;

    public override string Description => Resource.ExtensionHostDescription;

    public override string Version => "1.0.0";

    public override IExtension[] Extensions { get; } =
    [
        new PdfNovelFormatProviderExtension()
    ];

    public override byte[]? Icon
    {
        get
        {
            var stream = typeof(ExtensionsHost).Assembly.GetManifestResourceStream("logo");
            if (stream is null)
                return null;
            var array = new byte[stream.Length];
            _ = stream.Read(array);
            return array;
        }
    }

    public static ExtensionsHost Current { get; } = new();

    [UnmanagedCallersOnly(EntryPoint = nameof(GetExtensionsHost))]
    private static unsafe int GetExtensionsHost(void** ppv) => GetExtensionsHost(ppv, Current);
}
