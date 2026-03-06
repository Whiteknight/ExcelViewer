namespace ExcelTerminalViewer.Features.Cli;

public sealed record CliOptions(string FilePath, int MaxWidth = 30, int? GoToRow = null);
