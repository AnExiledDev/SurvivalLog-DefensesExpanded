using DefensesExpanded.Core;
using Xunit;

namespace DefensesExpanded.Tests;

public class HealthScaleTests
{
    [Fact]
    public void AnUnchangedHealthSettingLeavesATalentBoostedDefenseAlone()
    {
        // Row 600, the talent took it to 900, damaged to 700: changing another setting must not cost it health.
        Assert.Equal((900, 700), HealthScale.Rescale(900, 700, 600, 600));
    }

    [Fact]
    public void ANewHealthSettingKeepsTheShareAndTheBonus()
    {
        Assert.Equal((1800, 1400), HealthScale.Rescale(900, 700, 600, 1200));
        Assert.Equal((750, 300), HealthScale.Rescale(1500, 600, 1500, 750));
    }

    [Fact]
    public void ABrokenDefenseStaysBroken()
    {
        Assert.Equal((3000, 0), HealthScale.Rescale(1500, 0, 1500, 3000));
    }
}
