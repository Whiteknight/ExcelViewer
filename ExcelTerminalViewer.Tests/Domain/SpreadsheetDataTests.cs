using AwesomeAssertions;
using ExcelTerminalViewer.Domain;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Domain;

[TestFixture]
public class SpreadsheetDataTests
{
    [Test]
    public void Constructor_WithValidData_SetsHeadersAndRows()
    {
        var headers = new[] { "A", "B", "C" };
        var rows = new[] { new[] { "1", "2", "3" } };

        var sut = new SpreadsheetData(headers, rows);

        sut.Headers.Should().BeEquivalentTo(headers);
        sut.Rows.Should().HaveCount(1);
        sut.RowCount.Should().Be(1);
        sut.ColumnCount.Should().Be(3);
    }

    [Test]
    public void Constructor_WithEmptyData_ProducesZeroCounts()
    {
        var sut = new SpreadsheetData(Array.Empty<string>(), Array.Empty<string[]>());

        sut.RowCount.Should().Be(0);
        sut.ColumnCount.Should().Be(0);
        sut.Headers.Should().BeEmpty();
        sut.Rows.Should().BeEmpty();
    }

    [Test]
    public void Constructor_NullHeaders_Throws()
    {
        var act = () => new SpreadsheetData(null!, Array.Empty<string[]>());

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_NullRows_Throws()
    {
        var act = () => new SpreadsheetData(Array.Empty<string>(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetCell_ValidIndices_ReturnsCorrectValue()
    {
        var headers = new[] { "H1", "H2" };
        var rows = new[]
        {
            new[] { "a", "b" },
            new[] { "c", "d" }
        };
        var sut = new SpreadsheetData(headers, rows);

        sut.GetCell(0, 0).Should().Be("a");
        sut.GetCell(0, 1).Should().Be("b");
        sut.GetCell(1, 0).Should().Be("c");
        sut.GetCell(1, 1).Should().Be("d");
    }

    [Test]
    public void GetCell_NegativeRow_ReturnsEmpty()
    {
        var sut = new SpreadsheetData(new[] { "H" }, new[] { new[] { "v" } });

        sut.GetCell(-1, 0).Should().BeEmpty();
    }

    [Test]
    public void GetCell_RowBeyondBounds_ReturnsEmpty()
    {
        var sut = new SpreadsheetData(new[] { "H" }, new[] { new[] { "v" } });

        sut.GetCell(1, 0).Should().BeEmpty();
    }

    [Test]
    public void GetCell_NegativeColumn_ReturnsEmpty()
    {
        var sut = new SpreadsheetData(new[] { "H" }, new[] { new[] { "v" } });

        sut.GetCell(0, -1).Should().BeEmpty();
    }

    [Test]
    public void GetCell_ColumnBeyondHeaderCount_ReturnsEmpty()
    {
        var sut = new SpreadsheetData(new[] { "H" }, new[] { new[] { "v" } });

        sut.GetCell(0, 1).Should().BeEmpty();
    }

    [Test]
    public void GetCell_RaggedRow_ColumnBeyondRowLength_ReturnsEmpty()
    {
        var headers = new[] { "A", "B", "C" };
        var rows = new[]
        {
            new[] { "only-one" }
        };
        var sut = new SpreadsheetData(headers, rows);

        sut.GetCell(0, 0).Should().Be("only-one");
        sut.GetCell(0, 1).Should().BeEmpty();
        sut.GetCell(0, 2).Should().BeEmpty();
    }

    [Test]
    public void ColumnCount_MatchesHeaderLength()
    {
        var headers = new[] { "X", "Y", "Z", "W" };
        var sut = new SpreadsheetData(headers, Array.Empty<string[]>());

        sut.ColumnCount.Should().Be(4);
    }

    [Test]
    public void RowCount_MatchesRowsLength()
    {
        var headers = new[] { "H" };
        var rows = new[] { new[] { "a" }, new[] { "b" }, new[] { "c" } };
        var sut = new SpreadsheetData(headers, rows);

        sut.RowCount.Should().Be(3);
    }
}
