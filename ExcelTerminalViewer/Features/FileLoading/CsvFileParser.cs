using System.Globalization;
using CsvHelper;
using ExcelTerminalViewer.Domain;

namespace ExcelTerminalViewer.Features.FileLoading;

public sealed class CsvFileParser : IFileParser
{
    public Result<SpreadsheetData, FileLoadError> Parse(string filePath)
        => Result.Try(() => ParseInternal(filePath))
            .MapError(e => new FileLoadError(e.Message));

    private static SpreadsheetData ParseInternal(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        var headers = ReadHeaders(csv);
        var rows = ReadAllRows(csv, headers.Count);

        return new SpreadsheetData(headers, rows);
    }

    private static List<string> ReadHeaders(CsvReader csv)
        => (csv.HeaderRecord ?? [])
            .Select(CellNormalizer.Normalize)
            .ToList();

    private static List<IReadOnlyList<string>> ReadAllRows(CsvReader csv, int columnCount)
    {
        var rows = new List<IReadOnlyList<string>>();

        while (csv.Read())
            rows.Add(ReadSingleRow(csv, columnCount));

        return rows;
    }

    private static List<string> ReadSingleRow(CsvReader csv, int columnCount)
    {
        var row = new List<string>(columnCount);

        for (var i = 0; i < columnCount; i++)
        {
            var raw = csv.GetField(i);
            row.Add(CellNormalizer.Normalize(raw));
        }

        return row;
    }
}
