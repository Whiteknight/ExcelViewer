using AwesomeAssertions;
using ExcelTerminalViewer.Features.FileLoading;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.FileLoading;

[TestFixture]
public class FileLoaderTests
{
    [TestCase("data.xlsx")]
    [TestCase("data.xls")]
    [TestCase("data.csv")]
    public void Load_SupportedExtension_DoesNotReturnUnsupportedError(string fileName)
    {
        var result = FileLoader.Load(fileName);

        // The file doesn't exist, so it will error — but NOT with "Unsupported file extension"
        result.IsError.Should().BeTrue();
        var error = result.Match(static _ => "", static e => e.Message);
        error.Should().NotContain("Unsupported file extension");
    }

    [TestCase("data.XLSX")]
    [TestCase("data.Csv")]
    [TestCase("data.XLS")]
    public void Load_UppercaseExtension_DoesNotReturnUnsupportedError(string fileName)
    {
        var result = FileLoader.Load(fileName);

        result.IsError.Should().BeTrue();
        var error = result.Match(static _ => "", static e => e.Message);
        error.Should().NotContain("Unsupported file extension");
    }

    [TestCase("data.txt")]
    [TestCase("data.json")]
    [TestCase("data.pdf")]
    [TestCase("data")]
    public void Load_UnsupportedExtension_ReturnsErrorListingSupportedFormats(string fileName)
    {
        var result = FileLoader.Load(fileName);

        result.IsError.Should().BeTrue();
        var error = result.Match(static _ => "", static e => e.Message);
        error.Should().Contain(".xlsx");
        error.Should().Contain(".xls");
        error.Should().Contain(".csv");
    }
}
