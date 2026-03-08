namespace ExcelTerminalViewer.Features.Cli;

// TODO: Subclasses for different modes, so we don't need to jam all possible options into this one class.
public sealed record CliOptions(string FilePath, int MaxWidth = 30, int? GoToRow = null, string? SearchTerm = null);
