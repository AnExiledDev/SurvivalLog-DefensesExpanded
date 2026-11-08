using System;
using System.Collections.Generic;
using System.Linq;
using DefensesExpanded.Core;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppDictionary = Il2CppSystem.Collections.Generic;
using Il2CppInts = Il2CppSystem.Collections.Generic.List<int>;

namespace DefensesExpanded.Adapters;

/// <summary>
/// Writes the defenses' rows and text into the game's tables once they are loaded, adds their entries to the game
/// settings that list defenses by id, then rebuilds the game's lookups that were made from them. It writes again when
/// the game loads a table afresh or a setting changes, over its own rows where they already are, and then brings the
/// defenses already standing in a run up to the new settings, and puts each recipe at its defense skill level. Every few
/// seconds it also shows the defenses' recipes to a player who was already shown any stock defense recipe.
/// </summary>
internal sealed class DefenseTables
{
    private const float CheckSeconds = 1f;
    private const float RevealSeconds = 5f;

    // The six row tables, the game settings, the two languages and the defense skill levels.
    private const int Watched = 10;

    private readonly Action<string> _log;
    private readonly IntPtr[] _seen = new IntPtr[Watched];
    private readonly IntPtr[] _applied = new IntPtr[Watched];
    private readonly List<Reveal> _reveals = new();
    private HashSet<IntPtr> _ours = new();
    private IReadOnlyList<DefenseSettings>? _appliedSettings;
    private float _nextCheckAt;
    private float _nextRevealAt;

    public DefenseTables(Action<string> log)
    {
        _log = log;
    }

    public void Tick(float now)
    {
        if (now >= _nextRevealAt)
        {
            _nextRevealAt = now + RevealSeconds;
            ShowRecipes();
        }

        if (now < _nextCheckAt) return;

        _nextCheckAt = now + CheckSeconds;

        var config = GameSingleton.Existing<ConfigManager>();
        var english = config?.customCache?.EnglishLanguageMap;
        var chinese = config?.customCache?.ChineseLanguageMap;

        if (config == null || english == null || chinese == null) return;

        var tables = new Tables(config);

        if (!tables.HasRows) return;

        tables.Pointers(_seen, english, chinese);

        var settings = CurrentSettings.Current;

        if (ReferenceEquals(settings, _appliedSettings) && _seen.AsSpan().SequenceEqual(_applied)) return;

        var previous = _appliedSettings;
        var isChange = previous != null && !ReferenceEquals(settings, previous);

        _seen.CopyTo(_applied, 0);
        _appliedSettings = settings;

        var written = Write(config, tables, english, chinese, settings);

        // A defense placed in this run took its numbers when it was built; a change brings it up to the new ones.
        if (isChange && written.Count > 0) Step("defenses already placed", () => RefreshPlaced(config, written, previous!, settings));
    }

