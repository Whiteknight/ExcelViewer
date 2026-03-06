using AwesomeAssertions;
using ExcelTerminalViewer.Domain;
using ExcelTerminalViewer.Features.CellSearch;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.CellSearch;

[TestFixture]
public class SearchCoordinatorTests
{
    private static SpreadsheetData CreateData(string[][] rows, string[]? headers = null)
    {
        var cols = rows.Length > 0 ? rows[0].Length : 0;
        headers ??= Enumerable.Range(0, cols).Select(i => $"Col{i}").ToArray();
        return new SpreadsheetData(headers, rows);
    }

    private static SpreadsheetData EmptyData() =>
        new(Array.Empty<string>(), Array.Empty<string[]>());

    [Test]
    public async Task StartSearchAsync_EmptyQuery_ClearsResultsAndReturnsToIdle()
    {
        var states = new List<SearchDisplayState>();
        using var sut = new SearchCoordinator(EmptyData(), states.Add);

        await sut.StartSearchAsync("   ");

        sut.Results.Should().BeEmpty();
        sut.IsSearching.Should().BeFalse();
        sut.CurrentQuery.Should().BeEmpty();
        states.Should().ContainSingle()
            .Which.Status.Should().Be(SearchStatus.Idle);
    }

    [Test]
    public async Task StartSearchAsync_WithMatches_ReturnsCompleteState()
    {
        var data = CreateData([["hello", "world"], ["foo", "hello again"]]);
        var states = new List<SearchDisplayState>();
        using var sut = new SearchCoordinator(data, states.Add);

        await sut.StartSearchAsync("hello");

        sut.IsSearching.Should().BeFalse();
        sut.Results.Should().HaveCount(2);
        sut.CurrentQuery.Should().Be("hello");

        var lastState = states[^1];
        lastState.Status.Should().Be(SearchStatus.Complete);
        lastState.TotalResults.Should().Be(2);
    }

    [Test]
    public async Task StartSearchAsync_NoMatches_ReturnsCompleteWithZeroResults()
    {
        var data = CreateData([["hello", "world"]]);
        var states = new List<SearchDisplayState>();
        using var sut = new SearchCoordinator(data, states.Add);

        await sut.StartSearchAsync("zzz");

        sut.Results.Should().BeEmpty();
        var lastState = states[^1];
        lastState.Status.Should().Be(SearchStatus.Complete);
        lastState.TotalResults.Should().Be(0);
    }

    [Test]
    public async Task StartSearchAsync_NewSearch_ClearsPreviousResults()
    {
        var data = CreateData([["aaa", "bbb"], ["ccc", "aaa"]]);
        var states = new List<SearchDisplayState>();
        using var sut = new SearchCoordinator(data, states.Add);

        await sut.StartSearchAsync("aaa");
        sut.Results.Should().HaveCount(2);

        await sut.StartSearchAsync("bbb");
        sut.Results.Should().HaveCount(1);
        sut.Results[0].Should().Be(new SearchResult(0, 1));
    }

    [Test]
    public async Task NavigateNext_NoResults_ReturnsNull()
    {
        using var sut = new SearchCoordinator(EmptyData(), _ => { });

        await sut.StartSearchAsync("anything");

        sut.NavigateNext().Should().BeNull();
    }

    [Test]
    public async Task NavigatePrevious_NoResults_ReturnsNull()
    {
        using var sut = new SearchCoordinator(EmptyData(), _ => { });

        await sut.StartSearchAsync("anything");

        sut.NavigatePrevious().Should().BeNull();
    }

    [Test]
    public async Task NavigateNext_WrapsFromLastToFirst()
    {
        var data = CreateData([["a", "b"], ["c", "a"]]);
        using var sut = new SearchCoordinator(data, _ => { });

        await sut.StartSearchAsync("a");
        // Results: (0,0) and (1,1)
        sut.Results.Should().HaveCount(2);

        var first = sut.NavigateNext();
        first.Should().Be(new SearchResult(0, 0));

        var second = sut.NavigateNext();
        second.Should().Be(new SearchResult(1, 1));

        var wrapped = sut.NavigateNext();
        wrapped.Should().Be(new SearchResult(0, 0));
    }

    [Test]
    public async Task NavigatePrevious_WrapsFromFirstToLast()
    {
        var data = CreateData([["a", "b"], ["c", "a"]]);
        using var sut = new SearchCoordinator(data, _ => { });

        await sut.StartSearchAsync("a");
        sut.Results.Should().HaveCount(2);

        // First NavigatePrevious from index -1: (-1 - 1 + 2) % 2 = 0... wait,
        // let me trace: _currentIndex starts at -1.
        // NavigatePrevious: (-1 - 1 + 2) % 2 = 0. Returns results[0].
        var first = sut.NavigatePrevious();
        first.Should().Be(new SearchResult(0, 0));

        // Now index is 0. NavigatePrevious: (0 - 1 + 2) % 2 = 1. Returns results[1].
        var second = sut.NavigatePrevious();
        second.Should().Be(new SearchResult(1, 1));

        // Now index is 1. NavigatePrevious: (1 - 1 + 2) % 2 = 0. Returns results[0].
        var wrapped = sut.NavigatePrevious();
        wrapped.Should().Be(new SearchResult(0, 0));
    }

    [Test]
    public void CancelSearch_SetsStateToCancelled()
    {
        var states = new List<SearchDisplayState>();
        using var sut = new SearchCoordinator(EmptyData(), states.Add);

        sut.CancelSearch();

        states.Should().ContainSingle()
            .Which.Status.Should().Be(SearchStatus.Cancelled);
        sut.IsSearching.Should().BeFalse();
    }

    [Test]
    public async Task CancelSearch_ClearsExistingResults()
    {
        var data = CreateData([["match", "other"]]);
        using var sut = new SearchCoordinator(data, _ => { });

        await sut.StartSearchAsync("match");
        sut.Results.Should().HaveCount(1);

        sut.CancelSearch();

        sut.Results.Should().BeEmpty();
        sut.CurrentQuery.Should().BeEmpty();
    }

    [Test]
    public async Task NavigateNext_NotifiesStateChange()
    {
        var data = CreateData([["x"]]);
        var states = new List<SearchDisplayState>();
        using var sut = new SearchCoordinator(data, states.Add);

        await sut.StartSearchAsync("x");
        states.Clear();

        sut.NavigateNext();

        states.Should().ContainSingle();
        states[0].CurrentIndex.Should().Be(0);
        states[0].Status.Should().Be(SearchStatus.Complete);
    }

    [Test]
    public async Task NavigatePrevious_NotifiesStateChange()
    {
        var data = CreateData([["x"]]);
        var states = new List<SearchDisplayState>();
        using var sut = new SearchCoordinator(data, states.Add);

        await sut.StartSearchAsync("x");
        states.Clear();

        sut.NavigatePrevious();

        states.Should().ContainSingle();
        states[0].Status.Should().Be(SearchStatus.Complete);
    }

    [Test]
    public async Task StartSearchAsync_EmptyQueryAfterPreviousSearch_ClearsAndGoesIdle()
    {
        var data = CreateData([["hello"]]);
        var states = new List<SearchDisplayState>();
        using var sut = new SearchCoordinator(data, states.Add);

        await sut.StartSearchAsync("hello");
        sut.Results.Should().HaveCount(1);

        states.Clear();
        await sut.StartSearchAsync("");

        sut.Results.Should().BeEmpty();
        sut.CurrentQuery.Should().BeEmpty();
        states.Should().ContainSingle()
            .Which.Status.Should().Be(SearchStatus.Idle);
    }
}
