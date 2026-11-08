using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DefensesExpanded.Core;

public enum Table
{
    Item,
    Furniture,
    Recipe,
    Action,
    Effect,
    Tower,
}

/// <summary>One field to set on a row, by the name the table gives it: an int, a float, a string, a bool or an int[] list.</summary>
public sealed record RowField(string Name, object Value);

/// <summary>A row to add: a copy of the stock row <paramref name="CopyFrom"/> under <paramref name="Id"/>, with these fields changed.</summary>
public sealed record RowPlan(Table Table, int Id, int CopyFrom, IReadOnlyList<RowField> Fields);

public sealed record Text(string Key, string En, string Zh);

/// <summary>
/// One entry of a game setting that lists defenses (Config_GlobalSetting's Params3, entries split by ';' and fields by
/// ':'), such as "36001:200" in the repair list.
/// </summary>
public sealed record GlobalEntry(string Global, string Entry);

public sealed record DefensePlan(IReadOnlyList<RowPlan> Rows, IReadOnlyList<Text> Texts, IReadOnlyList<GlobalEntry> Globals);

/// <summary>What the game's tables hold at one id, as far as writing our rows is concerned.</summary>
public enum Slot
{
    Empty,
    Ours,
    Stock,
}

/// <summary>
/// The five defenses and the table rows that make each one: the recipe, the package it crafts, the unpack action and
/// its effect, the furniture and its tower row, all copies of the stock defense it stands next to, plus its entries in
/// the game settings that list defenses by id (repair, shut-down, siege size, which zombies go for it).
/// </summary>
public static class DefenseRows
{
    public const string WebIconFolder = "Res/Mods/DefensesExpanded";

    /// <summary>The game's sandbags, spike barrier, electric trap and mechanical chainsaw (game 1.1.20151).</summary>
    public static readonly RowIds Sandbags = new(Item: 26001, Furniture: 36001, Recipe: 400, Action: 100026001, Effect: 100026001);
    public static readonly RowIds SpikeBarrier = new(Item: 26002, Furniture: 36002, Recipe: 401, Action: 100026002, Effect: 100026002);
    public static readonly RowIds ElectricFence = new(Item: 26003, Furniture: 36003, Recipe: 402, Action: 100026003, Effect: 100026003);
    public static readonly RowIds Chainsaw = new(Item: 26004, Furniture: 36004, Recipe: 403, Action: 100026004, Effect: 100026004);

    /// <summary>The stock defense recipes: a player shown any of them is shown ours, and the skill level locks them.</summary>
    public static readonly IReadOnlyList<int> StockRecipes = new[] { Sandbags.Recipe, SpikeBarrier.Recipe, ElectricFence.Recipe, Chainsaw.Recipe };

    /// <summary>The defense skill's highest level (1.1.20719).</summary>
    public const int TopLevel = 5;

    /// <summary>The settings the defenses' entries go in, and which field of an entry is the defense id.</summary>
    public static readonly IReadOnlyDictionary<string, int> Globals = new Dictionary<string, int>
    {
        ["TowerZombieAttackDevice"] = 1,
        ["TowerZombieDodgeVsDevice"] = 1,
        ["TowerDeviceDisableThreshold"] = 0,
        ["TowerDeviceRepairCostRatio"] = 0,
        ["TowerDeviceRepairHpPerUnit"] = 0,
        ["TowerDeviceWreckDamage"] = 0,
        ["SiegeElastic_DevPts"] = 0,
    };

    /// <summary>The zombies that also go for a defense they could walk over, as the game lists them for its chainsaw.</summary>
    public static readonly IReadOnlyList<int> WalkThroughAttackers = new[] { 9109, 9110 };

    public static readonly DefenseKind Barrier = new()
    {
        Name = "Concrete Barrier",
        Key = "Barrier",
        // Far above the game's own ids (1.1.20151: item 70152, furniture 90291, recipe 1703), and clear of Wind Power's
        // 910000 and Hydroponics Expanded's 920000 ranges.
        Ids = new RowIds(Item: 930001, Furniture: 930001, Recipe: 9301, Action: 100930001, Effect: 100930001),
        Stock = Sandbags,
        ResName = "Mod_Defense_Barrier",
        StockPrefab = "P_Tripo01",
        ModelFile = "ConcreteBarrier.glb",
        IconFile = "UI_Item_Icon_ConcreteBarrier.png",
        SaveMarker = "DefensesExpanded.Defense.Barrier:",
        AttackPriority = 1,
        // Two and a half times the sandbags' 600, and its repairs go further for it.
        Defaults = new DefenseSettings { Health = 1500, Materials = "20004:4, 20361:1", RepairPerPart = 300, RepairCostRatio = 0.5f, SiegePoints = 2, UnlockLevel = 2 },
        Texts = Describe("Barrier", "Concrete Barrier", "混凝土路障",
            "A heavy barrier of cast concrete. It blocks zombies far longer than sandbags.",
            "浇筑混凝土制成的厚重路障，比沙包更能长久挡住丧尸。"),
    };