    private List<DefenseKind> Write(ConfigManager config, Tables tables, Il2CppDictionary.Dictionary<string, string> english, Il2CppDictionary.Dictionary<string, string> chinese, IReadOnlyList<DefenseSettings> settings)
    {
        var views = new Dictionary<Table, ITableView>
        {
            [Table.Item] = new TableView<Config_Item>(tables.Items!, () => new Config_Item()),
            [Table.Furniture] = new TableView<Config_Furniture>(tables.Furniture!, () => new Config_Furniture()),
            [Table.Recipe] = new TableView<Config_ProductionList>(tables.Recipes!, () => new Config_ProductionList()),
            [Table.Action] = new TableView<Config_Action>(tables.Actions!, () => new Config_Action()),
            [Table.Effect] = new TableView<Config_Effect>(tables.Effects!, () => new Config_Effect()),
            [Table.Tower] = new TableView<Config_TowerDevice>(tables.Towers!, () => new Config_TowerDevice()),
        };

        // Our rows from the last write, where they still are: a table loaded afresh has none of them.
        var before = _ours;

        _ours = new HashSet<IntPtr>();

        foreach (var kind in DefenseRows.All)
        foreach (var (table, id) in RowsOf(kind.Ids))
            views[table].Keep(id, before, _ours);

        var written = new List<(DefenseKind Kind, DefenseSettings Settings, int[] Materials, IReadOnlyList<GlobalEntry> Globals)>();
        var items = tables.Items!;
        var recipes = tables.Recipes!;

        for (var i = 0; i < DefenseRows.All.Count; i++)
        {
            var kind = DefenseRows.All[i];
            var wanted = settings[i];

            // The earlier defenses' materials as written, and the later ones' defaults, which each may fall back to.
            var taken = written.Select(done => (IReadOnlyList<int>)done.Materials).Concat(DefenseRows.All.Skip(i + 1).Select(later => (IReadOnlyList<int>)MaterialList.Standard(later.Defaults.Materials))).ToList();
            var (materials, problem) = MaterialList.Choose(wanted.Materials, kind.Defaults.Materials, items.ContainsKey, list => RecipeUsing(recipes, list), taken);

            if (problem != null) _log($"{kind.Name}: {problem}, so it takes its default materials");

            var plan = DefenseRows.Plan(kind, materials, wanted);
            var problems = DefenseRows.Problems(plan, (table, id) => views[table].SlotAt(id, _ours), list => RecipeUsing(recipes, list));

            if (problems.Count > 0)
            {
                foreach (var reason in problems) _log($"cannot add the {kind.Name}: {reason}");

                continue;
            }

            SetTexts(plan.Texts, english, chinese);

            var refused = new List<string>();

            foreach (var row in plan.Rows) refused.AddRange(views[row.Table].Write(row, _ours));

            foreach (var reason in refused) _log($"{kind.Name} field not set: {reason}");

            written.Add((kind, wanted, materials, plan.Globals));
        }

        // Every list is rewritten, so a defense that could not be added this time leaves no entries behind.
        Step("game settings", () => SetGlobals(tables.Globals!, written.SelectMany(done => done.Globals).ToList()));
        Step("defense skill levels", () => SetUnlockLevels(tables.DefenseLevels, written.Select(done => (done.Kind, done.Settings)).ToList()));

        if (written.Count == 0) return new List<DefenseKind>();

        // The recipe lookups are built once from the recipe table, and the "where to get it" lookups rebuild themselves
        // when this one is empty; the defenses' own recipe lookup rebuilds when it is gone.
        config.InitProductionListMap();
        config._furnitureToPackageItems = null;
        config._towerDeviceRecipe = null;
        config._towerDeviceRecipeSourceDict = null;

        var kinds = written.Select(done => done.Kind).ToList();

        KeepReveals(recipes, kinds);

        foreach (var (kind, wanted, materials, _) in written)
        {
            var ids = kind.Ids;
            var used = kind.Used(wanted);

            _log($"{kind.Name} added: item {ids.Item}, furniture {ids.Furniture}, recipe {ids.Recipe}; health {used.Health}, damage {used.Damage} every {used.AttackInterval}s in {used.AttackRange} m, power {used.PowerCost}, materials {MaterialText(materials)}");
        }

        return kinds;
    }

    /// <summary>
    /// Puts our entries in each game setting that lists defenses, after taking out any we wrote before. The game reads
    /// each again when its text changes, so a setting is only replaced when it comes out different.
    /// </summary>
    private void SetGlobals(Il2CppDictionary.Dictionary<string, Config_GlobalSetting> globals, IReadOnlyList<GlobalEntry> entries)
    {
        var ours = DefenseRows.All.Select(kind => kind.Ids.Furniture).ToArray();

        foreach (var (name, keyIndex) in DefenseRows.Globals)
        {
            var mine = entries.Where(entry => entry.Global == name).Select(entry => entry.Entry).ToList();
            var row = globals.ContainsKey(name) ? globals[name] : null;

            if (row == null)
            {
                if (mine.Count == 0) continue;

                row = new Config_GlobalSetting { ID = name, Params3 = "" };
                globals[name] = row;
                _log($"the game has no {name} setting, so the mod adds it");
            }

            var merged = GlobalLists.Merge(row.Params3, keyIndex, ours, mine);

            if (merged != (row.Params3 ?? "")) row.Params3 = merged;
        }
    }

