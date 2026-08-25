using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Pixeval.Extensions.Formats.Pdf.FormatProviders;

internal sealed class PixivNovelPdfWriter
{
    private const float MarginHorizontal = 90;
    private const float MarginVertical = 72;
    private const float FontSize = 11;
    private const float ChapterFontSize = 20;
    private const float ImageMaxHeight = 360;
    private const float ImageVerticalPadding = 10;

    private Action<TextDescriptor>? _lastDelegate;

    private Dictionary<(long Id, int Page), Stream> IllustrationImages { get; } = [];

    private Dictionary<long, Stream> UploadedImages { get; } = [];

    private Stream? CoverImage { get; set; }

    static PixivNovelPdfWriter() => QuestPdfNativeDependencyResolver.Configure();

    public PixivNovelPdfWriter(IReadOnlyDictionary<string, Stream> images) => InitImages(images);

    public Document CreateDocument(string novelInput)
    {
        return Document.Create(document =>
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(MarginHorizontal);
                page.MarginVertical(MarginVertical);
                page.DefaultTextStyle(style => style.FontSize(FontSize).LineHeight(2));
                page.Content().Column(column =>
                {
                    if (CoverImage is { } coverImage)
                    {
                        AddCoverImage(column, coverImage);
                        column.Item().PageBreak();
                    }

                    Compose(column, novelInput);
                });
            }));
    }

    private void Compose(ColumnDescriptor column, string novelInput)
    {
        _ = column.Item().Section("0");

        var pageIndex = 0;
        var index = 0;
        var paragraph = new StringBuilder();
        while (index < novelInput.Length)
        {
            if (TryReadToken(column, novelInput, ref index, ref pageIndex, paragraph))
                continue;

            var ch = novelInput[index++];
            switch (ch)
            {
                case '\r':
                    continue;
                case '\n':
                    LineBreak(column, paragraph);
                    continue;
                default:
                    _ = paragraph.Append(ch);
                    break;
            }
        }

        LineBreak(column, paragraph);
    }

    private bool TryReadToken(
        ColumnDescriptor column,
        string text,
        ref int index,
        ref int pageIndex,
        StringBuilder paragraph)
    {
        if (text.AsSpan(index).StartsWith("[newpage]"))
        {
            LineBreak(column, paragraph);
            column.Item().PageBreak();
            ++pageIndex;
            _ = column.Item().Section(pageIndex.ToString());
            index += "[newpage]".Length;
            return true;
        }

        if (TryReadSingleToken(column, text, ref index, paragraph, "[chapter:", AddChapter))
            return true;

        if (TryReadSingleToken(column, text, ref index, paragraph, "[uploadedimage:", AddUploadedImage))
            return true;

        if (TryReadSingleToken(column, text, ref index, paragraph, "[pixivimage:", AddPixivImage))
            return true;

        if (TryReadInlineJump(text, ref index, paragraph))
            return true;

        if (TryReadRuby(text, ref index, paragraph))
            return true;

        if (TryReadJumpUri(text, ref index, paragraph))
            return true;

        return false;
    }

    private bool TryReadSingleToken(
        ColumnDescriptor column,
        string text,
        ref int index,
        StringBuilder paragraph,
        string token,
        Action<ColumnDescriptor, string> action)
    {
        if (!text.AsSpan(index).StartsWith(token))
            return false;

        var endIndex = text.IndexOf(']', index + token.Length);
        if (endIndex is -1)
            return false;

        LineBreak(column, paragraph);
        action(column, text[(index + token.Length)..endIndex]);
        index = endIndex + 1;
        return true;
    }

    private bool TryReadRuby(string text, ref int index, StringBuilder paragraph)
    {
        const string token = "[[rb:";
        if (!text.AsSpan(index).StartsWith(token))
            return false;

        var separatorIndex = text.IndexOf('>', index + token.Length);
        var endIndex = text.IndexOf("]]", index + token.Length, StringComparison.Ordinal);
        if (separatorIndex is -1 || endIndex is -1 || separatorIndex > endIndex)
            return false;

        var kanji = text[(index + token.Length)..separatorIndex].Trim();
        var ruby = text[(separatorIndex + 1)..endIndex].Trim();
        AddAction(paragraph, descriptor =>
        {
            _ = descriptor.Span(kanji);
            _ = descriptor.Span($"\uFF08{ruby}\uFF09").FontColor(Colors.Grey.Medium);
        });
        index = endIndex + 2;
        return true;
    }

    private bool TryReadJumpUri(string text, ref int index, StringBuilder paragraph)
    {
        const string token = "[[jumpuri:";
        if (!text.AsSpan(index).StartsWith(token))
            return false;

        var separatorIndex = text.IndexOf('>', index + token.Length);
        var endIndex = text.IndexOf("]]", index + token.Length, StringComparison.Ordinal);
        if (separatorIndex is -1 || endIndex is -1 || separatorIndex > endIndex)
            return false;

        var content = text[(index + token.Length)..separatorIndex].Trim();
        var uri = text[(separatorIndex + 1)..endIndex].Trim();
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)
            || parsedUri.Scheme is not ("http" or "https"))
            return false;

        AddAction(paragraph, descriptor => _ = descriptor.Hyperlink(content, parsedUri.OriginalString).FontColor(Colors.Blue.Medium));
        index = endIndex + 2;
        return true;
    }

    private bool TryReadInlineJump(string text, ref int index, StringBuilder paragraph)
    {
        const string token = "[jump:";
        if (!text.AsSpan(index).StartsWith(token))
            return false;

        var endIndex = text.IndexOf(']', index + token.Length);
        if (endIndex is -1)
            return false;

        var pageText = text[(index + token.Length)..endIndex];
        if (!uint.TryParse(pageText, null, out var page))
            return false;

        AddAction(paragraph, descriptor => _ = descriptor.SectionLink($"P.{page}", (page - 1).ToString()).FontColor(Colors.Blue.Medium));
        index = endIndex + 1;
        return true;
    }

    private void AddAction(StringBuilder paragraph, Action<TextDescriptor> action)
    {
        if (paragraph.Length is not 0)
        {
            var text = paragraph.ToString();
            _lastDelegate += descriptor => _ = descriptor.Span(text);
            _ = paragraph.Clear();
        }

        _lastDelegate += action;
    }

    private void LineBreak(ColumnDescriptor column, StringBuilder paragraph)
    {
        if (paragraph.Length is not 0)
        {
            var text = paragraph.ToString();
            _lastDelegate += descriptor => _ = descriptor.Span(text);
            _ = paragraph.Clear();
        }

        if (_lastDelegate is null)
            return;

        column.Item().Text(_lastDelegate);
        _lastDelegate = null;
    }

    private void AddChapter(ColumnDescriptor column, string chapter)
    {
        _ = column.Item();
        _ = column.Item().Text(chapter.Trim()).FontSize(ChapterFontSize).Bold();
        _ = column.Item();
    }

    private void AddUploadedImage(ColumnDescriptor column, string imageId)
    {
        if (!long.TryParse(imageId.Trim(), null, out var id)
            || !UploadedImages.TryGetValue(id, out var imageStream))
            return;

        AddImage(column, imageStream, id.ToString());
    }

    private void AddPixivImage(ColumnDescriptor column, string image)
    {
        if (!TryParsePixivImageKey(image.Trim(), out var illustrationId, out var page)
            || !IllustrationImages.TryGetValue((illustrationId, page), out var imageStream))
            return;

        AddImage(column, imageStream, $"{illustrationId}-{page}", GenerateIllustrationWebUri(illustrationId));
    }

    private static bool TryParsePixivImageKey(string imageName, out long illustrationId, out int page)
    {
        page = 1;
        var separatorIndex = imageName.IndexOf('-');
        if (separatorIndex is -1)
            return long.TryParse(imageName, null, out illustrationId);

        if (imageName.IndexOf('-', separatorIndex + 1) is not -1
            || !long.TryParse(imageName[..separatorIndex], null, out illustrationId)
            || !int.TryParse(imageName[(separatorIndex + 1)..], null, out page))
        {
            illustrationId = 0;
            page = 0;
            return false;
        }

        return true;
    }

    private static void AddImage(ColumnDescriptor column, Stream imageStream, string imageName, string? hyperlink = null)
    {
        try
        {
            imageStream.Position = 0;
            var imageContainer = column.Item()
                .EnsureSpace(ImageMaxHeight + ImageVerticalPadding * 2)
                .PaddingVertical(ImageVerticalPadding)
                .AlignCenter()
                .MaxHeight(ImageMaxHeight);

            if (hyperlink is not null)
                imageContainer = imageContainer.Hyperlink(hyperlink);

            _ = imageContainer
                .Image(imageStream)
                .FitArea();
        }
        catch
        {
            _ = column.Item().Text($"[{imageName}]");
        }
    }

    private static void AddCoverImage(ColumnDescriptor column, Stream imageStream)
    {
        imageStream.Position = 0;
        using var image = Image.FromStream(imageStream);
        imageStream.Position = 0;
        var imageSize = image.Size;
        _ = column.Item()
            .Height(PageSizes.A4.Height - MarginVertical * 2)
            .AlignCenter()
            .AlignMiddle()
            .Width(imageSize.Width)
            .Height(imageSize.Height)
            .ScaleToFit()
            .Image(imageStream)
            .UseOriginalImage();
    }

    private static string GenerateIllustrationWebUri(long illustrationId) =>
        $"https://www.pixiv.net/artworks/{illustrationId}";

    private void InitImages(IReadOnlyDictionary<string, Stream> images)
    {
        foreach (var (name, stream) in images)
        {
            var token = Path.GetFileNameWithoutExtension(name);
            if (string.Equals(token, "cover", StringComparison.OrdinalIgnoreCase))
            {
                CoverImage = stream;
                continue;
            }

            if (TryParsePixivImageKey(token, out var illustrationId, out var page) && token.Contains('-'))
            {
                IllustrationImages[(illustrationId, page)] = stream;
                continue;
            }

            if (long.TryParse(token, null, out var uploadedImageId))
                UploadedImages[uploadedImageId] = stream;
        }
    }
}
