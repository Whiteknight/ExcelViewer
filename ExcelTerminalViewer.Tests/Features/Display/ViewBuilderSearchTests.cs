using AwesomeAssertions;
using ExcelTerminalViewer.Features.CellSearch;
using ExcelTerminalViewer.Features.Display;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.Display;

[TestFixture]
public class ViewBuilderRebuildHighlightSetTests
{
    [Test]
    public void RebuildHighlightSet_WhenComplete_PopulatesSetFromResults()
    {
        var set = new HashSet<(int Row, int Column)>();
        var results = new List<SearchResult>
        {
            new(0, 1),
            new(2, 3),
        };

        ViewBuilder.RebuildHighlightSet(set, results, SearchStatus.Complete);

        set.Should().HaveCount(2);
        set.Should().Contain((0, 1));
        set.Should().Contain((2, 3));
    }

    [Test]
    public void RebuildHighlightSet_WhenSearching_ClearsSet()
    {
        var set = new HashSet<(int Row, int Column)> { (0, 0) };
        var results = new List<SearchResult> { new(1, 1) };

        ViewBuilder.RebuildHighlightSet(set, results, SearchStatus.Searching);

        set.Should().BeEmpty();
    }

    [Test]
    public void RebuildHighlightSet_WhenCancelled_ClearsSet()
    {
        var set = new HashSet<(int Row, int Column)> { (0, 0) };
        var results = new List<SearchResult> { new(1, 1) };

        ViewBuilder.RebuildHighlightSet(set, results, SearchStatus.Cancelled);

        set.Should().BeEmpty();
    }

    [Test]
    public void RebuildHighlightSet_WhenIdle_ClearsSet()
    {
        var set = new HashSet<(int Row, int Column)> { (0, 0) };
        var results = new List<SearchResult>();

        ViewBuilder.RebuildHighlightSet(set, results, SearchStatus.Idle);

        set.Should().BeEmpty();
    }

    [Test]
    public void RebuildHighlightSet_WhenComplete_ClearsPreviousEntries()
    {
        var set = new HashSet<(int Row, int Column)> { (99, 99) };
        var results = new List<SearchResult> { new(0, 0) };

        ViewBuilder.RebuildHighlightSet(set, results, SearchStatus.Complete);

        set.Should().HaveCount(1);
        set.Should().Contain((0, 0));
        set.Should().NotContain((99, 99));
    }

    [Test]
    public void RebuildHighlightSet_WhenCompleteWithEmptyResults_ClearsSet()
    {
        var set = new HashSet<(int Row, int Column)> { (0, 0) };
        var results = new List<SearchResult>();

        ViewBuilder.RebuildHighlightSet(set, results, SearchStatus.Complete);

        set.Should().BeEmpty();
    }
}
