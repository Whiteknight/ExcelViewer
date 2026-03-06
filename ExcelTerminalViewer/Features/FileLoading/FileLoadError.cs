namespace ExcelTerminalViewer.Features.FileLoading;

public sealed record FileLoadError(string Message) : Error(Message);
