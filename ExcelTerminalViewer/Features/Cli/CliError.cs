namespace ExcelTerminalViewer.Features.Cli;

public sealed record CliError(string Message) : Error(Message);
