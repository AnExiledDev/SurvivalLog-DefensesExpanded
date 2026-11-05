using System.Globalization;
using DefensesExpanded.Core;
using Xunit;

namespace DefensesExpanded.Tests;

public class DefenseRowsTests
{
    public static IEnumerable<object[]> Keys => DefenseRows.All.Select(kind => new object[] { kind.Key });

    private static DefenseKind Kind(string key) => DefenseRows.All.Single(kind => kind.Key == key);

    private static DefensePlan Plan(DefenseKind kind, DefenseSettings? settings = null) => DefenseRows.Plan(kind, MaterialList.Standard(kind.Defaults.Materials), settings ?? kind.Defaults);

    private static RowPlan Row(DefensePlan plan, Table table) => plan.Rows.Single(row => row.Table == table);

    private static object Field(DefensePlan plan, Table table, string name) => Row(plan, table).Fields.Single(field => field.Name == name).Value;

    // The game follows recipe -> package -> unpack action -> effect -> furniture, and finds the tower row by the
    // furniture id. One wrong link crafts or places the stock defense instead.
    [Theory]
    [MemberData(nameof(Keys))]
    public void EveryLinkPointsAtTheDefensesOwnRows(string key)
    {
        var plan = Plan(Kind(key));

        Assert.Equal(new[] { Row(plan, Table.Item).Id }, (int[])Field(plan, Table.Recipe, "ProductID"));
        Assert.Equal(Row(plan, Table.Furniture).Id, Field(plan, Table.Item, "TargetFurnitureID"));
        Assert.Equal(Row(plan, Table.Action).Id, Field(plan, Table.Item, "UseAction"));
        Assert.Equal(Row(plan, Table.Effect).Id, Field(plan, Table.Action, "EffectConfigID"));
        Assert.Equal(Row(plan, Table.Furniture).Id, Row(plan, Table.Tower).Id);
    }

    [Theory]
    [MemberData(nameof(Keys))]
    public void EachRowSetsItsOwnIdFieldAndCopiesTheStockDefense(string key)
    {
        var kind = Kind(key);
        var plan = Plan(kind);
        var copied = plan.Rows.ToDictionary(row => row.Table, row => row.CopyFrom);

        foreach (var row in plan.Rows) Assert.Equal(row.Id, Field(plan, row.Table, "ID"));

        Assert.Equal(kind.Stock.Recipe, copied[Table.Recipe]);
        Assert.Equal(kind.Stock.Item, copied[Table.Item]);
        Assert.Equal(kind.Stock.Action, copied[Table.Action]);
        Assert.Equal(kind.Stock.Effect, copied[Table.Effect]);
        Assert.Equal(kind.Stock.Furniture, copied[Table.Furniture]);
        Assert.Equal(kind.Stock.Furniture, copied[Table.Tower]);
        Assert.All(plan.Rows, row => Assert.NotEqual(row.CopyFrom, row.Id));
    }

    // Every id must be clear of the game's own and of the other defenses', or one row would replace another.
    [Fact]
    public void NoTwoDefensesShareAnIdAndNoneUsesAStockId()
    {
        var stock = new[] { DefenseRows.Sandbags, DefenseRows.SpikeBarrier, DefenseRows.ElectricFence, DefenseRows.Chainsaw };

        foreach (var pick in new Func<RowIds, int>[] { ids => ids.Item, ids => ids.Furniture, ids => ids.Recipe, ids => ids.Action, ids => ids.Effect })
        {
            var ours = DefenseRows.All.Select(kind => pick(kind.Ids)).ToList();

            Assert.Equal(ours.Count, ours.Distinct().Count());
            Assert.Empty(ours.Intersect(stock.Select(pick)));
        }

        Assert.Equal(DefenseRows.All.Count, DefenseRows.All.Select(kind => kind.SaveMarker).Distinct().Count());
        Assert.All(DefenseRows.All, kind => Assert.StartsWith(SaveTwin.MarkerStart, kind.SaveMarker));
    }

