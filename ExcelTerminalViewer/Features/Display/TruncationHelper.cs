namespace ExcelTerminalViewer.Features.Display;

public static class TruncationHelper
{
    public static string Truncate(string value, int maxWidth)
    {
        if (value.Length <= maxWidth)
            return value;

        return string.Concat(value.AsSpan(0, maxWidth), "\u2026");
    }
}
