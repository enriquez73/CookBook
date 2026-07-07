using Cookbook.Domain.Common;

namespace Cookbook.UnitTests;

public sealed class NameUtilityTests
{
    [Fact]
    public void Normalize_TrimsLeadingAndTrailingWhitespace()
    {
        var normalized = NameUtility.Normalize("  Risotto ai funghi  ");

        Assert.Equal("Risotto ai funghi", normalized);
    }
}

