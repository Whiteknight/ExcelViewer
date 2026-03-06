using AwesomeAssertions;
using ExcelTerminalViewer.Features.Display;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests.Features.Display;

[TestFixture]
public class HelpContentEntriesTests
{
    [Test]
    public void Entries_IsNotEmpty()
    {
        HelpContent.Entries.Should().NotBeEmpty();
    }

    [Test]
    public void Entries_AllHaveNonEmptyKey()
    {
        foreach (var (key, _) in HelpContent.Entries)
            key.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void Entries_AllHaveNonEmptyDescription()
    {
        foreach (var (_, description) in HelpContent.Entries)
            description.Should().NotBeNullOrWhiteSpace();
    }
}

[TestFixture]
public class HelpContentFormatHelpTextTests
{
    [Test]
    public void FormatHelpText_ContainsAllKeys()
    {
        var text = HelpContent.FormatHelpText();

        foreach (var (key, _) in HelpContent.Entries)
            text.Should().Contain(key);
    }

    [Test]
    public void FormatHelpText_ContainsAllDescriptions()
    {
        var text = HelpContent.FormatHelpText();

        foreach (var (_, description) in HelpContent.Entries)
            text.Should().Contain(description);
    }

    [Test]
    public void FormatHelpText_HasOneLinePerEntry()
    {
        var text = HelpContent.FormatHelpText();
        var lines = text.Split('\n');

        lines.Should().HaveCount(HelpContent.Entries.Count);
    }

    [Test]
    public void FormatHelpText_LinesAreAligned()
    {
        var text = HelpContent.FormatHelpText();
        var lines = text.Split('\n');
        var maxKeyLen = HelpContent.Entries.Max(e => e.Key.Length);

        foreach (var line in lines)
        {
            // Each line starts with 2 spaces, then key padded to maxKeyLen, then 3 spaces
            var trimmed = line.TrimStart();
            line.Should().StartWith("  ");
            line.Length.Should().BeGreaterThan(maxKeyLen + 5);
        }
    }
}
