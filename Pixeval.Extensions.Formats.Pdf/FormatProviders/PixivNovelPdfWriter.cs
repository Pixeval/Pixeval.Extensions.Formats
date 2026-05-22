using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Pixeval.Extensions.Formats.Pdf.FormatProviders;

internal sealed class PixivNovelPdfWriter(IReadOnlyDictionary<string, Stream> images)
{
    private const float MarginHorizontal = 90;
    private const float MarginVertical = 72;
    private const float FontSize = 11;
    private const float ChapterFontSize = 20;
    private const float ImageMaxHeight = 360;
    private const float ImageVerticalPadding = 10;

    private static string[] ImageExtensions { get; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".bmp",
        ".webp"
    ];

    private Action<TextDescriptor>? _lastDelegate;

    static PixivNovelPdfWriter() => QuestPDF.Settings.License = LicenseType.Community;

    public void Write(string novelInput, string destinationPath)
    {
        var document = Document.Create(document =>
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(MarginHorizontal);
                page.MarginVertical(MarginVertical);
                page.DefaultTextStyle(style => style.FontSize(FontSize).LineHeight(2));
                page.Content().Column(column => Compose(column, novelInput));
            }));

        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        document.GeneratePdf(destinationPath);
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

    private void AddUploadedImage(ColumnDescriptor column, string imageId) => AddImage(column, imageId.Trim());

    private void AddPixivImage(ColumnDescriptor column, string image)
    {
        var name = image.Trim();
        if (!name.Contains('-'))
            name += "-1";

        AddImage(column, name);
    }

    private void AddImage(ColumnDescriptor column, string imageName)
    {
        if (!TryGetImage(imageName, out var imageStream))
            return;

        try
        {
            imageStream.Position = 0;
            _ = column.Item()
                .EnsureSpace(ImageMaxHeight + ImageVerticalPadding * 2)
                .PaddingVertical(ImageVerticalPadding)
                .AlignCenter()
                .MaxHeight(ImageMaxHeight)
                .Image(imageStream)
                .FitArea();
        }
        catch
        {
            _ = column.Item().Text($"[{imageName}]");
        }
    }

    private bool TryGetImage(string imageName, out Stream imageStream)
    {
        if (images.TryGetValue(imageName, out imageStream!))
            return true;

        foreach (var imageExtension in ImageExtensions)
        {
            if (images.TryGetValue(imageName + imageExtension, out imageStream!))
                return true;
        }

        foreach (var (name, stream) in images)
        {
            if (Path.GetFileNameWithoutExtension(name) != imageName)
                continue;

            imageStream = stream;
            return true;
        }

        imageStream = null!;
        return false;
    }
}
