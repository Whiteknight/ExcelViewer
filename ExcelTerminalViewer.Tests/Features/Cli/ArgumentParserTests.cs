using AwesomeAssertions;
using ExcelTerminalViewer.Features.Cli;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.Cli;

[TestFixture]
public class ArgumentParserTests
{
    [Test]
    public void Parse_NoArgs_ReturnsUsageError()
    {
        var result = ArgumentParser.Parse([]);

        result.IsError.Should().BeTrue();
        result.Match(static _ => "", static e => e.Message)
            .Should().Contain("Usage:");
    }

    [Test]
    public void Parse_FilePathOnly_ReturnsOptionsWithDefaultMaxWidth()
    {
        var result = ArgumentParser.Parse(["data.csv"]);

        result.IsSuccess.Should().BeTrue();
        result.Match(static o => o, static _ => null!)
            .Should().BeEquivalentTo(new CliOptions("data.csv", 30));
    }

    [Test]
    public void Parse_FilePathWithValidMaxWidth_ReturnsOptionsWithSpecifiedMaxWidth()
    {
        var result = ArgumentParser.Parse(["data.xlsx", "--max-width", "50"]);

        result.IsSuccess.Should().BeTrue();
        result.Match(static o => o, static _ => null!)
            .Should().BeEquivalentTo(new CliOptions("data.xlsx", 50));
    }

    [Test]
    public void Parse_MaxWidthAtLowerBound_ReturnsSuccess()
    {
        var result = ArgumentParser.Parse(["file.csv", "--max-width", "5"]);

        result.IsSuccess.Should().BeTrue();
        result.Match(static o => o.MaxWidth, static _ => -1)
            .Should().Be(5);
    }

    [Test]
    public void Parse_MaxWidthAtUpperBound_ReturnsSuccess()
    {
        var result = ArgumentParser.Parse(["file.csv", "--max-width", "200"]);

        result.IsSuccess.Should().BeTrue();
        result.Match(static o => o.MaxWidth, static _ => -1)
            .Should().Be(200);
    }

    [Test]
    public void Parse_MaxWidthBelowLowerBound_ReturnsRangeError()
    {
        var result = ArgumentParser.Parse(["file.csv", "--max-width", "4"]);

        result.IsError.Should().BeTrue();
        result.Match(static _ => "", static e => e.Message)
            .Should().Contain("between 5 and 200");
    }

    [Test]
    public void Parse_MaxWidthAboveUpperBound_ReturnsRangeError()
    {
        var result = ArgumentParser.Parse(["file.csv", "--max-width", "201"]);

        result.IsError.Should().BeTrue();
        result.Match(static _ => "", static e => e.Message)
            .Should().Contain("between 5 and 200");
    }

    [Test]
    public void Parse_MaxWidthNotAnInteger_ReturnsParseError()
    {
        var result = ArgumentParser.Parse(["file.csv", "--max-width", "abc"]);

        result.IsError.Should().BeTrue();
        result.Match(static _ => "", static e => e.Message)
            .Should().Contain("must be an integer").And.Contain("abc");
    }

    [Test]
    public void Parse_MaxWidthMissingValue_ReturnsError()
    {
        var result = ArgumentParser.Parse(["file.csv", "--max-width"]);

        result.IsError.Should().BeTrue();
        result.Match(static _ => "", static e => e.Message)
            .Should().Contain("requires a value");
    }

    [Test]
    public void Parse_DefaultMaxWidth_Is30()
    {
        var result = ArgumentParser.Parse(["report.xls"]);

        result.IsSuccess.Should().BeTrue();
        result.Match(static o => o.MaxWidth, static _ => -1)
            .Should().Be(30);
    }

    [Test]
    public void Parse_GoToRow_ReturnsSpecifiedRow()
    {
        var result = ArgumentParser.Parse(["data.csv", "--go-to", "42"]);

        result.IsSuccess.Should().BeTrue();
        result.Match(static o => o.GoToRow, static _ => null)
            .Should().Be(42);
    }

    [Test]
    public void Parse_GoToRowNotProvided_ReturnsNull()
    {
        var result = ArgumentParser.Parse(["data.csv"]);

        result.IsSuccess.Should().BeTrue();
        result.Match(static o => o.GoToRow, static _ => -1)
            .Should().BeNull();
    }

    [Test]
    public void Parse_GoToRowMissingValue_ReturnsError()
    {
        var result = ArgumentParser.Parse(["data.csv", "--go-to"]);

        result.IsError.Should().BeTrue();
        result.Match(static _ => "", static e => e.Message)
            .Should().Contain("requires a value");
    }

    [Test]
    public void Parse_GoToRowNotAnInteger_ReturnsError()
    {
        var result = ArgumentParser.Parse(["data.csv", "--go-to", "abc"]);

        result.IsError.Should().BeTrue();
        result.Match(static _ => "", static e => e.Message)
            .Should().Contain("must be an integer").And.Contain("abc");
    }

    [Test]
    public void Parse_GoToRowZero_ReturnsError()
    {
        var result = ArgumentParser.Parse(["data.csv", "--go-to", "0"]);

        result.IsError.Should().BeTrue();
        result.Match(static _ => "", static e => e.Message)
            .Should().Contain("at least 1");
    }

    [Test]
    public void Parse_GoToRowNegative_ReturnsError()
    {
        var result = ArgumentParser.Parse(["data.csv", "--go-to", "-5"]);

        result.IsError.Should().BeTrue();
        result.Match(static _ => "", static e => e.Message)
            .Should().Contain("at least 1");
    }

    [Test]
    public void Parse_GoToRowWithMaxWidth_ReturnsBoth()
    {
        var result = ArgumentParser.Parse(["data.csv", "--max-width", "50", "--go-to", "10"]);

        result.IsSuccess.Should().BeTrue();
        var options = result.Match(static o => o, static _ => null!);
        options.MaxWidth.Should().Be(50);
        options.GoToRow.Should().Be(10);
    }
}
