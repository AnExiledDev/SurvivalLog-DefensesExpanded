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