    // The settings decide the numbers the game reads: health on the furniture, the fight on the tower row.
    [Fact]
    public void TheSettingsReachTheRowsTheGameReads()
    {
        var kind = DefenseRows.Tesla;
        var settings = kind.Defaults with { Health = 777, Damage = 13, AttackInterval = 2.5f, AttackRange = 3f, MaxTargets = 5, SlowPercent = 40, SlowSeconds = 3f, PowerCost = 9, PowerPerHit = 1.5f };
        var plan = Plan(kind, settings);

        Assert.Equal(777, Field(plan, Table.Furniture, "FurnitureHP"));
        Assert.Equal(777, Field(plan, Table.Furniture, "FurnitureHPMax"));
        Assert.Equal(9, Field(plan, Table.Furniture, "PowerCost"));
        Assert.Equal(true, Field(plan, Table.Furniture, "IsElectrical"));
        Assert.Equal(13, Field(plan, Table.Tower, "Attack"));
        Assert.Equal(2.5f, Field(plan, Table.Tower, "AttackInterval"));
        Assert.Equal(3f, Field(plan, Table.Tower, "AttackRange"));
        Assert.Equal(5, Field(plan, Table.Tower, "MaxHitTargets"));
        Assert.Equal(0.4f, (float)Field(plan, Table.Tower, "HitSlowRatio"), 3);
        Assert.Equal(3f, Field(plan, Table.Tower, "HitSlowSeconds"));
        Assert.Equal(1.5f, Field(plan, Table.Tower, "PowerCostPerHit"));
    }

    // A barrier only blocks: even a setting file with attack numbers in it must not make it strike or draw power.
    [Fact]
    public void ARoleNeverGetsTheNumbersOfAnother()
    {
        var kind = DefenseRows.Barrier;
        var plan = Plan(kind, kind.Defaults with { Damage = 50, AttackInterval = 1f, AttackRange = 3f, MaxTargets = 4, SlowPercent = 50, PowerCost = 5, ShutsDownBelow = 0.5f });

        Assert.Equal(0, Field(plan, Table.Tower, "Attack"));
        Assert.Equal(0, Field(plan, Table.Tower, "MaxHitTargets"));
        Assert.Equal(0f, (float)Field(plan, Table.Tower, "HitSlowRatio"));
        Assert.Equal(0, Field(plan, Table.Furniture, "PowerCost"));
        Assert.Equal(false, Field(plan, Table.Furniture, "IsElectrical"));
        Assert.DoesNotContain(plan.Globals, entry => entry.Global == "TowerDeviceDisableThreshold");
    }

    // The spike barrier hurts whatever bites it; the copies do their harm by their own attack, or not at all.
    [Theory]
    [MemberData(nameof(Keys))]
    public void NoDefenseHurtsWhatBitesIt(string key) => Assert.Equal(0, Field(Plan(Kind(key)), Table.Tower, "CounterDamage"));

    // Walk-over defenses must be walk-over in the tower row, and the zombies that would skip them must go for them.
    [Theory]
    [MemberData(nameof(Keys))]
    public void WalkOverDefensesAreWalkOverAndTheOthersBlock(string key)
    {
        var kind = Kind(key);
        var plan = Plan(kind);
        var attackers = plan.Globals.Where(entry => entry.Global == "TowerZombieAttackDevice").Select(entry => entry.Entry).ToList();

        Assert.Equal(kind.WalkThrough, Field(plan, Table.Tower, "IsWalkThrough"));

        if (kind.WalkThrough) Assert.Equal(DefenseRows.WalkThroughAttackers.Select(zombie => $"{zombie}:{kind.Ids.Furniture}:1"), attackers);
        else Assert.Empty(attackers);
    }

    // The game reads these lists by the defense id in a set field; an entry under any other key is never found.
    [Theory]
    [MemberData(nameof(Keys))]
    public void EveryGlobalEntryNamesTheDefenseWhereTheGameLooks(string key)
    {
        var kind = Kind(key);

        foreach (var entry in Plan(kind).Globals)
        {
            Assert.True(DefenseRows.Globals.TryGetValue(entry.Global, out var keyIndex), entry.Global);
            Assert.Equal(kind.Ids.Furniture.ToString(CultureInfo.InvariantCulture), entry.Entry.Split(':')[keyIndex]);
        }
    }

