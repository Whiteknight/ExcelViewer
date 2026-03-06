using ExcelTerminalViewer.Domain;

namespace ExcelTerminalViewer.Features.FileLoading;

public interface IFileParser
{
    Result<SpreadsheetData, FileLoadError> Parse(string filePath);
}
