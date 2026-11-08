using System.Collections.Generic;
using System.Linq;

namespace DefensesExpanded.Core;

/// <summary>
/// The defense skill table has, for each level, the recipes that level unlocks: one list for every role, and one per
/// role that replaces it when it has anything in it (a list of just 0 means the role gets nothing at that level). The
/// game locks a recipe below the first level whose list for the player's role names it, and leaves a recipe no list
/// names unlocked.
/// </summary>
public static class UnlockLevels
{
    /// <summary>
    /// One level's list with our recipes taken out and put back where they belong: in the shared list and in each role
    /// list that is in use, at the level each recipe asks for. A level of 0 (or past the top) is in no list, so it needs
    /// no level. An empty role list stays empty, so the role keeps using the shared list.
    /// </summary>
    public static List<int> Rebuild(IReadOnlyList<int> list, bool roleList, int level, IReadOnlyCollection<int> ours, IReadOnlyDictionary<int, int> unlockAt)
    {
        var result = list.Where(id => !ours.Contains(id)).ToList();

        if (roleList && result.Count == 0) return result;

        foreach (var pair in unlockAt.OrderBy(pair => pair.Key))
            if (pair.Value == level) result.Add(pair.Key);

        return result;
    }
}
