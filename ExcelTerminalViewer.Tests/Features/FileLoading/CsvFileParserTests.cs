using AwesomeAssertions;
using ExcelTerminalViewer.Domain;
using ExcelTerminalViewer.Features.FileLoading;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.FileLoading;

[TestFixture]
public class CsvFileParserTests
{
    private readonly CsvFileParser _parser = new();

    [Test]
    public void Parse_SimpleCsv_ReturnsHeadersAndRows()
    {
        var path = WriteTempCsv("Name,Age\nAlice,30\nBob,25");

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
    public void Parse_QuotedFields_ParsesCorrectly()
    {
        var path = WriteTempCsv("Col1,Col2\n\"hello, world\",\"value\"");

        var result = _parser.Parse(path);

        var data = AssertSuccess(result);
        data.GetCell(0, 0).Should().Be("hello, world");
        data.GetCell(0, 1).Should().Be("value");
    }

    [Test]
    public void Parse_EmptyFields_StoresEmptyStrings()
    {
        var path = WriteTempCsv("A,B,C\n,hello,\nworld,,");

        var result = _parser.Parse(path);

        var data = AssertSuccess(result);
        data.GetCell(0, 0).Should().BeEmpty();
        data.GetCell(0, 1).Should().Be("hello");
        data.GetCell(0, 2).Should().BeEmpty();
        data.GetCell(1, 0).Should().Be("world");
        data.GetCell(1, 1).Should().BeEmpty();
        data.GetCell(1, 2).Should().BeEmpty();
    }

    [Test]
    public void Parse_NewlinesInCells_NormalizesToSpaces()
    {
        var path = WriteTempCsv("H1,H2\n\"line1\r\nline2\",\"line3\nline4\"");

        var result = _parser.Parse(path);

        var data = AssertSuccess(result);
        data.GetCell(0, 0).Should().Be("line1 line2");
        data.GetCell(0, 1).Should().Be("line3 line4");
    }

    [Test]
    public void Parse_NonExistentFile_ReturnsError()
    {
        var result = _parser.Parse("nonexistent_file_12345.csv");

        result.IsError.Should().BeTrue();
    }

    [Test]
    public void Parse_HeadersOnly_ReturnsEmptyRows()
    {
        var path = WriteTempCsv("A,B,C");

        var result = _parser.Parse(path);

        var data = AssertSuccess(result);
        data.Headers.Should().BeEquivalentTo(["A", "B", "C"]);
        data.RowCount.Should().Be(0);
    }

    private static string WriteTempCsv(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        File.WriteAllText(path, content);
        return path;
    }

    private static SpreadsheetData AssertSuccess(Result<SpreadsheetData, FileLoadError> result)
    {
        result.IsSuccess.Should().BeTrue();
        return result.Match(static d => d, static _ => throw new InvalidOperationException());
    }
}
