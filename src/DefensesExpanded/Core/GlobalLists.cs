using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DefensesExpanded.Core;

/// <summary>
/// The game settings that list defenses by id hold one string, entries split by ';' and each entry's fields by ':'
/// ("36001:0.5;36002:0.65"). The mod adds its defenses' entries to the end and leaves the game's alone.
/// </summary>
public static class GlobalLists
{
    /// <summary>
    /// <paramref name="raw"/> with every entry naming one of <paramref name="ourIds"/> in field <paramref name="keyIndex"/>
    /// taken out (left from an earlier write, or with settings since changed) and <paramref name="ours"/> added at the end.
    /// Blank entries go; everything else stays as it was written, in its order.
    /// </summary>
    public static string Merge(string? raw, int keyIndex, IReadOnlyCollection<int> ourIds, IEnumerable<string> ours)
    {
        var kept = (raw ?? "").Split(';').Select(entry => entry.Trim()).Where(entry => entry.Length > 0 && !IsOurs(entry, keyIndex, ourIds));

        return string.Join(";", kept.Concat(ours));
    }

    private static bool IsOurs(string entry, int keyIndex, IReadOnlyCollection<int> ourIds)
    {
        var fields = entry.Split(':');

        return fields.Length > keyIndex && int.TryParse(fields[keyIndex].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && ourIds.Contains(id);
    }
}
