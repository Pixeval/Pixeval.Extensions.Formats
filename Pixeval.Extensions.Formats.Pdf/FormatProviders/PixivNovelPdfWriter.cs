using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Pixeval.Extensions.Formats.Pdf.FormatProviders;

internal sealed class PixivNovelPdfWriter(IReadOnlyDictionary<string, Stream> images)
{
    private const double PageWidth = 595;
    private const double PageHeight = 842;
    private const double MarginLeft = 72;
    private const double MarginRight = 72;
    private const double MarginTop = 72;
    private const double MarginBottom = 72;
    private const double FontSize = 11;
    private const double ChapterFontSize = 17;
    private const double LineHeight = 18;
    private const double ParagraphSpacing = 8;
    private const double ImageSpacing = 10;
    private const double SeparatorWidth = 0.8;

    private readonly PdfDocumentBuilder _builder = new();
    private PdfDocumentBuilder.AddedFont _bodyFont = null!;
    private PdfDocumentBuilder.AddedFont _boldFont = null!;
    private PdfPageBuilder _page = null!;
    private double _y;

    private static string[] FontCandidates { get; } =
    [
        @"C:\Windows\Fonts\NotoSansSC-VF.ttf",
        @"C:\Windows\Fonts\NotoSerifSC-VF.ttf",
        @"C:\Windows\Fonts\NotoSansJP-VF.ttf",
        @"C:\Windows\Fonts\NotoSerifJP-VF.ttf",
        @"C:\Windows\Fonts\msyh.ttc",
        @"C:\Windows\Fonts\simhei.ttf",
        @"C:\Windows\Fonts\simsun.ttc"
    ];

