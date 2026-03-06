using AwesomeAssertions;
using ExcelTerminalViewer.Domain;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Domain;

[TestFixture]
public class CellNormalizerTests
{
    [Test]
    public void Normalize_Null_ReturnsEmptyString()
    {
        CellNormalizer.Normalize(null).Should().BeEmpty();
    }

    [Test]
    public void Normalize_EmptyString_ReturnsEmptyString()
    {
        CellNormalizer.Normalize("").Should().BeEmpty();
    }

    [Test]
    public void Normalize_NoNewlines_ReturnsUnchanged()
    {
        CellNormalizer.Normalize("hello world").Should().Be("hello world");
    }

    [Test]
    public void Normalize_CarriageReturnLineFeed_ReplacesWithSpace()
    {
        CellNormalizer.Normalize("line1\r\nline2").Should().Be("line1 line2");
    }

    [Test]
    public void Normalize_CarriageReturn_ReplacesWithSpace()
    {
        CellNormalizer.Normalize("line1\rline2").Should().Be("line1 line2");
    }

    [Test]
    public void Normalize_LineFeed_ReplacesWithSpace()
    {
        CellNormalizer.Normalize("line1\nline2").Should().Be("line1 line2");
    }

    [Test]
    public void Normalize_MixedNewlines_ReplacesEachWithSingleSpace()
    {
        CellNormalizer.Normalize("a\r\nb\rc\nd").Should().Be("a b c d");
    }

    [Test]
    public void Normalize_ConsecutiveCrLf_EachBecomesOneSpace()
    {
        CellNormalizer.Normalize("a\r\n\r\nb").Should().Be("a  b");
    }
}
