using AwesomeAssertions;
using ClosedXML.Excel;
using ExcelTerminalViewer.Domain;
using ExcelTerminalViewer.Features.FileLoading;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.FileLoading;

[TestFixture]
public class XlsxFileParserTests
{
    private readonly XlsxFileParser _parser = new();

    [Test]
    public void Parse_SimpleWorkbook_ReturnsHeadersAndRows()
    {
        var path = CreateSimpleWorkbook();

        var result = _parser.Parse(path);

        var data = AssertSuccess(result);
        data.Headers.Should().BeEquivalentTo(["Name", "Age"]);
        data.RowCount.Should().Be(2);
        data.GetCell(0, 0).Should().Be("Alice");
        data.GetCell(0, 1).Should().Be("30");
        data.GetCell(1, 0).Should().Be("Bob");
        data.GetCell(1, 1).Should().Be("25");
    }

    [Test]
    public void Parse_FormulaCells_ReturnsDisplayValue()
    {
        var path = CreateWorkbookWithFormula();

        var result = _parser.Parse(path);

        var data = AssertSuccess(result);
        data.Headers.Should().BeEquivalentTo(["A", "B", "Sum"]);
        data.GetCell(0, 0).Should().Be("10");
        data.GetCell(0, 1).Should().Be("20");
        data.GetCell(0, 2).Should().Be("30");
    }

    [Test]
    public void Parse_EmptyCells_StoresEmptyStrings()
    {
        var path = CreateWorkbookWithEmptyCells();

        var result = _parser.Parse(path);

        var data = AssertSuccess(result);
        data.GetCell(0, 0).Should().Be("hello");
        data.GetCell(0, 1).Should().BeEmpty();
        data.GetCell(0, 2).Should().Be("world");
    }

    [Test]
    public void Parse_NewlinesInCells_NormalizesToSpaces()
    {
        var path = CreateWorkbookWithNewlines();

        var result = _parser.Parse(path);

        var data = AssertSuccess(result);
        data.GetCell(0, 0).Should().Be("line1 line2");
    }

    [Test]
    public void Parse_NonExistentFile_ReturnsError()
    {
        var result = _parser.Parse("nonexistent_file_12345.xlsx");

        result.IsError.Should().BeTrue();
    }

    private static string CreateSimpleWorkbook()
    {
        var path = TempXlsxPath();
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sheet1");
        ws.Cell(1, 1).Value = "Name";
        ws.Cell(1, 2).Value = "Age";
        ws.Cell(2, 1).Value = "Alice";
        ws.Cell(2, 2).Value = 30;
        ws.Cell(3, 1).Value = "Bob";
        ws.Cell(3, 2).Value = 25;
        workbook.SaveAs(path);
        return path;
    }

    private static string CreateWorkbookWithFormula()
    {
        var path = TempXlsxPath();
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sheet1");
        ws.Cell(1, 1).Value = "A";
        ws.Cell(1, 2).Value = "B";
        ws.Cell(1, 3).Value = "Sum";
        ws.Cell(2, 1).Value = 10;
        ws.Cell(2, 2).Value = 20;
        ws.Cell(2, 3).FormulaA1 = "=A2+B2";
        workbook.SaveAs(path);
        return path;
    }

    private static string CreateWorkbookWithEmptyCells()
    {
        var path = TempXlsxPath();
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sheet1");
        ws.Cell(1, 1).Value = "H1";
        ws.Cell(1, 2).Value = "H2";
        ws.Cell(1, 3).Value = "H3";
        ws.Cell(2, 1).Value = "hello";
        // Cell(2,2) intentionally left empty
        ws.Cell(2, 3).Value = "world";
        workbook.SaveAs(path);
        return path;
    }

    private static string CreateWorkbookWithNewlines()
    {
        var path = TempXlsxPath();
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Sheet1");
        ws.Cell(1, 1).Value = "Header";
        ws.Cell(2, 1).Value = "line1\r\nline2";
        workbook.SaveAs(path);
        return path;
    }

    private static string TempXlsxPath() =>
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

    private static SpreadsheetData AssertSuccess(Result<SpreadsheetData, FileLoadError> result)
    {
        result.IsSuccess.Should().BeTrue();
        return result.Match(static d => d, static _ => throw new InvalidOperationException());
    }
}
