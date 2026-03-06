namespace ExcelTerminalViewer.Features.CellSearch;

public static class SearchStatusFormatter
{
    public static string Format(SearchDisplayState state)
    {
        if (state.Status == SearchStatus.Searching)
            return "Searching...";

        if (state.Status != SearchStatus.Complete)
            return string.Empty;

        if (state.TotalResults == 0)
            return "No matches";

        if (state.CurrentIndex < 0)
            return $"{state.TotalResults} matches";

        return $"{state.CurrentIndex + 1}/{state.TotalResults} matches";
    }
}
