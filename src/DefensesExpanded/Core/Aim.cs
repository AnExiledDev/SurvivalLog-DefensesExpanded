using System;

namespace DefensesExpanded.Core;

/// <summary>The turret head's turn toward its target, in degrees about the upright axis, 0 facing +Z.</summary>
public static class Aim
{
    /// <summary>The heading that faces a point this far across (x) and ahead (z) of the head.</summary>
    public static float Heading(float x, float z) => (float)(Math.Atan2(x, z) * 180.0 / Math.PI);

    /// <summary>The heading after turning from <paramref name="current"/> toward <paramref name="target"/> by at most <paramref name="most"/>, the short way round.</summary>
    public static float Turn(float current, float target, float most)
    {
        var delta = (target - current) % 360f;

        if (delta > 180f) delta -= 360f;
        else if (delta < -180f) delta += 360f;

        var step = Math.Clamp(delta, -most, most);
        var heading = (current + step) % 360f;

        return heading < 0f ? heading + 360f : heading;
    }
}
