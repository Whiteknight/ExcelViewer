using ExcelTerminalViewer.Domain;

namespace ExcelTerminalViewer.Features.CellSearch;

public static class CellSearchEngine
{
    public static Task<List<SearchResult>> SearchAsync(
        SpreadsheetData data,
        string query,
        CancellationToken cancellationToken)
    {
        return Task.Run(() => Search(data, query, cancellationToken), cancellationToken);
    }

    internal static List<SearchResult> Search(
        SpreadsheetData data,
        string query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || data.RowCount == 0)
            return [];

        var results = new List<SearchResult>();

        for (var row = 0; row < data.RowCount; row++)
        {
            if (cancellationToken.IsCancellationRequested)
                return results;

            for (var col = 0; col < data.ColumnCount; col++)
            {
                var cellValue = data.GetCell(row, col);
                if (cellValue.Contains(query, StringComparison.OrdinalIgnoreCase))
                    results.Add(new SearchResult(row, col));
            }
        }

        return results;
    }
}