    /// <summary>
    /// Gives the placed defenses of these kinds the numbers in their rows now: their fighting numbers, their health (in
    /// the same share of the new most as before), and a place among the game's shooters for one that now attacks.
    /// </summary>
    private void RefreshPlaced(ConfigManager config, IReadOnlyList<DefenseKind> kinds, IReadOnlyList<DefenseSettings> before, IReadOnlyList<DefenseSettings> now)
    {
        var health = new Dictionary<int, (int Old, int New)>();

        for (var i = 0; i < DefenseRows.All.Count && i < before.Count && i < now.Count; i++)
            health[DefenseRows.All[i].Ids.Furniture] = (before[i].Health, now[i].Health);

        var world = GameSingleton.Existing<BattleLogicWorld>();
        var furnitures = world?._AgentManager?.GetAllFurnitures();

        if (world == null || furnitures == null) return;

        var ids = kinds.Select(kind => kind.Ids.Furniture).ToHashSet();
        var count = 0;

        for (var i = 0; i < furnitures.Count; i++)
        {
            var furniture = furnitures[i];

            if (furniture == null || !ids.Contains(furniture.AgentConfigId)) continue;

            var id = furniture.AgentConfigId;

            if (AgentTools.GetAgentComponent<TowerDeviceComponent>(furniture) is { } device)
            {
                var timer = device.AttackTimerSeconds;

                device.OnInit(id);
                device.AttackTimerSeconds = timer;
            }

            // Only a change of the Health setting moves its health, by that setting's own ratio: the most it has now
            // can carry a talent's bonus over the row, which the game adds back but does not refill.
            if (health.TryGetValue(id, out var change) && change.Old != change.New && AgentTools.GetAgentComponent<FurnitureDurabilityComponent>(furniture) is { } durability)
            {
                var (max, current) = HealthScale.Rescale(durability.MaxDurability, durability.CurrentDurability, change.Old, change.New);

                durability.SetMaxDurability(max);
                durability.SetCurrentDurability(current);
            }

            count++;
        }

        if (count == 0) return;

        // The game lists its shooters, walls and walk-over defenses once and lists them again when this is cleared.
        if (world._TowerCombatManager is { } combat) combat.deviceScanDone = false;

        // The game keeps its list of powered furniture until something is placed or removed, and only that list draws
        // power, so a defense whose power cost went from 0 to more (or back) would not join or leave it until a reload.
        if (PowerCostChanged(before, now))
        {
            world._AgentManager?.InvalidateHomeElecCache();
            world._PowerManager?.ForceUpdatePanel();
        }

        _log($"{count} placed defenses brought up to the new settings");
    }

    private static bool PowerCostChanged(IReadOnlyList<DefenseSettings> before, IReadOnlyList<DefenseSettings> now)
    {
        for (var i = 0; i < before.Count && i < now.Count; i++)
            if (before[i].PowerCost != now[i].PowerCost) return true;

        return false;
    }

    /// <summary>
    /// Puts each defense's recipe in the defense skill level lists at the level its setting asks for, after taking out
    /// every recipe of ours from every list. The lists are changed in place, since the game reads them on every check.
    /// </summary>
    private void SetUnlockLevels(Il2CppDictionary.Dictionary<int, Config_DefenseLv>? levels, IReadOnlyList<(DefenseKind Kind, DefenseSettings Settings)> written)
    {
        if (levels == null || levels.Count == 0)
        {
            _log("the game has no defense skill levels, so the defenses unlock with no level");

            return;
        }

        var ours = DefenseRows.All.Select(kind => kind.Ids.Recipe).ToHashSet();
        var unlockAt = written.ToDictionary(done => done.Kind.Ids.Recipe, done => done.Settings.UnlockLevel);

        foreach (var pair in levels)
        {
            var row = pair.Value;

            if (row == null) continue;

            Rewrite(row.unlock_recipes, false);
            Rewrite(row.unlock_recipes_P1, true);
            Rewrite(row.unlock_recipes_P2, true);
            Rewrite(row.unlock_recipes_P3, true);
            Rewrite(row.unlock_recipes_P4, true);

            void Rewrite(Il2CppInts? list, bool roleList)
            {
                if (list == null) return;

                var now = UnlockLevels.Rebuild(ToArray(list), roleList, row.Lv, ours, unlockAt);

                if (now.SequenceEqual(ToArray(list))) return;

                list.Clear();

                foreach (var id in now) list.Add(id);
            }
        }

        foreach (var (kind, settings) in written)
            if (settings.UnlockLevel > 0 && !levels.ContainsKey(settings.UnlockLevel)) _log($"the defense skill has no level {settings.UnlockLevel}, so the {kind.Name} needs no level");
    }

