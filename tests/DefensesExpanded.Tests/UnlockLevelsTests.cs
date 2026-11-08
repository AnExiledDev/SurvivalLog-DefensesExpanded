using DefensesExpanded.Core;
using Xunit;

namespace DefensesExpanded.Tests;

public class UnlockLevelsTests
{
    private static readonly int[] Ours = { 900, 901, 902 };

    private static readonly Dictionary<int, int> At = new() { [900] = 2, [901] = 3, [902] = 0 };

    // A recipe of ours left behind at its old level would unlock there too, so every one comes out first.
    [Fact]
    public void OursMoveToTheirLevelAndLeaveEveryOther()
    {
        Assert.Equal(new[] { 404, 900 }, UnlockLevels.Rebuild(new[] { 404, 901 }, false, 2, Ours, At));
        Assert.Equal(new[] { 402, 901 }, UnlockLevels.Rebuild(new[] { 402, 900 }, false, 3, Ours, At));
        Assert.Equal(new[] { 403 }, UnlockLevels.Rebuild(new[] { 403, 900, 901 }, false, 4, Ours, At));
    }

    // An empty role list means the role uses the shared list, so adding ours to it would hide every stock recipe there.
    [Fact]
    public void AnEmptyRoleListStaysEmpty()
    {
        Assert.Empty(UnlockLevels.Rebuild(Array.Empty<int>(), true, 2, Ours, At));
    }

    // A role list of [0] is the role's own "nothing at this level", so it is a real list and gets ours.
    [Fact]
    public void ARoleListOfItsOwnGetsOurs()
    {
        Assert.Equal(new[] { 0, 900 }, UnlockLevels.Rebuild(new[] { 0 }, true, 2, Ours, At));
    }

    // Level 0 means no level is needed, so the recipe is in no list at all.
    [Fact]
    public void LevelZeroIsInNoList()
    {
        for (var level = 1; level <= DefenseRows.TopLevel; level++)
            Assert.DoesNotContain(902, UnlockLevels.Rebuild(new[] { 902 }, false, level, Ours, At));
    }
}

public class AimTests
{
    // The barrel points along +Z, and +X is a quarter turn clockwise seen from above, as Unity's yaw turns.
    [Fact]
    public void HeadingIsMeasuredFromForwardTowardRight()
    {
        Assert.Equal(0f, Aim.Heading(0f, 1f), 3);
        Assert.Equal(90f, Aim.Heading(1f, 0f), 3);
        Assert.Equal(-90f, Aim.Heading(-1f, 0f), 3);
    }

    // From 350 to 10 is 20 degrees forward, not 340 back.
    [Fact]
    public void TheHeadTurnsTheShortWayRound()
    {
        Assert.Equal(0f, Aim.Turn(350f, 10f, 10f), 3);
        Assert.Equal(10f, Aim.Turn(350f, 10f, 45f), 3);
        Assert.Equal(340f, Aim.Turn(10f, -20f, 30f), 3);
    }

    [Fact]
    public void ATurnNeverGoesPastItsMost()
    {
        Assert.Equal(15f, Aim.Turn(0f, 120f, 15f), 3);
        Assert.Equal(345f, Aim.Turn(0f, 200f, 15f), 3);
    }
}
