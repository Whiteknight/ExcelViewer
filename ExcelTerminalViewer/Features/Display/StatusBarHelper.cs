namespace ExcelTerminalViewer.Features.Display;

public static class StatusBarHelper
{
    public static string FormatPosition(int row, int col, int totalRows, int totalCols, string fileName)
        => $"{fileName} | Row {row + 1}/{totalRows} | Col {col + 1}/{totalCols}";
}