    private void KeepReveals(Il2CppDictionary.Dictionary<int, Config_ProductionList> recipes, IReadOnlyList<DefenseKind> kinds)
    {
        _reveals.Clear();

        var stock = DefenseRows.StockRecipes.Where(recipes.ContainsKey).Select(id => recipes[id]).ToList();

        if (stock.Count == 0) return;

        foreach (var kind in kinds)
            if (recipes.ContainsKey(kind.Ids.Recipe)) _reveals.Add(new Reveal(kind, recipes[kind.Ids.Recipe], stock));
    }

    /// <summary>
    /// Shows the defenses' recipes to a player who has any stock defense recipe: a run that was past it before the mod
    /// was added, or a run whose materials setting changed, since the game remembers a recipe by its materials. The
    /// defense skill level still locks each one until the player reaches it.
    /// </summary>
    private void ShowRecipes()
    {
        if (_reveals.Count == 0) return;

        try
        {
            var tool = GameSingleton.Existing<BattleLogicWorld>()?._ToolTableManager;

            if (tool == null) return;

            foreach (var reveal in _reveals)
            {
                if (tool.IsProductionRevealedByRecord(reveal.Ours) || !AnyRevealed(tool, reveal.ComesWith)) continue;

                var ids = new Il2CppInts();

                ids.Add(reveal.Kind.Ids.Recipe);
                tool.AddProductionRecords(ids);
                _log($"{reveal.Kind.Name} recipe shown, since a stock defense recipe already is");
            }
        }
        catch (Exception ex)
        {
            _reveals.Clear();
            _log($"cannot show the defenses' recipes in a run already going, so they wait for the stock ones: {ex}");
        }
    }

    private static bool AnyRevealed(ToolTableManager tool, IReadOnlyList<Config_ProductionList> recipes)
    {
        for (var i = 0; i < recipes.Count; i++)
            if (tool.IsProductionRevealedByRecord(recipes[i])) return true;

        return false;
    }

    private void Step(string what, Action step)
    {
        try
        {
            step();
        }
        catch (Exception ex)
        {
            _log($"the {what} could not be changed: {ex}");
        }
    }

    private int? RecipeUsing(Il2CppDictionary.Dictionary<int, Config_ProductionList> recipes, IReadOnlyList<int> materials)
    {
        var key = DefenseRows.MaterialKey(materials);

        foreach (var pair in recipes)
        {
            var recipe = pair.Value;

            if (recipe == null || _ours.Contains(recipe.Pointer) || recipe.MaterialList == null || recipe.MaterialList.Count != materials.Count) continue;

            if (DefenseRows.MaterialKey(ToArray(recipe.MaterialList)) == key) return pair.Key;
        }

        return null;
    }

    private static void SetTexts(IEnumerable<Text> texts, Il2CppDictionary.Dictionary<string, string> english, Il2CppDictionary.Dictionary<string, string> chinese)
    {
        foreach (var text in texts)
        {
            english[text.Key] = text.En;
            chinese[text.Key] = text.Zh;
        }
    }

    private static string MaterialText(IEnumerable<int> materials) => string.Join(", ", materials.GroupBy(id => id).Select(group => $"{group.Key}:{group.Count()}"));

    private static (Table Table, int Id)[] RowsOf(RowIds ids) => new[]
    {
        (Table.Item, ids.Item),
        (Table.Furniture, ids.Furniture),
        (Table.Recipe, ids.Recipe),
        (Table.Action, ids.Action),
        (Table.Effect, ids.Effect),
        (Table.Tower, ids.Furniture),
    };

    private static int[] ToArray(Il2CppInts? list)
    {
        if (list == null) return Array.Empty<int>();

        var values = new int[list.Count];

        for (var i = 0; i < values.Length; i++) values[i] = list[i];

        return values;
    }

    private sealed record Reveal(DefenseKind Kind, Config_ProductionList Ours, IReadOnlyList<Config_ProductionList> ComesWith);

