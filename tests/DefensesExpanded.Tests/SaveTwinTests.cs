using DefensesExpanded.Core;
using Xunit;

namespace DefensesExpanded.Tests;

public class SaveTwinTests
{
    public static IEnumerable<object[]> Keys => DefenseRows.All.Select(kind => new object[] { kind.Key });

    private static readonly IReadOnlyList<DefenseKind> Kinds = DefenseRows.All;

    private static DefenseKind Kind(string key) => DefenseRows.All.Single(kind => kind.Key == key);

    // Without the mod the game must find an id it knows, or the save does not load.
    [Theory]
    [InlineData("Barrier", 36001)]
    [InlineData("Wire", 36002)]
    [InlineData("Spikes", 36002)]
    [InlineData("Turret", 36004)]
    [InlineData("Tesla", 36003)]
    public void APlacedDefenseIsSavedAsTheStockDefenseItCopies(string key, int stock)
    {
        var kind = Kind(key);
        var save = new SaveTwin.Fields { AgentConfigId = kind.Ids.Furniture };

        Assert.True(SaveTwin.ToStock(ref save, Kinds));
        Assert.Equal(stock, save.AgentConfigId);
        Assert.StartsWith(kind.SaveMarker, save.Name);
    }

    [Theory]
    [MemberData(nameof(Keys))]
    public void LoadingWithTheModTurnsItBackIntoTheSameDefense(string key)
    {
        var kind = Kind(key);
        var save = new SaveTwin.Fields { AgentConfigId = kind.Ids.Furniture };

        SaveTwin.ToStock(ref save, Kinds);

        Assert.True(SaveTwin.FromStock(ref save, Kinds));
        Assert.Equal(kind.Ids.Furniture, save.AgentConfigId);
        Assert.Null(save.Name);
    }

    // The Barbed Wire and the Spike Plate are both saved as the Spike Barrier: only the marker tells them apart.
    [Fact]
    public void TwoDefensesSavedAsTheSameStockOneComeBackAsThemselves()
    {
        var wire = new SaveTwin.Fields { AgentConfigId = DefenseRows.Wire.Ids.Furniture };
        var spikes = new SaveTwin.Fields { AgentConfigId = DefenseRows.Spikes.Ids.Furniture };

        SaveTwin.ToStock(ref wire, Kinds);
        SaveTwin.ToStock(ref spikes, Kinds);
        SaveTwin.FromStock(ref wire, Kinds);
        SaveTwin.FromStock(ref spikes, Kinds);

        Assert.Equal((DefenseRows.Wire.Ids.Furniture, DefenseRows.Spikes.Ids.Furniture), (wire.AgentConfigId, spikes.AgentConfigId));
    }

    [Theory]
    [MemberData(nameof(Keys))]
    public void APackedDefenseSwapsItsPackedId(string key)
    {
        var kind = Kind(key);
        var save = new SaveTwin.Fields { AgentConfigId = 7, IsBagFurniture = true, BagFurnitureConfigId = kind.Ids.Furniture };

        Assert.True(SaveTwin.ToStock(ref save, Kinds));
        Assert.Equal((7, kind.Stock.Furniture), (save.AgentConfigId, save.BagFurnitureConfigId));

        Assert.True(SaveTwin.FromStock(ref save, Kinds));
        Assert.Equal((7, kind.Ids.Furniture), (save.AgentConfigId, save.BagFurnitureConfigId));
    }

    // A packed defense's own save names the bag it sits in; only the packed id the marker names is ours.
    [Fact]
    public void OnlyTheIdsTheMarkerNamesAreTurnedBack()
    {
        var kind = DefenseRows.Barrier;
        var save = new SaveTwin.Fields { AgentConfigId = kind.Stock.Furniture, IsBagFurniture = true, BagFurnitureConfigId = kind.Stock.Furniture, Name = kind.SaveMarker + "b" };

        Assert.True(SaveTwin.FromStock(ref save, Kinds));
        Assert.Equal((kind.Stock.Furniture, kind.Ids.Furniture), (save.AgentConfigId, save.BagFurnitureConfigId));
    }

    // A real Sandbags placed next to a Concrete Barrier must stay Sandbags.
    [Theory]
    [InlineData(36001)]
    [InlineData(36002)]
    [InlineData(36003)]
    [InlineData(36004)]
    public void ARealStockDefenseStaysItself(int stock)
    {
        var save = new SaveTwin.Fields { AgentConfigId = stock };

        Assert.False(SaveTwin.ToStock(ref save, Kinds));
        Assert.False(SaveTwin.FromStock(ref save, Kinds));
        Assert.Equal(stock, save.AgentConfigId);
        Assert.Null(save.Name);
    }

    [Fact]
    public void ASaveThatAlreadyHasANameIsLeftAlone()
    {
        var save = new SaveTwin.Fields { AgentConfigId = DefenseRows.Barrier.Ids.Furniture, Name = "Front door" };

        Assert.False(SaveTwin.ToStock(ref save, Kinds));
        Assert.Equal((DefenseRows.Barrier.Ids.Furniture, "Front door"), (save.AgentConfigId, save.Name));
    }

    [Fact]
    public void AnotherNameIsNotReadAsAMarker()
    {
        var save = new SaveTwin.Fields { AgentConfigId = 36001, Name = "Kitchen" };

        Assert.False(SaveTwin.FromStock(ref save, Kinds));
        Assert.Equal((36001, "Kitchen"), (save.AgentConfigId, save.Name));
    }
}
