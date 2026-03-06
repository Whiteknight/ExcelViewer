using AwesomeAssertions;
using ExcelTerminalViewer.Features.Display;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.Display;

[TestFixture]
public class StatusBarHelperTests
{
    [Test]
    public void FormatPosition_TypicalValues_ContainsAllComponents()
    {
        var result = StatusBarHelper.FormatPosition(2, 3, 100, 10, "data.csv");

        result.Should().Contain("data.csv");
        result.Should().Contain("Row 3/100");
        result.Should().Contain("Col 4/10");
    }

    [Test]
    public void FormatPosition_ZeroBasedIndices_DisplaysAsOneBased()
    {
        var result = StatusBarHelper.FormatPosition(0, 0, 50, 5, "test.xlsx");

        result.Should().Contain("Row 1/50");
        result.Should().Contain("Col 1/5");
    }

    [Test]
    public void FormatPosition_LastRowLastCol_DisplaysCorrectly()
    {
        var result = StatusBarHelper.FormatPosition(99, 9, 100, 10, "sheet.xls");

        result.Should().Contain("Row 100/100");
        result.Should().Contain("Col 10/10");
    }

    [Test]
    public void FormatPosition_FileNameWithPath_ContainsFileName()
    {
        var result = StatusBarHelper.FormatPosition(0, 0, 1, 1, "some/path/file.csv");

        result.Should().Contain("some/path/file.csv");
    }

    [Test]
    public void FormatPosition_SingleRowSingleCol_DisplaysCorrectly()
    {
        var result = StatusBarHelper.FormatPosition(0, 0, 1, 1, "tiny.csv");

        result.Should().Contain("Row 1/1");
        result.Should().Contain("Col 1/1");
    }
}