    public static readonly DefenseKind Wire = new()
    {
        Name = "Barbed Wire",
        Key = "Wire",
        Ids = new RowIds(Item: 930002, Furniture: 930002, Recipe: 9302, Action: 100930002, Effect: 100930002),
        Stock = SpikeBarrier,
        ResName = "Mod_Defense_Wire",
        StockPrefab = "P_Tripo02",
        ModelFile = "BarbedWire.glb",
        IconFile = "UI_Item_Icon_BarbedWire.png",
        SaveMarker = "DefensesExpanded.Defense.Wire:",
        WalkThrough = true,
        Attacks = true,
        Slows = true,
        Defaults = new DefenseSettings
        {
            Health = 250, Materials = "20004:3, 20104:2", RepairPerPart = 50, RepairCostRatio = 0.65f, SiegePoints = 3, UnlockLevel = 2,
            Damage = 1, AttackInterval = 2f, AttackRange = 1.2f, MaxTargets = 6, SlowPercent = 40, SlowSeconds = 4f,
        },
        Texts = Describe("Wire", "Barbed Wire", "铁丝网",
            "Coils of barbed wire laid across the way. Zombies can push through it, but it slows them down and cuts them as they go.",
            "横在路上的带刺铁丝。丧尸能挤过去，但会被拖慢脚步，一路被割伤。"),
    };

    public static readonly DefenseKind Spikes = new()
    {
        Name = "Spike Plate",
        Key = "Spikes",
        Ids = new RowIds(Item: 930003, Furniture: 930003, Recipe: 9303, Action: 100930003, Effect: 100930003),
        Stock = SpikeBarrier,
        ResName = "Mod_Defense_Spikes",
        StockPrefab = "P_Tripo02",
        ModelFile = "SpikePlate.glb",
        IconFile = "UI_Item_Icon_SpikePlate.png",
        SaveMarker = "DefensesExpanded.Defense.Spikes:",
        WalkThrough = true,
        Attacks = true,
        Defaults = new DefenseSettings
        {
            Health = 300, Materials = "20004:2, 20361:1", RepairPerPart = 60, RepairCostRatio = 0.65f, SiegePoints = 3, UnlockLevel = 2,
            Damage = 3, AttackInterval = 2f, AttackRange = 0.8f, MaxTargets = 6,
        },
        Texts = Describe("Spikes", "Spike Plate", "钉板",
            "A plank studded with spikes, laid flat on the ground. Zombies walk over it and are stabbed with every step.",
            "钉满尖刺的木板，平铺在地上。丧尸可以走过去，但每一步都会被扎伤。"),
    };

    public static readonly DefenseKind Turret = new()
    {
        Name = "Nail Gun Turret",
        Key = "Turret",
        Ids = new RowIds(Item: 930004, Furniture: 930004, Recipe: 9304, Action: 100930004, Effect: 100930004),
        Stock = Chainsaw,
        ResName = "Mod_Defense_Turret",
        StockPrefab = "P_Tripo04",
        ModelFile = "NailGunTurret.glb",
        ModelYaw = 180f,
        IconFile = "UI_Item_Icon_NailGunTurret.png",
        SaveMarker = "DefensesExpanded.Defense.Turret:",
        AttackPriority = 1,
        Attacks = true,
        Powered = true,
        Defaults = new DefenseSettings
        {
            Health = 250, Materials = "20210:1, 20361:1, 20004:3", RepairPerPart = 40, RepairCostRatio = 0.8f, SiegePoints = 6, UnlockLevel = 3,
            Damage = 8, AttackInterval = 1.5f, AttackRange = 4f, MaxTargets = 1, PowerCost = 4, PowerPerHit = 1f, ShutsDownBelow = 0.5f,
        },
        Texts = Describe("Turret", "Nail Gun Turret", "射钉炮塔",
            "A powered nail gun on a turning mount. It fires at zombies from a distance, one at a time.",
            "装在转台上的电动射钉枪，会从远处一个接一个地射击丧尸。"),
    };

