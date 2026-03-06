namespace ExcelTerminalViewer.Features.CellSearch;

using ExcelTerminalViewer.Domain;

public static class SearchResultFormatter
{
    private const int MaxContentLength = 100;
    private const int TruncationLength = 97;
    private const string Ellipsis = "...";

    public static string Format(SearchResult result, SpreadsheetData data)
    {
        // Convert 0-based indices to 1-based for output
        int oneBasedRow = result.Row + 1;
        int oneBasedColumn = result.Column + 1;

        // Get cell content
        string cellContent = data.GetCell(result.Row, result.Column);

        // Abbreviate content if necessary
        string abbreviatedContent = AbbreviateContent(cellContent);

        // Format as "(row, col): content"
        return $"({oneBasedRow}, {oneBasedColumn}): {abbreviatedContent}";
    }

    internal static string AbbreviateContent(string content)
    {
        // Replace newline characters with spaces
        string normalized = content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

        // Limit content to 100 characters maximum
        if (normalized.Length <= MaxContentLength)
        {
            return normalized;
        }

        // Truncate to 97 characters and append "..."
        return normalized.Substring(0, TruncationLength) + Ellipsis;
    }
}
