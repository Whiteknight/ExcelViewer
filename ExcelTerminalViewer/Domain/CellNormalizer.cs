namespace ExcelTerminalViewer.Domain;

public static class CellNormalizer
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\r\n", " ")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }
}
