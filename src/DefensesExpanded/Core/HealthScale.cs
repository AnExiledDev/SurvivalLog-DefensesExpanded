using System;

namespace DefensesExpanded.Core;

/// <summary>
/// A placed defense's health after its Health setting changes. Its most can be above the setting (the game adds a
/// talent's share on top of the row), so both numbers move by the setting's own ratio: the defense keeps its share of
/// health and its bonus. A defense whose Health setting did not change is left exactly as it is.
/// </summary>
public static class HealthScale
{
    public static (int Max, int Current) Rescale(int max, int current, int oldHealth, int newHealth)
    {
        if (oldHealth <= 0 || newHealth <= 0 || oldHealth == newHealth) return (max, current);

        var ratio = (double)newHealth / oldHealth;
        var newMax = Math.Max(1, (int)Math.Round(max * ratio));

        return (newMax, Math.Clamp((int)Math.Round(current * ratio), 0, newMax));
    }
}