    public static readonly DefenseKind Tesla = new()
    {
        Name = "Tesla Coil",
        Key = "Tesla",
        Ids = new RowIds(Item: 930005, Furniture: 930005, Recipe: 9305, Action: 100930005, Effect: 100930005),
        Stock = ElectricFence,
        ResName = "Mod_Defense_Tesla",
        StockPrefab = "P_Tripo03",
        ModelFile = "TeslaCoil.glb",
        ModelYaw = 180f,
        IconFile = "UI_Item_Icon_TeslaCoil.png",
        SaveMarker = "DefensesExpanded.Defense.Tesla:",
        AttackPriority = 1,
        Attacks = true,
        Slows = true,
        Powered = true,
        Defaults = new DefenseSettings
        {
            Health = 250, Materials = "20210:2, 20104:3, 20004:2", RepairPerPart = 40, RepairCostRatio = 0.65f, SiegePoints = 6, UnlockLevel = 5,
            Damage = 18, AttackInterval = 3f, AttackRange = 3f, MaxTargets = 8, SlowPercent = 20, SlowSeconds = 2f, PowerCost = 6, PowerPerHit = 2f,
        },
        Texts = Describe("Tesla", "Tesla Coil", "特斯拉线圈",
            "A powered coil that throws arcs of electricity into the zombies around it, shocking several at once and slowing them down.",
            "通电的线圈，向周围的丧尸放出电弧，一次电击好几只并减慢它们。"),
    };

    public static readonly IReadOnlyList<DefenseKind> All = new[] { Barrier, Wire, Spikes, Turret, Tesla };

    /// <summary>The kind whose furniture id this is, or null.</summary>
    public static DefenseKind? ByFurniture(int furnitureId)
    {
        foreach (var kind in All)
            if (kind.Ids.Furniture == furnitureId) return kind;

        return null;
    }

    /// <summary>The key the game files a recipe under: its materials, sorted.</summary>
    public static string MaterialKey(IEnumerable<int> materials) => string.Join(",", materials.OrderBy(id => id));

    /// <param name="materials">The recipe's materials, each id once per piece.</param>
    /// <param name="wanted">The settings to write; the parts the defense's role does not use are left out.</param>
    public static DefensePlan Plan(DefenseKind kind, IReadOnlyList<int> materials, DefenseSettings wanted)
    {
        var ids = kind.Ids;
        var settings = kind.Used(wanted);
        var package = kind.TextKey("Package");
        var name = kind.TextKey("Name");
        var des = kind.TextKey("Des");
        var unpack = kind.TextKey("Unpack");
        var unpacking = kind.TextKey("Unpacking");
        var isElectrical = settings.PowerCost > 0;

        string Zh(string key) => kind.Texts.First(text => text.Key == key).Zh;

        var rows = new[]
        {
            // The stock defense's recipe, so it sits next to it at the workbench and gives the same defense experience.
            new RowPlan(Table.Recipe, ids.Recipe, kind.Stock.Recipe, new[]
            {
                new RowField("ID", ids.Recipe),
                new RowField("ShopName", package),
                new RowField("ShopName_Local", Zh(package)),
                new RowField("ProductID", new[] { ids.Item }),
                new RowField("MaterialList", materials.ToArray()),
            }),
            new RowPlan(Table.Item, ids.Item, kind.Stock.Item, new[]
            {
                new RowField("ID", ids.Item),
                new RowField("ItemName", package),
                new RowField("ItemName_Local", Zh(package)),
                new RowField("TargetFurnitureID", ids.Furniture),
                new RowField("UseAction", ids.Action),
                new RowField("WebIcon", kind.WebIcon),
            }),
            new RowPlan(Table.Action, ids.Action, kind.Stock.Action, new[]
            {
                new RowField("ID", ids.Action),
                new RowField("Name", unpack),
                new RowField("Name_Local", Zh(unpack)),
                new RowField("NameDoing", unpacking),
                new RowField("NameDoing_Local", Zh(unpacking)),
                new RowField("EffectConfigID", ids.Effect),
                new RowField("WebIcon", kind.WebIcon),
            }),
            new RowPlan(Table.Effect, ids.Effect, kind.Stock.Effect, new[]
            {
                new RowField("ID", ids.Effect),
            }),
            new RowPlan(Table.Furniture, ids.Furniture, kind.Stock.Furniture, new[]
            {
                new RowField("ID", ids.Furniture),
                new RowField("Name", name),
                new RowField("Name_Local", Zh(name)),
                new RowField("Des", des),
                new RowField("Des_Local", Zh(des)),
                new RowField("ResName", kind.ResName),
                new RowField("ICON", kind.WebIcon),
                new RowField("FurnitureHP", settings.Health),
                new RowField("FurnitureHPMax", settings.Health),
                new RowField("PowerCost", settings.PowerCost),
                new RowField("IsElectrical", isElectrical),
                new RowField("ElectricalType", isElectrical ? 1 : 0),
            }),
            new RowPlan(Table.Tower, ids.Furniture, kind.Stock.Furniture, new[]
            {
                new RowField("ID", ids.Furniture),
                new RowField("AttackPriority", kind.AttackPriority),
                new RowField("IsWalkThrough", kind.WalkThrough),
                // The spike barrier hurts what bites it; ours do their harm with their own attack instead.
                new RowField("CounterDamage", 0),
                new RowField("Attack", settings.Damage),
                new RowField("AttackInterval", settings.AttackInterval),
                new RowField("AttackRange", settings.AttackRange),
                new RowField("MaxHitTargets", settings.MaxTargets),
                new RowField("HitSlowRatio", settings.SlowPercent / 100f),
                new RowField("HitSlowSeconds", settings.SlowSeconds),
                new RowField("PowerCostPerHit", settings.PowerPerHit),
            }),
        };

        return new DefensePlan(rows, kind.Texts, GlobalsOf(kind, settings));
    }

