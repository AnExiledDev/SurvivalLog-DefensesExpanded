using System;
using DefensesExpanded.Core;
using GameCore.HotUpdate.Battle.Show;
using ExiledToolkit.Game;
using UnityEngine;

namespace DefensesExpanded.Adapters;

/// <summary>
/// The game picks a defense's strike effect (the spikes, the electric arc, the chainsaw sparks) by the defense's own
/// furniture id, so ours would show none. While the effect is chosen, a defense of ours reads as the stock defense it
/// copies, and gets its id back straight after. The target was checked in GameAssembly.dll: no relative jump or call in
/// its first 14 bytes. The same step tells <see cref="TurretAim"/> which zombie a turret just shot.
/// </summary>
internal static class FxPatches
{
    private static PatchKit? _hooks;

    public static void Apply(PatchKit hooks)
    {
        _hooks = hooks;

        hooks.Patch(typeof(FxPatches), typeof(TowerDefenseShowManager), "PlayStrikeFx", prefix: nameof(AsStock), finalizer: nameof(AsOurs), consequence: "the defenses' attacks show no effect");
    }

    private static void AsStock(AgentModel __0, AgentModel __1, out int __state)
    {
        __state = 0;

        try
        {
            if (__0 == null || DefenseRows.ByFurniture(__0.configId) is not { } kind) return;

            if (kind == DefenseRows.Turret) TurretAim.Fired(__0.Obj, __1?.Obj, Time.realtimeSinceStartup);

            __state = __0.configId;
            __0.configId = kind.Stock.Furniture;
        }
        catch (Exception ex)
        {
            _hooks?.Fail("showing a defense's strike effect", ex);
        }
    }

    // A finalizer, so the id comes back even when the game's step throws; returning null lets the game's
    // exception through as it was.
    private static Exception? AsOurs(AgentModel __0, int __state, Exception? __exception)
    {
        try
        {
            if (__state != 0 && __0 != null) __0.configId = __state;
        }
        catch (Exception ex)
        {
            _hooks?.Fail("giving a defense its id back after its strike effect", ex);
        }

        return __exception;
    }
}
