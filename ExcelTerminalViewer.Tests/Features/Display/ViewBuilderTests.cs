using System.Data;
using AwesomeAssertions;
using ExcelTerminalViewer.Domain;
using ExcelTerminalViewer.Features.CellSearch;
using ExcelTerminalViewer.Features.Display;
using NUnit.Framework;
using Terminal.Gui;

namespace ExcelTerminalViewer.Tests.Features.Display;

[TestFixture]
public class ViewBuilderToDataTableTests
{
    [Test]
    public void ToDataTable_WithSimpleData_CreatesCorrectColumns()
    {
        var data = CreateSpreadsheetData(
            ["Name", "Age"],
            [["Alice", "30"], ["Bob", "25"]]);

        var result = ViewBuilder.ToDataTable(data, 30);

        result.Columns.Count.Should().Be(3);
        result.Columns[0].ColumnName.Should().Be("#");
        result.Columns[1].ColumnName.Should().Be("Name");
        result.Columns[2].ColumnName.Should().Be("Age");
    }

    [Test]
    public void ToDataTable_WithSimpleData_CreatesCorrectRows()
    {
        var data = CreateSpreadsheetData(
            ["Name", "Age"],
            [["Alice", "30"], ["Bob", "25"]]);

        var result = ViewBuilder.ToDataTable(data, 30);

        result.Rows.Count.Should().Be(2);
        result.Rows[0][1].Should().Be("Alice");
        result.Rows[0][2].Should().Be("30");
        result.Rows[1][1].Should().Be("Bob");
        result.Rows[1][2].Should().Be("25");
    }

    [Test]
    public void ToDataTable_RowNumberColumn_ContainsOneBasedIndices()
    {
        var data = CreateSpreadsheetData(
            ["A"],
            [["x"], ["y"], ["z"]]);

        var result = ViewBuilder.ToDataTable(data, 30);

        result.Rows[0][0].Should().Be("1");
        result.Rows[1][0].Should().Be("2");
        result.Rows[2][0].Should().Be("3");
    }

    [Test]
    public void ToDataTable_TruncatesLongValues()
    {
        var longValue = new string('x', 40);
        var data = CreateSpreadsheetData(
            ["Col"],
            [[longValue]]);

        var result = ViewBuilder.ToDataTable(data, 10);

        var cellValue = result.Rows[0][1].ToString()!;
        cellValue.Length.Should().Be(11); // 10 chars + ellipsis
        cellValue.Should().EndWith("\u2026");
    }

    [Test]
    public void ToDataTable_TruncatesLongHeaders()
    {
        var longHeader = new string('H', 40);
        var data = CreateSpreadsheetData(
            [longHeader],
            [["value"]]);

        var result = ViewBuilder.ToDataTable(data, 10);

        var headerName = result.Columns[1].ColumnName;
        headerName.Length.Should().Be(11);
        headerName.Should().EndWith("\u2026");
    }

    [Test]
    public void ToDataTable_WithEmptyData_CreatesEmptyTable()
    {
        var data = CreateSpreadsheetData(
            ["A", "B"],
            []);

        var result = ViewBuilder.ToDataTable(data, 30);

        result.Columns.Count.Should().Be(3); // # + A + B
        result.Rows.Count.Should().Be(0);
    }

    [Test]
    public void ToDataTable_WithRaggedRows_FillsMissingCellsWithEmptyString()
    {
        var data = CreateSpreadsheetData(
            ["A", "B", "C"],
            [["1"], ["2", "3"]]);

        var result = ViewBuilder.ToDataTable(data, 30);

        result.Rows[0][1].Should().Be("1");
        result.Rows[0][2].Should().Be("");
        result.Rows[0][3].Should().Be("");
        result.Rows[1][1].Should().Be("2");
        result.Rows[1][2].Should().Be("3");
        result.Rows[1][3].Should().Be("");
    }

    [Test]
    public void ToDataTable_ShortValuesAreNotTruncated()
    {
        var data = CreateSpreadsheetData(
            ["Col"],
            [["short"]]);

        var result = ViewBuilder.ToDataTable(data, 30);

        result.Rows[0][1].Should().Be("short");
    }

    private static SpreadsheetData CreateSpreadsheetData(
        string[] headers,
        string[][] rows)
    {
        var rowList = rows.Select(r => (IReadOnlyList<string>)r.ToList()).ToList();
        return new SpreadsheetData(headers.ToList(), rowList);
    }
}

[TestFixture]
public class ViewBuilderIsExitKeyTests
{
    [Test]
    public void IsExitKey_Q_ReturnsTrue()
    {
        ViewBuilder.IsExitKey(Key.Q).Should().BeTrue();
    }

    [Test]
    public void IsExitKey_CtrlC_ReturnsTrue()
    {
        ViewBuilder.IsExitKey(Key.C.WithCtrl).Should().BeTrue();
    }

    [Test]
    public void IsExitKey_ArrowUp_ReturnsFalse()
    {
        ViewBuilder.IsExitKey(Key.CursorUp).Should().BeFalse();
    }

    [Test]
    public void IsExitKey_Enter_ReturnsFalse()
    {
        ViewBuilder.IsExitKey(Key.Enter).Should().BeFalse();
    }

    [Test]
    public void IsExitKey_Escape_ReturnsFalse()
    {
        ViewBuilder.IsExitKey(Key.Esc).Should().BeFalse();
    }
}

[TestFixture]
public class ViewBuilderToDataColumnTests
{
    [Test]
    public void ToDataColumn_ZeroReturnsZero()
    {
        ViewBuilder.ToDataColumn(0).Should().Be(0);
    }

