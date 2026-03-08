using ExcelTerminalViewer.Domain;

namespace ExcelTerminalViewer.Features.FileLoading;

public static class FileLoader
{
    public static Result<SpreadsheetData, FileLoadError> Load(string filePath)
    {
        var parser = ResolveParser(filePath);
        return parser is null
            ? new FileLoadError("Unsupported file extension. Supported: .xlsx, .xls, .csv")
            : parser.Parse(filePath);
    }

    private static IFileParser? ResolveParser(string filePath)
        => Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".xlsx" => new XlsxFileParser(),
            ".xls" => new XlsFileParser(),
            ".csv" => new CsvFileParser(),
            _ => null
        };
}
