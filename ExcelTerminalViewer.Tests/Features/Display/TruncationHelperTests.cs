using AwesomeAssertions;
using ExcelTerminalViewer.Features.Display;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.Display;

[TestFixture]
public class TruncationHelperTests
{
    [Test]
    public void Truncate_EmptyString_ReturnsEmpty()
    {
        TruncationHelper.Truncate("", 30).Should().BeEmpty();
    }

    [Test]
    public void Truncate_StringShorterThanLimit_ReturnsUnchanged()
    {
        TruncationHelper.Truncate("hello", 30).Should().Be("hello");
    }

    [Test]
    public void Truncate_StringExactlyAtLimit_ReturnsUnchanged()
    {
        var input = new string('a', 10);
        TruncationHelper.Truncate(input, 10).Should().Be(input);
    }

    [Test]
    public void Truncate_StringOneCharOverLimit_TruncatesWithEllipsis()
    {
        var input = new string('a', 11);
        var result = TruncationHelper.Truncate(input, 10);

        result.Should().Be(new string('a', 10) + "\u2026");
    }

    [Test]
    public void Truncate_StringWellOverLimit_TruncatesWithEllipsis()
    {
        var input = new string('x', 100);
        var result = TruncationHelper.Truncate(input, 5);

        result.Should().Be("xxxxx\u2026");
        result.Length.Should().Be(6);
    }

    [Test]
    public void Truncate_TruncatedResult_StartsWithOriginalPrefix()
    {
        var input = "abcdefghijklmnop";
        var result = TruncationHelper.Truncate(input, 5);

        result.Should().StartWith("abcde");
    }

    [Test]
    public void Truncate_TruncatedResult_EndsWithEllipsis()
    {
        var input = "abcdefghijklmnop";
        var result = TruncationHelper.Truncate(input, 5);

        result[^1].Should().Be('\u2026');
    }

    [Test]
    public void Truncate_UnicodeCharacters_TruncatesCorrectly()
    {
        var input = "héllo wörld café";
        var result = TruncationHelper.Truncate(input, 5);

        result.Should().Be("héllo\u2026");
    }
}