    /// <summary>The tables the mod writes, read from the game once a check.</summary>
    private sealed class Tables
    {
        public Tables(ConfigManager config)
        {
            Items = config._Config_Item_Dict;
            Furniture = config._Config_Furniture_Dict;
            Recipes = config._Config_ProductionList_Dict;
            Actions = config._Config_Action_Dict;
            Effects = config._Config_Effect_Dict;
            Towers = config._Config_TowerDevice_Dict;
            Globals = config._Config_GlobalSetting_Dict;
            DefenseLevels = config._Config_DefenseLv_Dict;
        }

        public Il2CppDictionary.Dictionary<int, Config_Item>? Items { get; }
        public Il2CppDictionary.Dictionary<int, Config_Furniture>? Furniture { get; }
        public Il2CppDictionary.Dictionary<int, Config_ProductionList>? Recipes { get; }
        public Il2CppDictionary.Dictionary<int, Config_Action>? Actions { get; }
        public Il2CppDictionary.Dictionary<int, Config_Effect>? Effects { get; }
        public Il2CppDictionary.Dictionary<int, Config_TowerDevice>? Towers { get; }
        public Il2CppDictionary.Dictionary<string, Config_GlobalSetting>? Globals { get; }
        public Il2CppDictionary.Dictionary<int, Config_DefenseLv>? DefenseLevels { get; }

        /// <summary>Whether every table the defenses need is loaded.</summary>
        public bool HasRows => IsLoaded(Items) && IsLoaded(Furniture) && IsLoaded(Recipes) && IsLoaded(Actions) && IsLoaded(Effects) && IsLoaded(Towers) && Globals != null && Globals.Count > 0;

        public void Pointers(IntPtr[] into, Il2CppObjectBase english, Il2CppObjectBase chinese)
        {
            into[0] = Pointer(Items);
            into[1] = Pointer(Furniture);
            into[2] = Pointer(Recipes);
            into[3] = Pointer(Actions);
            into[4] = Pointer(Effects);
            into[5] = Pointer(Towers);
            into[6] = Pointer(Globals);
            into[7] = english.Pointer;
            into[8] = chinese.Pointer;
            into[9] = Pointer(DefenseLevels);
        }

        private static bool IsLoaded<T>(Il2CppDictionary.Dictionary<int, T>? table) => table != null && table.Count > 0;

        private static IntPtr Pointer(Il2CppObjectBase? table) => table?.Pointer ?? IntPtr.Zero;
    }

    private interface ITableView
    {
        Slot SlotAt(int id, HashSet<IntPtr> ours);

        /// <summary>Carries the row at <paramref name="id"/> into <paramref name="next"/> when it was one of ours.</summary>
        void Keep(int id, HashSet<IntPtr> before, HashSet<IntPtr> next);

        /// <summary>Writes the row, over our own row at its id when there is one, and returns the fields that could not be set.</summary>
        List<string> Write(RowPlan plan, HashSet<IntPtr> ours);
    }

    private sealed class TableView<T> : ITableView where T : Il2CppObjectBase
    {
        private readonly Il2CppDictionary.Dictionary<int, T> _table;
        private readonly Func<T> _make;

        public TableView(Il2CppDictionary.Dictionary<int, T> table, Func<T> make)
        {
            _table = table;
            _make = make;
        }

        public Slot SlotAt(int id, HashSet<IntPtr> ours)
        {
            if (!_table.ContainsKey(id)) return Slot.Empty;

            return ours.Contains(_table[id].Pointer) ? Slot.Ours : Slot.Stock;
        }

        public void Keep(int id, HashSet<IntPtr> before, HashSet<IntPtr> next)
        {
            if (_table.ContainsKey(id) && _table[id] is { } row && before.Contains(row.Pointer)) next.Add(row.Pointer);
        }

        public List<string> Write(RowPlan plan, HashSet<IntPtr> ours)
        {
            // Writing over our own row keeps every reference the game already holds to it up to date.
            var row = SlotAt(plan.Id, ours) == Slot.Ours ? _table[plan.Id] : _make();

            RowFields.CopyAll(_table[plan.CopyFrom], row);

            var refused = plan.Fields.Select(field => RowFields.Set(row, field.Name, field.Value)).OfType<string>().ToList();

            _table[plan.Id] = row;
            ours.Add(row.Pointer);

            return refused;
        }
    }
}
