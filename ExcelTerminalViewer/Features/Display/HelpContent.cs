namespace ExcelTerminalViewer.Features.Display;

public static class HelpContent
{
    public static IReadOnlyList<(string Key, string Description)> Entries { get; } =
    [
        ("q", "Quit"),
        ("Ctrl+C", "Quit"),
        ("Arrow keys", "Navigate cells"),
        ("Ctrl+F", "Open search"),
        ("Enter", "Submit search query"),
        ("Esc", "Close search / clear results"),
        ("F3", "Next search result"),
        ("Shift+F3", "Previous search result"),
        ("h", "Toggle this help"),
    ];

    public static string FormatHelpText()
    {
        var maxKeyLen = Entries.Max(e => e.Key.Length);
        var lines = Entries.Select(e => $"  {e.Key.PadRight(maxKeyLen)}   {e.Description}");
        return string.Join("\n", lines);
    }
}
