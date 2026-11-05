using DefensesExpanded.Core;
using Xunit;

namespace DefensesExpanded.Tests;

// A game without the mod drops what it does not know, so Uninstall prep must turn every defense id into the stock
// defense it copies, and leave everything else alone.
public class UninstallTests
{
    [Theory]
    [InlineData("Barrier", 36001, 26001)]
    [InlineData("Wire", 36002, 26002)]
    [InlineData("Spikes", 36002, 26002)]
    [InlineData("Turret", 36004, 26004)]
    [InlineData("Tesla", 36003, 26003)]
    public void EachDefenseBecomesTheStockDefenseItCopies(string key, int furniture, int package)
    {
        var kind = DefenseRows.All.Single(kind => kind.Key == key);

        Assert.Equal(furniture, Uninstall.StockFurniture(kind.Ids.Furniture, DefenseRows.All));
        Assert.Equal(package, Uninstall.StockItem(kind.Ids.Item, DefenseRows.All));
    }

    [Theory]
    [InlineData(36001)]
    [InlineData(26004)]
    [InlineData(920001)]
    [InlineData(0)]
    public void AnythingElseIsLeftAlone(int id)
    {
        Assert.Null(Uninstall.StockItem(id, DefenseRows.All));
        Assert.Null(Uninstall.StockFurniture(id, DefenseRows.All));
    }
}
