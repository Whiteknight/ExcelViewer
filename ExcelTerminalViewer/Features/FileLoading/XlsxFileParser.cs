using ClosedXML.Excel;
using ExcelTerminalViewer.Domain;

namespace ExcelTerminalViewer.Features.FileLoading;

public sealed class XlsxFileParser : IFileParser
{
    public Result<SpreadsheetData, FileLoadError> Parse(string filePath)
    {
        return Result.Try(() => ParseInternal(filePath))
            .MapError(e => new FileLoadError(e.Message));
    }

    private static SpreadsheetData ParseInternal(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.First();

        var rangeUsed = worksheet.RangeUsed();
        if (rangeUsed is null)
            return new SpreadsheetData([], []);

        var firstRow = rangeUsed.FirstRow();
        var headers = ReadHeaders(firstRow);
        var rows = ReadDataRows(rangeUsed, headers.Count);

        return new SpreadsheetData(headers, rows);
    }

    private static List<string> ReadHeaders(IXLRangeRow headerRow)
    {
        return headerRow.CellsUsed()
            .Select(cell => CellNormalizer.Normalize(cell.GetFormattedString()))
            .ToList();
    }

    private static List<IReadOnlyList<string>> ReadDataRows(IXLRange range, int columnCount)
    {
        var rows = new List<IReadOnlyList<string>>();
        var rowCount = range.RowCount();

        for (var r = 2; r <= rowCount; r++)
            rows.Add(ReadSingleRow(range.Row(r), columnCount));

        return rows;
    }

    private static List<string> ReadSingleRow(IXLRangeRow row, int columnCount)
    {
        var cells = new List<string>(columnCount);

        for (var c = 1; c <= columnCount; c++)
        {
            var cell = row.Cell(c);
            cells.Add(CellNormalizer.Normalize(cell.GetFormattedString()));
        }

        return cells;
    }
}
