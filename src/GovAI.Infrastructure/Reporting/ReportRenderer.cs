using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using GovAI.Application.Abstractions.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GovAI.Infrastructure.Reporting;

/// <summary>
/// PDF ve Excel çıktı üretimi.
///
/// PDF tarafında QuestPDF kullanılır; HTML render eden bir tarayıcı bağımlılığı (Chromium) taşımamak
/// için Application katmanının ürettiği basit HTML, blok bazlı olarak PDF akışına çevrilir.
/// </summary>
public sealed partial class ReportRenderer : IReportRenderer
{
    static ReportRenderer()
    {
        // QuestPDF Community lisansı: yıllık geliri 1M USD altındaki kuruluşlar için ücretsiz.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> RenderPdfAsync(string title, string htmlBody, CancellationToken cancellationToken = default)
    {
        var blocks = HtmlToBlocks(htmlBody);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontSize(9).FontFamily(Fonts.Calibri));

                page.Header().PaddingBottom(8).Column(column =>
                {
                    column.Item().Text(title).FontSize(15).SemiBold();
                    column.Item().Text($"GOVAI · {DateTimeOffset.UtcNow:dd.MM.yyyy HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                page.Content().Column(column =>
                {
                    column.Spacing(6);

                    foreach (var block in blocks)
                    {
                        switch (block)
                        {
                            case HeadingBlock heading:
                                column.Item().PaddingTop(6).Text(heading.Text).FontSize(11).SemiBold();
                                break;

                            case ParagraphBlock paragraph:
                                column.Item().Text(paragraph.Text);
                                break;

                            case TableBlock table:
                                column.Item().Table(builder => BuildTable(builder, table));
                                break;
                        }
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> RenderExcelAsync(
        string sheetName,
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<object?>> rows,
        CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet(Sanitize(sheetName));

        for (var column = 0; column < headers.Count; column++)
        {
            var cell = sheet.Cell(1, column + 1);
            cell.Value = headers[column];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2F6");
        }

        for (var row = 0; row < rows.Count; row++)
        {
            for (var column = 0; column < rows[row].Count; column++)
            {
                SetCellValue(sheet.Cell(row + 2, column + 1), rows[row][column]);
            }
        }

        sheet.SheetView.FreezeRows(1);
        if (rows.Count > 0)
        {
            sheet.Range(1, 1, rows.Count + 1, headers.Count).SetAutoFilter();
        }

        sheet.Columns().AdjustToContents(minWidth: 10d, maxWidth: 60d);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = string.Empty;
                break;
            case decimal number:
                cell.Value = number;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case double number:
                cell.Value = number;
                break;
            case int number:
                cell.Value = number;
                break;
            case long number:
                cell.Value = number;
                break;
            case bool flag:
                cell.Value = flag ? "Evet" : "Hayır";
                break;
            case DateTime date:
                cell.Value = date;
                cell.Style.DateFormat.Format = "dd.MM.yyyy";
                break;
            case DateTimeOffset date:
                cell.Value = date.DateTime;
                cell.Style.DateFormat.Format = "dd.MM.yyyy";
                break;
            default:
                cell.Value = value.ToString() ?? string.Empty;
                break;
        }
    }

    private static void BuildTable(TableDescriptor builder, TableBlock table)
    {
        var columnCount = Math.Max(1, table.Rows.Max(r => r.Count));

        builder.ColumnsDefinition(columns =>
        {
            for (var i = 0; i < columnCount; i++)
            {
                columns.RelativeColumn();
            }
        });

        var isFirstRow = true;
        foreach (var row in table.Rows)
        {
            foreach (var cellText in row)
            {
                var cell = builder.Cell()
                    .BorderBottom(0.5f)
                    .BorderColor(Colors.Grey.Lighten1)
                    .PaddingVertical(3)
                    .PaddingHorizontal(4);

                if (isFirstRow)
                {
                    cell.Background(Colors.Grey.Lighten3).Text(cellText).SemiBold().FontSize(8.5f);
                }
                else
                {
                    cell.Text(cellText).FontSize(8.5f);
                }
            }

            // Eksik hücreleri doldur ki tablo hizası bozulmasın.
            for (var i = row.Count; i < columnCount; i++)
            {
                builder.Cell().Text(string.Empty);
            }

            isFirstRow = false;
        }
    }

    /// <summary>
    /// Application katmanının ürettiği basit HTML'i (h1/h2, div, table) PDF blokları hâline getirir.
    /// Tam bir HTML motoru değildir; yalnızca bu projenin ürettiği işaretlemeyi anlar.
    /// </summary>
    private static List<IReportBlock> HtmlToBlocks(string html)
    {
        var blocks = new List<IReportBlock>();
        var body = StyleTagRegex().Replace(html, string.Empty);

        foreach (Match match in BlockRegex().Matches(body))
        {
            var tag = match.Groups["tag"].Value.ToLowerInvariant();
            var inner = match.Groups["inner"].Value;

            if (tag is "h1" or "h2" or "h3")
            {
                blocks.Add(new HeadingBlock(Clean(inner)));
            }
            else if (tag == "table")
            {
                var rows = new List<List<string>>();
                foreach (Match rowMatch in TableRowRegex().Matches(inner))
                {
                    var cells = TableCellRegex()
                        .Matches(rowMatch.Groups["row"].Value)
                        .Select(c => Clean(c.Groups["cell"].Value))
                        .ToList();

                    if (cells.Count > 0)
                    {
                        rows.Add(cells);
                    }
                }

                if (rows.Count > 0)
                {
                    blocks.Add(new TableBlock(rows));
                }
            }
            else
            {
                var text = Clean(inner);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    blocks.Add(new ParagraphBlock(text));
                }
            }
        }

        return blocks;
    }

    private static string Clean(string html)
    {
        var withoutTags = TagRegex().Replace(html.Replace("<br/>", " · ").Replace("<br />", " · "), " ");
        var decoded = System.Net.WebUtility.HtmlDecode(withoutTags);
        return WhitespaceRegex().Replace(decoded, " ").Trim();
    }

    private static string Sanitize(string sheetName)
    {
        var cleaned = new string(sheetName.Where(c => !"[]:*?/\\".Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(cleaned)
            ? "Sayfa1"
            : cleaned[..Math.Min(31, cleaned.Length)];
    }

    private interface IReportBlock;

    private sealed record HeadingBlock(string Text) : IReportBlock;

    private sealed record ParagraphBlock(string Text) : IReportBlock;

    private sealed record TableBlock(List<List<string>> Rows) : IReportBlock;

    [GeneratedRegex(@"<style\b[^>]*>.*?</style>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex StyleTagRegex();

    [GeneratedRegex(@"<(?<tag>h1|h2|h3|div|p|table)\b[^>]*>(?<inner>.*?)</\k<tag>>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex BlockRegex();

    [GeneratedRegex(@"<tr\b[^>]*>(?<row>.*?)</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex TableRowRegex();

    [GeneratedRegex(@"<t[hd]\b[^>]*>(?<cell>.*?)</t[hd]>", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex TableCellRegex();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
