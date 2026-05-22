using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Formats.Images.FormatProviders;
using Pixeval.Extensions.Formats.Images.Settings;
using Pixeval.Extensions.SDK;

namespace Pixeval.Extensions.Formats.Images;

[GeneratedComClass]
public partial class ExtensionsHost : ExtensionsHostBase
{
    public override string ExtensionName => "Pixeval Image Formats";

    public override string AuthorName => "Poker";

    public override string ExtensionLink => "https://github.com/Pixeval/Pixeval.Extensions.Formats";

    public override string HelpLink => ExtensionLink;

    public override string Description => "Provides JPEG, PNG, APNG, GIF, and WebP output for image downloads.";

    public override string Version => "1.0.0";

    public override IExtension[] Extensions { get; } =
    [
        new WebpLosslessSettingsExtension(),
        new JpegImageFormatProviderExtension(),
        new PngImageFormatProviderExtension(),
        new WebPImageFormatProviderExtension(),
        new APngAnimatedImageFormatProviderExtension(),
        new GifAnimatedImageFormatProviderExtension(),
        new WebPAnimatedImageFormatProviderExtension()
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

    [UnmanagedCallersOnly(EntryPoint = nameof(DllGetExtensionsHost))]
    private static unsafe int DllGetExtensionsHost(void** ppv) => DllGetExtensionsHost(ppv, Current);
}
