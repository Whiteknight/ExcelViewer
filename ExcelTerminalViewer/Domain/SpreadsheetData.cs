using static ExcelTerminalViewer.Assert;

namespace ExcelTerminalViewer.Domain;

public sealed class SpreadsheetData
{
    public IReadOnlyList<string> Headers { get; }

    public IReadOnlyList<IReadOnlyList<string>> Rows { get; }

    public int RowCount => Rows.Count;

    public int ColumnCount => Headers.Count;

    public SpreadsheetData(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        Headers = NotNull(headers);
        Rows = NotNull(rows);
    }

    public string GetCell(int row, int column)
    {
        if (row < 0 || row >= RowCount)
            return string.Empty;
        if (column < 0 || column >= ColumnCount)
            return string.Empty;

        var rowData = Rows[row];
        if (column >= rowData.Count)
            return string.Empty;

        return rowData[column];
    }
}