    private static string[] ImageExtensions { get; } =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".bmp",
        ".webp"
    ];

    private double ContentWidth => PageWidth - MarginLeft - MarginRight;

    public void Write(string novelInput, string destinationPath)
    {
        InitFonts();
        NewPage();

        var index = 0;
        var paragraph = new StringBuilder();
        while (index < novelInput.Length)
        {
            if (TryReadToken(novelInput, ref index, paragraph))
                continue;

            var ch = novelInput[index++];
            if (ch is '\r')
                continue;

            if (ch is '\n')
            {
                FlushParagraph(paragraph);
                continue;
            }

            _ = paragraph.Append(ch);
        }

        FlushParagraph(paragraph);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        File.WriteAllBytes(destinationPath, _builder.Build());
    }

    private void InitFonts()
    {
        foreach (var path in FontCandidates)
        {
            if (!File.Exists(path))
                continue;

            var bytes = File.ReadAllBytes(path);
            if (!_builder.CanUseTrueTypeFont(bytes, out _))
                continue;

            _bodyFont = _builder.AddTrueTypeFont(bytes);
            _boldFont = _bodyFont;
            return;
        }

        _bodyFont = _builder.AddStandard14Font(Standard14Font.Helvetica);
        _boldFont = _builder.AddStandard14Font(Standard14Font.HelveticaBold);
    }

    private void NewPage()
    {
        _page = _builder.AddPage(PageSize.A4);
        _y = PageHeight - MarginTop;
    }

    private bool TryReadToken(string text, ref int index, StringBuilder paragraph)
    {
        if (text.AsSpan(index).StartsWith("[newpage]"))
        {
            FlushParagraph(paragraph);
            NewPage();
            index += "[newpage]".Length;
            return true;
        }

        if (TryReadSingleToken(text, ref index, paragraph, "[chapter:", AddChapter))
            return true;

        if (TryReadSingleToken(text, ref index, paragraph, "[uploadedimage:", AddUploadedImage))
            return true;

        if (TryReadSingleToken(text, ref index, paragraph, "[pixivimage:", AddPixivImage))
            return true;

        if (TryReadSingleToken(text, ref index, paragraph, "[jump:", page => _ = paragraph.Append($"P.{page}")))
            return true;

        if (TryReadRuby(text, ref index, paragraph))
            return true;

        if (TryReadJumpUri(text, ref index, paragraph))
            return true;

        return false;
    }

    private bool TryReadSingleToken(string text, ref int index, StringBuilder paragraph, string token, Action<string> action)
    {
        if (!text.AsSpan(index).StartsWith(token))
            return false;

        var endIndex = text.IndexOf(']', index + token.Length);
        if (endIndex is -1)
            return false;

        FlushParagraph(paragraph);
        action(text[(index + token.Length)..endIndex]);
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
        _ = paragraph.Append(kanji).Append('(').Append(ruby).Append(')');
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
        _ = paragraph.Append(content).Append(" (").Append(uri).Append(')');
        index = endIndex + 2;
        return true;
    }

    private void FlushParagraph(StringBuilder paragraph)
    {
        if (paragraph.Length is 0)
            return;

        var lines = Wrap(paragraph.ToString(), FontSize, _bodyFont);
        foreach (var line in lines)
            AddLine(line, FontSize, _bodyFont);
        _y -= ParagraphSpacing;
        _ = paragraph.Clear();
    }

    private void AddChapter(string chapter)
    {
        EnsureSpace(ChapterFontSize + ParagraphSpacing * 2);
        _y -= ParagraphSpacing;
        var lines = Wrap(chapter.Trim(), ChapterFontSize, _boldFont);
        foreach (var line in lines)
            AddLine(line, ChapterFontSize, _boldFont);
        _y -= ParagraphSpacing;
    }

    private void AddUploadedImage(string imageId) => AddImage(imageId.Trim());

    private void AddPixivImage(string image)
    {
        var name = image.Trim();
        if (!name.Contains('-'))
            name += "-1";
        AddImage(name);
    }

    private void AddImage(string imageName)
    {
        if (!TryGetImage(imageName, out var imageStream))
            return;

        try
        {
            imageStream.Position = 0;
            var imageKind = DetectImageKind(imageStream);
            imageStream.Position = 0;
            var placeholder = new PdfRectangle(MarginLeft, _y - 10, MarginLeft + 10, _y);
            var addedImage = imageKind switch
            {
                ImageKind.Jpeg => _page.AddJpeg(imageStream, placeholder),
                ImageKind.Png => _page.AddPng(imageStream, placeholder),
                _ => throw new NotSupportedException()
            };

            var width = addedImage.Width;
            var height = addedImage.Height;
            var scale = Math.Min(ContentWidth / width, 360 / height);
            var drawWidth = width * scale;
            var drawHeight = height * scale;

            EnsureSpace(drawHeight + ImageSpacing);
            var left = MarginLeft + (ContentWidth - drawWidth) / 2;
            var bottom = _y - drawHeight;
            _page.AddImage(addedImage, new PdfRectangle(left, bottom, left + drawWidth, bottom + drawHeight));
            _y -= drawHeight + ImageSpacing;
        }
        catch
        {
            AddLine($"[{imageName}]", FontSize, _bodyFont);
        }
    }

    private static ImageKind DetectImageKind(Stream stream)
    {
        Span<byte> header = stackalloc byte[8];
        var count = stream.Read(header);
        if (count >= 3 && header[0] is 0xFF && header[1] is 0xD8 && header[2] is 0xFF)
            return ImageKind.Jpeg;

        ReadOnlySpan<byte> pngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        if (count >= pngHeader.Length && header[..pngHeader.Length].SequenceEqual(pngHeader))
            return ImageKind.Png;

        return ImageKind.Unknown;
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

    private void AddLine(string line, double fontSize, PdfDocumentBuilder.AddedFont font)
    {
        EnsureSpace(LineHeight);
        _page.AddText(line, fontSize, new PdfPoint(MarginLeft, _y), font);
        _y -= LineHeight;
    }

    private void EnsureSpace(double requiredHeight)
    {
        if (_y - requiredHeight < MarginBottom)
            NewPage();
    }

    private IReadOnlyList<string> Wrap(string text, double fontSize, PdfDocumentBuilder.AddedFont font)
    {
        var result = new List<string>();
        foreach (var rawLine in text.ReplaceLineEndings("\n").Split('\n'))
        {
            var line = new StringBuilder();
            foreach (var rune in rawLine.EnumerateRunes())
            {
                var next = line.ToString() + rune;
                if (line.Length is not 0 && Measure(next, fontSize, font) > ContentWidth)
                {
                    result.Add(line.ToString());
                    _ = line.Clear();
                }

                _ = line.Append(rune);
            }

            if (line.Length is not 0)
                result.Add(line.ToString());
            else if (rawLine.Length is 0)
                result.Add("");
        }

        return result;
    }

    private double Measure(string text, double fontSize, PdfDocumentBuilder.AddedFont font)
    {
        var letters = _page.MeasureText(text, fontSize, PdfPoint.Origin, font);
        if (letters.Count is 0)
            return 0;

        return letters.Max(static letter => letter.BoundingBox.Right)
               - letters.Min(static letter => letter.BoundingBox.Left);
    }

    private enum ImageKind
    {
        Unknown,
        Jpeg,
        Png
    }
}
