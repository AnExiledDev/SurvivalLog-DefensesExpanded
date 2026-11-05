using System;
using System.Collections.Generic;
using DefensesExpanded.Core;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using Il2CppItemSaves = Il2CppSystem.Collections.Generic.List<GameCore.HotUpdate.ItemSave>;

namespace DefensesExpanded.Adapters;

/// <summary>
/// While the Uninstall prep setting is on, turns every defense package in the loaded run into the package of the stock
/// defense it copies (see <see cref="Uninstall"/>): in bags, storage and on the floor of the place the player is in,
/// and the same in every other place the save holds. Placed and packed defenses are always saved as their stock twins,
/// so they need nothing. Nothing it changes is undone when the setting goes off again.
/// </summary>
internal static class UninstallPrep
{
    private const float LiveSeconds = 3f;
    private const float SavedSeconds = 30f;

    private static readonly HashSet<long> Touched = new();

    private static volatile bool _isOn;
    private static float _nextLiveAt;
    private static float _nextSavedAt;
    private static IntPtr _sweptSave;

    internal static Action<string> Log = _ => { };

    public static bool IsOn => _isOn;

    /// <summary>Turns the sweep on or off. Safe from any thread.</summary>
    public static void Set(bool isOn)
    {
        _isOn = isOn;
        _sweptSave = IntPtr.Zero;
        _nextLiveAt = 0;
        _nextSavedAt = 0;
    }

    public static void Tick(float now)
    {
        if (!_isOn || now < _nextLiveAt) return;

        _nextLiveAt = now + LiveSeconds;

        var items = GameSingleton.Existing<BattleLogicWorld>()?._ItemManager;

        if (items == null) return;

        var packages = Packages(items);
        var saved = 0;
        var save = GameSingleton.Existing<GameSaveManager>()?._gameSave_k__BackingField?._CurSave_k__BackingField;

        if (save != null && (save.Pointer != _sweptSave || now >= _nextSavedAt))
        {
            _sweptSave = save.Pointer;
            _nextSavedAt = now + SavedSeconds;
            saved = Saved(save);
        }

        if (packages + saved > 0)
            Log($"Uninstall prep: {packages} defense packages here and {saved} saved elsewhere turned into the stock defense packages. Save the game, then remove DefensesExpanded.dll.");
    }

    /// <summary>The defense packages in the place loaded now, wherever they lie. Each bag's weight is kept right.</summary>
    private static int Packages(ItemManager items)
    {
        var cache = items._Cache_k__BackingField;

        if (cache == null) return 0;

        var count = 0;

        Touched.Clear();

        foreach (var item in cache.Values)
        {
            if (item == null || Uninstall.StockItem(item._ItemConfigId_k__BackingField, DefenseRows.All) is not int stock) continue;

            var before = items.GetItemActualWeight(item);

            item._ItemConfigId_k__BackingField = stock;

            var delta = items.GetItemActualWeight(item) - before;
            var owner = item._OwnerId_k__BackingField;

            if (delta != 0) items.UpdateOwnerBagWeight(owner, delta, stock, item._ItemCount_k__BackingField);

            Touched.Add(owner);
            count++;
        }

        foreach (var owner in Touched)
            if (owner > 0) items.SyncSingleBag(owner);

        Touched.Clear();

        return count;
    }

    /// <summary>
    /// The save's copy of every place: the packages held by each thing saved there. The place loaded now is written
    /// over from the live game when it is saved, which the sweep above has already changed.
    /// </summary>
    private static int Saved(SaveChildData save)
    {
        var count = 0;

        if (save._LeadingRole_k__BackingField is { } player) count += Agent(player);

        if (save._ChapterAgentMap_k__BackingField is { } chapters)
            foreach (var chapter in chapters.Values)
            {
                if (chapter == null) continue;

                foreach (var agent in chapter)
                    if (agent != null) count += Agent(agent);
            }

        return count;
    }

    private static int Agent(AgentSave agent) => Items(agent._ItemList_k__BackingField) + Items(agent._ShredderTrayItems_k__BackingField) + Items(agent._CompressorTrayItems_k__BackingField);

    private static int Items(Il2CppItemSaves? list)
    {
        if (list == null) return 0;

        var count = 0;

        foreach (var item in list)
        {
            if (item == null || Uninstall.StockItem(item._ItemConfigId_k__BackingField, DefenseRows.All) is not int stock) continue;

            item._ItemConfigId_k__BackingField = stock;
            count++;
        }

        return count;
    }
}
