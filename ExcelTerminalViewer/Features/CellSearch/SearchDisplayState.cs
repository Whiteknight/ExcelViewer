namespace ExcelTerminalViewer.Features.CellSearch;

public enum SearchStatus
{
    Idle,
    Searching,
    Complete,
    Cancelled
}

public readonly record struct SearchDisplayState(
    SearchStatus Status,
    int CurrentIndex,
    int TotalResults,
    string Query);