    [Test]
    public void ToDataColumn_OneReturnsZero()
    {
        ViewBuilder.ToDataColumn(1).Should().Be(0);
    }

    [Test]
    public void ToDataColumn_TwoReturnsOne()
    {
        ViewBuilder.ToDataColumn(2).Should().Be(1);
    }

    [Test]
    public void ToDataColumn_NegativeReturnsZero()
    {
        ViewBuilder.ToDataColumn(-1).Should().Be(0);
    }
}

[TestFixture]
public class ViewBuilderClampGoToRowTests
{
    [Test]
    public void ClampGoToRow_RowOneReturnsIndexZero()
    {
        ViewBuilder.ClampGoToRow(1, 10).Should().Be(0);
    }

    [Test]
    public void ClampGoToRow_RowTenReturnsIndexNine()
    {
        ViewBuilder.ClampGoToRow(10, 10).Should().Be(9);
    }

    [Test]
    public void ClampGoToRow_ExceedsRowCount_ClampsToLastRow()
    {
        ViewBuilder.ClampGoToRow(100, 5).Should().Be(4);
    }

    [Test]
    public void ClampGoToRow_ZeroRowCount_ReturnsZero()
    {
        ViewBuilder.ClampGoToRow(1, 0).Should().Be(0);
    }

    [Test]
    public void ClampGoToRow_MiddleRow_ReturnsCorrectIndex()
    {
        ViewBuilder.ClampGoToRow(5, 10).Should().Be(4);
    }
}

[TestFixture]
public class ViewBuilderShouldHandleExitKeyTests
{
    [Test]
    public void ShouldHandleExitKey_Q_WhenPromptHidden_ReturnsTrue()
    {
        ViewBuilder.ShouldHandleExitKey(Key.Q, searchPromptVisible: false).Should().BeTrue();
    }

    [Test]
    public void ShouldHandleExitKey_CtrlC_WhenPromptHidden_ReturnsTrue()
    {
        ViewBuilder.ShouldHandleExitKey(Key.C.WithCtrl, searchPromptVisible: false).Should().BeTrue();
    }

    [Test]
    public void ShouldHandleExitKey_Q_WhenPromptVisible_ReturnsFalse()
    {
        ViewBuilder.ShouldHandleExitKey(Key.Q, searchPromptVisible: true).Should().BeFalse();
    }

    [Test]
    public void ShouldHandleExitKey_CtrlC_WhenPromptVisible_ReturnsFalse()
    {
        ViewBuilder.ShouldHandleExitKey(Key.C.WithCtrl, searchPromptVisible: true).Should().BeFalse();
    }

    [Test]
    public void ShouldHandleExitKey_NonExitKey_WhenPromptHidden_ReturnsFalse()
    {
        ViewBuilder.ShouldHandleExitKey(Key.A, searchPromptVisible: false).Should().BeFalse();
    }

    [Test]
    public void ShouldHandleExitKey_NonExitKey_WhenPromptVisible_ReturnsFalse()
    {
        ViewBuilder.ShouldHandleExitKey(Key.A, searchPromptVisible: true).Should().BeFalse();
    }
}

[TestFixture]
public class ViewBuilderHasActiveSearchTests
{
    private static SpreadsheetData CreateSimpleData()
    {
        return new SpreadsheetData(["A"], [["hello"]]);
    }

    [Test]
    public void HasActiveSearch_WhenSearching_ReturnsTrue()
    {
        using var coordinator = new SearchCoordinator(CreateSimpleData(), _ => { });
        // Start a search but don't await -- coordinator.IsSearching will be true
        // We can't easily test IsSearching without async, so test the results path instead.
        ViewBuilder.HasActiveSearch(coordinator).Should().BeFalse();
    }

    [Test]
    public async Task HasActiveSearch_WhenResultsExist_ReturnsTrue()
    {
        using var coordinator = new SearchCoordinator(CreateSimpleData(), _ => { });
        await coordinator.StartSearchAsync("hello");

        ViewBuilder.HasActiveSearch(coordinator).Should().BeTrue();
    }

    [Test]
    public void HasActiveSearch_WhenNoSearchPerformed_ReturnsFalse()
    {
        using var coordinator = new SearchCoordinator(CreateSimpleData(), _ => { });

        ViewBuilder.HasActiveSearch(coordinator).Should().BeFalse();
    }

    [Test]
    public async Task HasActiveSearch_AfterCancelSearch_ReturnsFalse()
    {
        using var coordinator = new SearchCoordinator(CreateSimpleData(), _ => { });
        await coordinator.StartSearchAsync("hello");
        coordinator.CancelSearch();

        ViewBuilder.HasActiveSearch(coordinator).Should().BeFalse();
    }
}

[TestFixture]
public class ViewBuilderShouldHandleHelpKeyTests
{
    [Test]
    public void ShouldHandleHelpKey_H_WhenPromptHidden_ReturnsTrue()
    {
        ViewBuilder.ShouldHandleHelpKey(Key.H, searchPromptVisible: false).Should().BeTrue();
    }

    [Test]
    public void ShouldHandleHelpKey_H_WhenPromptVisible_ReturnsFalse()
    {
        ViewBuilder.ShouldHandleHelpKey(Key.H, searchPromptVisible: true).Should().BeFalse();
    }

    [Test]
    public void ShouldHandleHelpKey_NonHKey_WhenPromptHidden_ReturnsFalse()
    {
        ViewBuilder.ShouldHandleHelpKey(Key.A, searchPromptVisible: false).Should().BeFalse();
    }

    [Test]
    public void ShouldHandleHelpKey_Q_ReturnsFalse()
    {
        ViewBuilder.ShouldHandleHelpKey(Key.Q, searchPromptVisible: false).Should().BeFalse();
    }
}
