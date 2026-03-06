using AwesomeAssertions;
using ExcelTerminalViewer.Features.CellSearch;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.CellSearch;

[TestFixture]
public class SearchStatusFormatterTests
{
    [Test]
    public void Format_WhenSearching_ReturnsSearchingText()
    {
        var state = new SearchDisplayState(SearchStatus.Searching, -1, 0, "test");

        SearchStatusFormatter.Format(state).Should().Be("Searching...");
    }

    [Test]
    public void Format_WhenCompleteWithZeroResults_ReturnsNoMatches()
    {
        var state = new SearchDisplayState(SearchStatus.Complete, -1, 0, "test");

        SearchStatusFormatter.Format(state).Should().Be("No matches");
    }

    [Test]
    public void Format_WhenCompleteWithResultsAndNoNavigation_ReturnsTotalMatches()
    {
        var state = new SearchDisplayState(SearchStatus.Complete, -1, 5, "test");

        SearchStatusFormatter.Format(state).Should().Be("5 matches");
    }

    [Test]
    public void Format_WhenCompleteWithNavigationAtFirstResult_ReturnsPositionAndTotal()
    {
        var state = new SearchDisplayState(SearchStatus.Complete, 0, 3, "test");

        SearchStatusFormatter.Format(state).Should().Be("1/3 matches");
    }

    [Test]
    public void Format_WhenCompleteWithNavigationAtMiddleResult_ReturnsPositionAndTotal()
    {
        var state = new SearchDisplayState(SearchStatus.Complete, 1, 3, "test");

        SearchStatusFormatter.Format(state).Should().Be("2/3 matches");
    }

    [Test]
    public void Format_WhenCompleteWithNavigationAtLastResult_ReturnsPositionAndTotal()
    {
        var state = new SearchDisplayState(SearchStatus.Complete, 2, 3, "test");

        SearchStatusFormatter.Format(state).Should().Be("3/3 matches");
    }

    [Test]
    public void Format_WhenIdle_ReturnsEmptyString()
    {
        var state = new SearchDisplayState(SearchStatus.Idle, -1, 0, "");

        SearchStatusFormatter.Format(state).Should().BeEmpty();
    }

    [Test]
    public void Format_WhenCancelled_ReturnsEmptyString()
    {
        var state = new SearchDisplayState(SearchStatus.Cancelled, -1, 0, "test");

        SearchStatusFormatter.Format(state).Should().BeEmpty();
    }

    [Test]
    public void Format_WhenCompleteWithSingleResult_ReturnsSingularCount()
    {
        var state = new SearchDisplayState(SearchStatus.Complete, -1, 1, "test");

        SearchStatusFormatter.Format(state).Should().Be("1 matches");
    }
}
