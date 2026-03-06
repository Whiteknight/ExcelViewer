using ExcelTerminalViewer.Domain;

namespace ExcelTerminalViewer.Features.CellSearch;

public sealed class SearchCoordinator : IDisposable
{
    private readonly SpreadsheetData _data;
    private readonly Action<SearchDisplayState> _onStateChanged;

    private List<SearchResult> _results = [];
    private int _currentIndex = -1;
    private CancellationTokenSource? _cts;
    private bool _isSearching;
    private string _currentQuery = string.Empty;
    private bool _disposed;

    public bool IsSearching => _isSearching;
    public IReadOnlyList<SearchResult> Results => _results;
    public string CurrentQuery => _currentQuery;

    public SearchCoordinator(SpreadsheetData data, Action<SearchDisplayState> onStateChanged)
    {
        _data = data;
        _onStateChanged = onStateChanged;
    }

    public async Task StartSearchAsync(string query)
    {
        CancelExistingSearch();
        ClearResults();

        if (string.IsNullOrWhiteSpace(query))
        {
            _currentQuery = string.Empty;
            NotifyStateChanged(SearchStatus.Idle);
            return;
        }

        _currentQuery = query;
        _isSearching = true;

        var cts = new CancellationTokenSource();
        _cts = cts;

        NotifyStateChanged(SearchStatus.Searching);

        var results = await CellSearchEngine.SearchAsync(_data, query, cts.Token);

        if (cts.Token.IsCancellationRequested)
            return;

        _results = results;
        _isSearching = false;
        NotifyStateChanged(SearchStatus.Complete);
    }

    public void CancelSearch()
    {
        CancelExistingSearch();
        _isSearching = false;
        _currentQuery = string.Empty;
        ClearResults();
        NotifyStateChanged(SearchStatus.Cancelled);
    }

    public SearchResult? NavigateNext()
    {
        if (_isSearching || _results.Count == 0)
            return null;

        _currentIndex = (_currentIndex + 1) % _results.Count;
        NotifyStateChanged(SearchStatus.Complete);
        return _results[_currentIndex];
    }

    public SearchResult? NavigatePrevious()
    {
        if (_isSearching || _results.Count == 0)
            return null;

        _currentIndex = (_currentIndex - 1 + _results.Count) % _results.Count;
        NotifyStateChanged(SearchStatus.Complete);
        return _results[_currentIndex];
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void CancelExistingSearch()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void ClearResults()
    {
        _results = [];
        _currentIndex = -1;
    }

    private void NotifyStateChanged(SearchStatus status)
    {
        _onStateChanged(new SearchDisplayState(status, _currentIndex, _results.Count, _currentQuery));
    }
}
