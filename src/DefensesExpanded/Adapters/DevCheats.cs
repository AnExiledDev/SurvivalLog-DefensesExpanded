using System.Collections.Generic;
using System.ComponentModel;
using DefensesExpanded.Core;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using Il2CppInts = Il2CppSystem.Collections.Generic.List<int>;

namespace DefensesExpanded.Adapters;

/// <summary>
/// Test buttons for Cheat Menu's Mod tests tab. Cheat Menu finds this class by its name, so neither mod needs the other.
/// Each method is one button: its DisplayName is the label and its Description the help, as "English\nChinese". What
/// it returns is shown in Cheat Menu's log.
/// </summary>
public static class DevCheats
{
    private const string NotInGame = "load a save first";
    private const int HoursPerDay = 24;

    // The game's own late story wave: every zombie type, from both spawn points.
    private const int TestWaveEvent = 40673;
    private const float TestWaveCountMultiplier = 4f;
    private const float TestWaveAliveCapMultiplier = 4f;

    [DisplayName("Give 1 of each defense\n给每种防御设施各 1 个")]
    [Description("Puts one Concrete Barrier, Barbed Wire, Spike Plate, Nail Gun Turret and Tesla Coil Package in your bag.\n在背包里放入混凝土路障、铁丝网、钉板、射钉炮塔和特斯拉线圈包裹各 1 个。")]
    public static string GiveOneOfEach()
    {
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var config = GameSingleton.Existing<ConfigManager>();
        var role = world?._AgentManager?.GetLeadingRole();
        var items = world?._ItemManager;

        if (config == null || role == null || items == null) return NotInGame;

        var hour = world!._GameTimeManager?.GetHour() ?? 0;
        var lines = new List<string>();

        foreach (var kind in DefenseRows.All)
        {
            var life = config.Get_Config_Item(kind.Ids.Item)?.Life ?? 0;
            var timeLeft = life <= 0 ? -1 : HoursPerDay * life;
            var isAdded = items.TryAddItemToBag(role.InstanceId, kind.Ids.Item, 1, hour, timeLeft, 0, 1f, null, 0, null, null, 0, 0, false, false, false, false, 0);

            lines.Add(isAdded ? $"gave 1 {kind.Name} Package" : $"could not give the {kind.Name} Package: the bag is full or too heavy");
        }

        return string.Join("\n", lines);
    }

    [DisplayName("Start a large test wave\n开始一波大型测试尸潮")]
    [Description("Starts a zombie wave now, four times the size of the game's late story wave, with every zombie type from both spawn points. The zombies have their plain health and attack, so the defenses' damage reads true. A wave already under way is ended first.\n立即开始一波尸潮，规模是游戏后期剧情尸潮的四倍，所有丧尸种类从两个出生点出现。丧尸保持原本的生命和攻击，方便看清防御设施的伤害。正在进行的尸潮会先结束。")]
    public static string StartLargeWave()
    {
        var waves = GameSingleton.Existing<BattleLogicWorld>()?._TowerWaveManager;

        if (waves == null) return NotInGame;
        if (waves.IsWaveOperationLocked()) return "the game is not taking waves right now";

        var rows = TowerWaveManager.FindStoryWaveRows(TestWaveEvent);

        if (rows == null || rows.Count == 0) return $"the game has no wave {TestWaveEvent}";

        var wave = TowerWaveManager.BuildWaveConfigFromRows(rows, TestWaveCountMultiplier, TestWaveAliveCapMultiplier);

        if (wave == null) return $"the game could not build wave {TestWaveEvent}";

        wave.HpMultiplier = 1f;
        wave.AttackMultiplier = 1;
        wave.AttackSpeedMultiplier = 1f;
        waves.GmStartWave(wave);

        return $"wave started: {wave.TotalBudget} zombie budget, up to {wave.PeakFieldCap} at once, one every {wave.SpawnIntervalSeconds:0.#}s";
    }

    [DisplayName("Show defenses\n查看防御设施")]
    [Description("Shows each new defense's numbers as the game holds them now: health, power, damage, how often and how far it strikes, how many zombies it hits, how much it slows them, whether its recipe is shown, and how many are placed here.\n显示游戏当前记录的每种新防御设施：耐久、耗电、伤害、攻击间隔和范围、可同时命中的丧尸数、减速效果、配方是否已显示，以及这里放了几个。")]
    public static string ShowDefenses()
    {
        var config = GameSingleton.Existing<ConfigManager>();
        var furniture = config?._Config_Furniture_Dict;
        var towers = config?._Config_TowerDevice_Dict;
        var recipes = config?._Config_ProductionList_Dict;
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var tool = world?._ToolTableManager;

        if (furniture == null || towers == null || recipes == null || tool == null) return NotInGame;

        var placed = new Dictionary<int, int>();
        var all = world!._AgentManager?.GetAllFurnitures();

        if (all != null)
            for (var i = 0; i < all.Count; i++)
                if (all[i] is { } one && DefenseRows.ByFurniture(one.AgentConfigId) != null)
                    placed[one.AgentConfigId] = placed.GetValueOrDefault(one.AgentConfigId) + 1;

        var lines = new List<string>();

        foreach (var kind in DefenseRows.All)
        {
            var ids = kind.Ids;

            if (!furniture.ContainsKey(ids.Furniture) || !towers.ContainsKey(ids.Furniture) || !recipes.ContainsKey(ids.Recipe))
            {
                lines.Add($"{kind.Name}: not in the game's tables yet (furniture {ids.Furniture}, recipe {ids.Recipe})");

                continue;
            }

            var row = furniture[ids.Furniture];
            var tower = towers[ids.Furniture];
            var shown = tool.IsProductionRevealedByRecord(recipes[ids.Recipe]);

            lines.Add($"{kind.Name}: health {row.FurnitureHPMax}, power {row.PowerCost}, damage {tower.Attack} every {tower.AttackInterval}s in {tower.AttackRange} m, up to {tower.MaxHitTargets} zombies, slow {tower.HitSlowRatio:0.##} for {tower.HitSlowSeconds}s, power per hit {tower.PowerCostPerHit}, walk over {tower.IsWalkThrough}, recipe {ids.Recipe} {(shown ? "shown" : "not shown yet")}, {placed.GetValueOrDefault(ids.Furniture)} placed here");
        }

        return string.Join("\n", lines);
    }

    [DisplayName("Show all recipes now\n立即显示所有配方")]
    [Description("Shows every new defense's recipe at the workbench now, without finding the stock defense recipes first.\n不必先获得原版防御设施的配方，立即在工作台显示所有新防御设施的配方。")]
    public static string ShowAllRecipes()
    {
        var tool = GameSingleton.Existing<BattleLogicWorld>()?._ToolTableManager;
        var recipes = GameSingleton.Existing<ConfigManager>()?._Config_ProductionList_Dict;

        if (tool == null || recipes == null) return NotInGame;

        var added = new List<string>();

        foreach (var kind in DefenseRows.All)
        {
            if (!recipes.ContainsKey(kind.Ids.Recipe)) return $"{kind.Name}: recipe {kind.Ids.Recipe} is not in the game's tables yet";
            if (tool.IsProductionRevealedByRecord(recipes[kind.Ids.Recipe])) continue;

            var ids = new Il2CppInts();

            ids.Add(kind.Ids.Recipe);
            tool.AddProductionRecords(ids);
            added.Add(kind.Name);
        }

        return added.Count == 0 ? "every recipe was already shown" : $"shown now: {string.Join(", ", added)}";
    }
}
