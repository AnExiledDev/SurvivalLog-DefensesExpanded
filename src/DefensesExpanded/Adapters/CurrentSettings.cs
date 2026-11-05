using System;
using System.Collections.Generic;
using System.Linq;
using DefensesExpanded.Core;

namespace DefensesExpanded.Adapters;

/// <summary>
/// The settings each defense runs on now, in the order of <see cref="DefenseRows.All"/>. A change replaces the whole
/// list, so a reader always sees one complete set and the table check sees a change by the list itself.
/// </summary>
internal static class CurrentSettings
{
    private static DefenseSettings[] _current = DefenseRows.All.Select(kind => kind.Defaults).ToArray();

    public static IReadOnlyList<DefenseSettings> Current => _current;

    public static DefenseSettings Of(DefenseKind kind) => _current[IndexOf(kind)];

    public static void Set(DefenseKind kind, DefenseSettings settings)
    {
        var next = (DefenseSettings[])_current.Clone();

        next[IndexOf(kind)] = settings;
        _current = next;
    }

    private static int IndexOf(DefenseKind kind)
    {
        for (var i = 0; i < DefenseRows.All.Count; i++)
            if (ReferenceEquals(DefenseRows.All[i], kind)) return i;

        throw new ArgumentException(kind.Name + " is not one of the mod's defenses", nameof(kind));
    }
}