    /// <summary>The defense's entries in the game settings that list defenses by id.</summary>
    public static List<GlobalEntry> GlobalsOf(DefenseKind kind, DefenseSettings settings)
    {
        var id = kind.Ids.Furniture;
        var entries = new List<GlobalEntry>
        {
            new("TowerDeviceRepairCostRatio", $"{id}:{Number(settings.RepairCostRatio)}"),
            new("TowerDeviceRepairHpPerUnit", $"{id}:{settings.RepairPerPart}"),
            new("SiegeElastic_DevPts", $"{id}:{settings.SiegePoints}"),
        };

        if (settings.ShutsDownBelow > 0f) entries.Add(new GlobalEntry("TowerDeviceDisableThreshold", $"{id}:{Number(settings.ShutsDownBelow)}"));

        if (kind.WalkThrough)
            foreach (var zombie in WalkThroughAttackers)
                entries.Add(new GlobalEntry("TowerZombieAttackDevice", $"{zombie}:{id}:1"));

        return entries;
    }

    /// <summary>
    /// Why the plan cannot be written, or nothing if it can. Writing is all or nothing: a defense missing its recipe or
    /// tower row is worse than no defense, and a stock row at one of our ids must never be replaced.
    /// </summary>
    /// <param name="recipeUsing">The id of another recipe made of these materials in any order, or null.</param>
    public static List<string> Problems(DefensePlan plan, Func<Table, int, Slot> slotAt, Func<IReadOnlyList<int>, int?> recipeUsing)
    {
        var problems = new List<string>();

        foreach (var row in plan.Rows)
        {
            if (slotAt(row.Table, row.CopyFrom) == Slot.Empty) problems.Add($"{row.Table} {row.CopyFrom}, the row to copy, is missing");

            if (slotAt(row.Table, row.Id) == Slot.Stock) problems.Add($"{row.Table} {row.Id} is already used by the game");

            if (row.Fields.FirstOrDefault(field => field.Name == "MaterialList")?.Value is int[] materials && recipeUsing(materials) is int other)
                problems.Add($"Recipe {other} already uses the same materials, so only one of the two could be crafted");
        }

        return problems;
    }

    /// <summary>A number the way the game's settings write one: invariant, no trailing zeros.</summary>
    public static string Number(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static Text[] Describe(string key, string en, string zh, string desEn, string desZh) => new[]
    {
        new Text($"DefensesExpanded_{key}_Package", $"{en} Package", $"{zh}包裹"),
        new Text($"DefensesExpanded_{key}_Name", en, zh),
        new Text($"DefensesExpanded_{key}_Des", desEn, desZh),
        new Text($"DefensesExpanded_{key}_Unpack", $"Unwrap {en} Package", $"拆封 {zh}包裹"),
        new Text($"DefensesExpanded_{key}_Unpacking", $"Unwrapping {en} Package...", $"拆封 {zh}包裹中…"),
    };
}
