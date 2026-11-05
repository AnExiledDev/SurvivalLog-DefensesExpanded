using System;
using System.Collections.Generic;

namespace DefensesExpanded.Core;

/// <summary>The ids of one defense's rows: the recipe crafts the package item, whose unpack action and effect place the furniture, whose tower row says how it fights.</summary>
public sealed record RowIds(int Item, int Furniture, int Recipe, int Action, int Effect);

/// <summary>
/// What the player sets for one defense. Materials is written as id:count pairs, "20004:3, 20362:2". Each defense uses
/// only the settings its role has (see <see cref="DefenseKind"/>); the rest stay 0.
/// </summary>
public sealed record DefenseSettings
{
    public int Health { get; init; }
    public string Materials { get; init; } = "";

    /// <summary>How much health one repair part puts back.</summary>
    public int RepairPerPart { get; init; }

    /// <summary>The share of the materials a full repair costs, the way the game counts it for its own defenses.</summary>
    public float RepairCostRatio { get; init; }

    /// <summary>How much the defense counts toward the size of the next siege.</summary>
    public int SiegePoints { get; init; }

    public int Damage { get; init; }
    public float AttackInterval { get; init; }
    public float AttackRange { get; init; }
    public int MaxTargets { get; init; }

    /// <summary>How much slower a zombie it hits walks, from 0 to 90 percent.</summary>
    public int SlowPercent { get; init; }
    public float SlowSeconds { get; init; }

    public int PowerCost { get; init; }
    public float PowerPerHit { get; init; }

    /// <summary>The share of its health below which it stops working until repaired. 0 keeps it working until it breaks.</summary>
    public float ShutsDownBelow { get; init; }
}

/// <summary>
/// One defense the mod adds: the stock defense it copies (its rows, its prefab, and the ids a save writes in place of
/// ours), its own ids, model and text, what it does in a fight, and the settings it starts with.
/// </summary>
public sealed record DefenseKind
{
    /// <summary>The English name, for the log and the settings.</summary>
    public string Name { get; init; } = "";

    /// <summary>The middle of each text key and the settings section, DefensesExpanded_Barrier_Name.</summary>
    public string Key { get; init; } = "";

    public RowIds Ids { get; init; } = new(0, 0, 0, 0, 0);

    /// <summary>The stock defense the rows are copied from, and which a save names in place of ours.</summary>
    public RowIds Stock { get; init; } = new(0, 0, 0, 0, 0);

    /// <summary>The prefab name the furniture row gives. The game has no such prefab: the mod builds it.</summary>
    public string ResName { get; init; } = "";

    /// <summary>The stock defense's prefab the model is built on, so it keeps its states, collider and effects.</summary>
    public string StockPrefab { get; init; } = "";

    public string ModelFile { get; init; } = "";

    public string IconFile { get; init; } = "";

    /// <summary>The stock recipe that reveals this one: anyone shown it is shown this too.</summary>
    public int RevealedWith { get; init; }

    /// <summary>What the save's name field starts with while this defense is saved as its stock twin.</summary>
    public string SaveMarker { get; init; } = "";

    /// <summary>Zombies walk over it rather than stopping to break it.</summary>
    public bool WalkThrough { get; init; }

    /// <summary>Which defense zombies go for first, as the game ranks its own: lower first.</summary>
    public int AttackPriority { get; init; }

    /// <summary>It hurts the zombies in its range.</summary>
    public bool Attacks { get; init; }

    /// <summary>The zombies it hits walk slower for a while.</summary>
    public bool Slows { get; init; }

    /// <summary>It needs power to work.</summary>
    public bool Powered { get; init; }

    public DefenseSettings Defaults { get; init; } = new();

    /// <summary>The names and descriptions its rows point at.</summary>
    public IReadOnlyList<Text> Texts { get; init; } = Array.Empty<Text>();

    /// <summary>Where the web UI finds the icon, written the way the game writes its own (from a page two folders below WebUI).</summary>
    public string WebIcon => "../../" + DefenseRows.WebIconFolder + "/" + IconFile;

    public string TextKey(string part) => $"DefensesExpanded_{Key}_{part}";

    /// <summary>The settings with the parts its role does not use set to 0, so they never reach the game.</summary>
    public DefenseSettings Used(DefenseSettings settings) => settings with
    {
        Damage = Attacks ? settings.Damage : 0,
        AttackInterval = Attacks ? settings.AttackInterval : 0f,
        AttackRange = Attacks ? settings.AttackRange : 0f,
        MaxTargets = Attacks ? settings.MaxTargets : 0,
        SlowPercent = Slows ? settings.SlowPercent : 0,
        SlowSeconds = Slows ? settings.SlowSeconds : 0f,
        PowerCost = Powered ? settings.PowerCost : 0,
        PowerPerHit = Powered ? settings.PowerPerHit : 0f,
        ShutsDownBelow = Powered ? settings.ShutsDownBelow : 0f,
    };
}
