using ExcelDataReader;
using ExcelTerminalViewer.Domain;
using System.Text;

namespace ExcelTerminalViewer.Features.FileLoading;

public sealed class XlsFileParser : IFileParser
{
    public Result<SpreadsheetData, FileLoadError> Parse(string filePath)
        => Result.Try(() => ParseInternal(filePath))
            .MapError(e => new FileLoadError(e.Message));

    private static SpreadsheetData ParseInternal(string filePath)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var stream = File.OpenRead(filePath);
        using var reader = ExcelReaderFactory.CreateBinaryReader(stream);

        if (!reader.Read())
            return new SpreadsheetData([], []);

        var headers = ReadHeaders(reader);
        var rows = ReadDataRows(reader, headers.Count);

        return new SpreadsheetData(headers, rows);
    }

    private static List<string> ReadHeaders(IExcelDataReader reader)
    {
        var headers = new List<string>(reader.FieldCount);

        for (var i = 0; i < reader.FieldCount; i++)
            headers.Add(CellNormalizer.Normalize(Convert.ToString(reader.GetValue(i))));

        return headers;
    }

    private static List<IReadOnlyList<string>> ReadDataRows(IExcelDataReader reader, int columnCount)
    {
        var rows = new List<IReadOnlyList<string>>();

        while (reader.Read())
            rows.Add(ReadSingleRow(reader, columnCount));

        return rows;
    }

    private static List<string> ReadSingleRow(IExcelDataReader reader, int columnCount)
    {
        var cells = new List<string>(columnCount);

        for (var i = 0; i < columnCount; i++)
            cells.Add(CellNormalizer.Normalize(Convert.ToString(reader.GetValue(i))));

        return cells;
    }
}
