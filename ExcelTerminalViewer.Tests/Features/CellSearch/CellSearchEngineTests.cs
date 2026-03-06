using AwesomeAssertions;
using ExcelTerminalViewer.Domain;
using ExcelTerminalViewer.Features.CellSearch;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.CellSearch;

[TestFixture]
public class CellSearchEngineTests
{
    [Test]
    public void Search_EmptyQuery_ReturnsEmptyList()
    {
        var data = CreateData(["H1"], [["value"]]);

        var results = CellSearchEngine.Search(data, "", CancellationToken.None);

        results.Should().BeEmpty();
    }

    [Test]
    public void Search_WhitespaceQuery_ReturnsEmptyList()
    {
        var data = CreateData(["H1"], [["value"]]);

        var results = CellSearchEngine.Search(data, "   ", CancellationToken.None);

        results.Should().BeEmpty();
    }

    [Test]
    public void Search_EmptySpreadsheet_ReturnsEmptyList()
    {
        var data = new SpreadsheetData(Array.Empty<string>(), Array.Empty<string[]>());

        var results = CellSearchEngine.Search(data, "test", CancellationToken.None);

        results.Should().BeEmpty();
    }

    [Test]
    public void Search_SingleMatch_ReturnsCorrectPosition()
    {
        var data = CreateData(["H1", "H2"], [["apple", "banana"], ["cherry", "date"]]);

        var results = CellSearchEngine.Search(data, "banana", CancellationToken.None);

        results.Should().ContainSingle()
            .Which.Should().Be(new SearchResult(0, 1));
    }

    [Test]
    public void Search_MultipleMatches_ReturnsInRowMajorOrder()
    {
        var data = CreateData(
            ["H1", "H2", "H3"],
            [
                ["cat", "dog", "cat"],
                ["bird", "cat", "fish"]
            ]);

        var results = CellSearchEngine.Search(data, "cat", CancellationToken.None);

        results.Should().HaveCount(3);
        results[0].Should().Be(new SearchResult(0, 0));
        results[1].Should().Be(new SearchResult(0, 2));
        results[2].Should().Be(new SearchResult(1, 1));
    }

    [Test]
    public void Search_CaseInsensitive_MatchesRegardlessOfCase()
    {
        var data = CreateData(["H1"], [["ABC"], ["def"], ["GhI"]]);

        var results = CellSearchEngine.Search(data, "abc", CancellationToken.None);

        results.Should().ContainSingle()
            .Which.Should().Be(new SearchResult(0, 0));
    }

    [Test]
    public void Search_CaseInsensitive_UppercaseQueryMatchesLowercase()
    {
        var data = CreateData(["H1"], [["hello world"]]);

        var results = CellSearchEngine.Search(data, "HELLO", CancellationToken.None);

        results.Should().ContainSingle()
            .Which.Should().Be(new SearchResult(0, 0));
    }

    [Test]
    public void Search_NoMatches_ReturnsEmptyList()
    {
        var data = CreateData(["H1", "H2"], [["apple", "banana"], ["cherry", "date"]]);

        var results = CellSearchEngine.Search(data, "grape", CancellationToken.None);

        results.Should().BeEmpty();
    }

    [Test]
    public void Search_PartialStringMatch_FindsContainedSubstring()
    {
        var data = CreateData(["H1"], [["pineapple"]]);

        var results = CellSearchEngine.Search(data, "apple", CancellationToken.None);

        results.Should().ContainSingle()
            .Which.Should().Be(new SearchResult(0, 0));
    }

    [Test]
    public void Search_CancellationRequested_ReturnsPartialOrEmptyList()
    {
        var data = CreateData(
            ["H1"],
            [["match"], ["match"], ["match"], ["match"], ["match"]]);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var results = CellSearchEngine.Search(data, "match", cts.Token);

        // Cancellation is checked at the start of each row, so with pre-cancelled token
        // we should get an empty list (cancelled before processing any row).
        results.Should().BeEmpty();
    }

    [Test]
    public async Task SearchAsync_DelegatesToSearch_ReturnsResults()
    {
        var data = CreateData(["H1"], [["target"]]);

        var results = await CellSearchEngine.SearchAsync(data, "target", CancellationToken.None);

        results.Should().ContainSingle()
            .Which.Should().Be(new SearchResult(0, 0));
    }

    private static SpreadsheetData CreateData(string[] headers, string[][] rows)
    {
        return new SpreadsheetData(headers, rows);
    }
}