    [Fact]
    public void TheRepairAndSiegeSettingsAreWrittenTheWayTheGameWritesItsOwn()
    {
        var kind = DefenseRows.Barrier;
        var globals = Plan(kind, kind.Defaults with { RepairCostRatio = 0.45f, RepairPerPart = 320, SiegePoints = 4 }).Globals;

        Assert.Contains(new GlobalEntry("TowerDeviceRepairCostRatio", "930001:0.45"), globals);
        Assert.Contains(new GlobalEntry("TowerDeviceRepairHpPerUnit", "930001:320"), globals);
        Assert.Contains(new GlobalEntry("SiegeElastic_DevPts", "930001:4"), globals);
    }

    [Fact]
    public void ATurretShutsDownOnlyWhenItsSettingSaysSo()
    {
        var kind = DefenseRows.Turret;

        Assert.Contains(new GlobalEntry("TowerDeviceDisableThreshold", "930004:0.5"), Plan(kind).Globals);
        Assert.DoesNotContain(Plan(kind, kind.Defaults with { ShutsDownBelow = 0f }).Globals, entry => entry.Global == "TowerDeviceDisableThreshold");
    }

    [Fact]
    public void APoweredDefenseSetToNoPowerIsNotElectrical()
    {
        var plan = Plan(DefenseRows.Turret, DefenseRows.Turret.Defaults with { PowerCost = 0 });

        Assert.Equal(false, Field(plan, Table.Furniture, "IsElectrical"));
        Assert.Equal(0, Field(plan, Table.Furniture, "ElectricalType"));
    }

    // Our own rows are free (empty, or ours from an earlier write); everything else is the game's.
    private static Func<Table, int, Slot> Free(DefensePlan plan, Table? stockAt = null, int? missing = null) => (table, id) =>
        table == stockAt && plan.Rows.Any(row => row.Table == table && row.Id == id) ? Slot.Stock
        : missing == id && plan.Rows.Any(row => row.Table == table && row.CopyFrom == id) ? Slot.Empty
        : plan.Rows.Any(row => row.Table == table && row.Id == id) ? Slot.Empty : Slot.Stock;

    [Fact]
    public void ADefenseWithAllItsRowsFreeHasNoProblems()
    {
        var plan = Plan(DefenseRows.Wire);

        Assert.Empty(DefenseRows.Problems(plan, Free(plan), _ => null));
    }

    [Fact]
    public void AStockRowAtOneOfOurIdsIsNeverReplaced()
    {
        var plan = Plan(DefenseRows.Wire);
        var problems = DefenseRows.Problems(plan, Free(plan, stockAt: Table.Tower), _ => null);

        Assert.Contains(problems, reason => reason.Contains("Tower 930002 is already used by the game"));
    }

    [Fact]
    public void AMissingRowToCopyStopsTheWholeDefense()
    {
        var plan = Plan(DefenseRows.Wire);
        var problems = DefenseRows.Problems(plan, Free(plan, missing: DefenseRows.SpikeBarrier.Furniture), _ => null);

        Assert.Contains(problems, reason => reason.Contains("Tower 36002, the row to copy, is missing"));
    }

    [Fact]
    public void ARecipeWithTheSameMaterialsStopsTheDefense()
    {
        var plan = Plan(DefenseRows.Wire);

        Assert.Contains(DefenseRows.Problems(plan, Free(plan), _ => 401), reason => reason.Contains("Recipe 401"));
    }

    // The game's own text and the mod's must match in both languages, or one language shows a raw key.
    [Theory]
    [MemberData(nameof(Keys))]
    public void EveryTextTheRowsNameExistsInBothLanguages(string key)
    {
        var kind = Kind(key);
        var plan = Plan(kind);
        var named = plan.Rows.SelectMany(row => row.Fields).Where(field => field.Value is string text && text.StartsWith("DefensesExpanded_", StringComparison.Ordinal)).Select(field => (string)field.Value).Distinct();

        foreach (var name in named)
        {
            var text = plan.Texts.Single(t => t.Key == name);

            Assert.False(string.IsNullOrWhiteSpace(text.En));
            Assert.False(string.IsNullOrWhiteSpace(text.Zh));
        }
    }

    [Theory]
    [InlineData(0.5f, "0.5")]
    [InlineData(0.65f, "0.65")]
    [InlineData(2f, "2")]
    public void NumbersAreWrittenWithoutTrailingZeros(float value, string text) => Assert.Equal(text, DefenseRows.Number(value));
}
