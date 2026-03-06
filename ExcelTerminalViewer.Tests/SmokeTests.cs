using AwesomeAssertions;
using NUnit.Framework;

namespace ExcelTerminalViewer.Tests;

[TestFixture]
public class SmokeTests
{
    [Test]
    public void TestInfrastructure_ShouldWork()
    {
        var result = 1 + 1;
        result.Should().Be(2);
    }
}
