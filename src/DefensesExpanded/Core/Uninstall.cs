using System.Collections.Generic;

namespace DefensesExpanded.Core;

/// <summary>
/// What Uninstall prep turns the defenses' own ids into, so a save made with it on loads cleanly without the mod. A
/// game without the mod drops an item it does not know when it loads, so a defense package becomes the package of the
/// stock defense it copies (the package rows are copies of those, so its size and weight stay the same). Placed and
/// packed defenses need nothing here: every save already writes them as their stock defense.
/// </summary>
public static class Uninstall
{
    /// <summary>The stock package a defense package becomes, or null for any other item.</summary>
    public static int? StockItem(int itemId, IReadOnlyList<DefenseKind> kinds)
    {
        for (var i = 0; i < kinds.Count; i++)
            if (kinds[i].Ids.Item == itemId) return kinds[i].Stock.Item;

        return null;
    }

    /// <summary>The stock defense a defense's furniture id becomes, or null for any other furniture.</summary>
    public static int? StockFurniture(int furnitureId, IReadOnlyList<DefenseKind> kinds)
    {
        for (var i = 0; i < kinds.Count; i++)
            if (kinds[i].Ids.Furniture == furnitureId) return kinds[i].Stock.Furniture;

        return null;
    }
}
