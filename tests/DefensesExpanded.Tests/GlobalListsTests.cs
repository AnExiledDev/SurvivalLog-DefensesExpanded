using DefensesExpanded.Core;
using Xunit;

namespace DefensesExpanded.Tests;

// The game's entries in these settings must survive every write, and ours must never pile up.
public class GlobalListsTests
{
    private static readonly int[] Ours = { 930001, 930002 };

    [Fact]
    public void OurEntriesAreAddedAfterTheGames()
    {
        Assert.Equal("36001:0.5;36002:0.65;930001:0.5", GlobalLists.Merge("36001:0.5;36002:0.65", 0, Ours, new[] { "930001:0.5" }));
    }

    [Fact]
    public void WritingAgainReplacesOurOldEntriesInsteadOfAddingMore()
    {
        var once = GlobalLists.Merge("36001:200", 0, Ours, new[] { "930001:300", "930002:50" });

        Assert.Equal("36001:200;930001:320;930002:50", GlobalLists.Merge(once, 0, Ours, new[] { "930001:320", "930002:50" }));
    }

    [Fact]
    public void ADefenseNoLongerAddedLeavesNoEntryBehind()
    {
        Assert.Equal("36001:200", GlobalLists.Merge("36001:200;930001:300;930002:50", 0, Ours, Array.Empty<string>()));
    }

    // The zombie lists name the zombie first and the defense second.
    [Fact]
    public void TheDefenseIdIsReadFromTheFieldTheSettingKeepsItIn()
    {
        var raw = "9109:36004:1;9110:36004:1;9109:930002:1";

        Assert.Equal("9109:36004:1;9110:36004:1;9110:930002:1", GlobalLists.Merge(raw, 1, Ours, new[] { "9110:930002:1" }));
    }

    [Fact]
    public void AnEntryOnlyLookingLikeOursIsKept()
    {
        Assert.Equal("930001x:1;:930001;930001:2", GlobalLists.Merge("930001x:1;:930001", 0, Ours, new[] { "930001:2" }));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ; ;")]
    public void AnEmptySettingTakesJustOurs(string? raw)
    {
        Assert.Equal("930001:1", GlobalLists.Merge(raw, 0, Ours, new[] { "930001:1" }));
    }
}
