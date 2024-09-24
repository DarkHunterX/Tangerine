using System;
using System.Collections.Generic;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class ItemInfo
{
    private static List<NetItemInfo> item_mods = new();

    internal static void Clear()
    {
        item_mods.Clear();
    }

    internal static bool HasData()
    {
        if (item_mods.Count > 0)
            return true;
        return false;
    }

    internal static void Remove()
    {
        foreach (var item in item_mods)
            ItemService.Instance._dicItem.Remove(item.ItemID);
    }

    internal static void Restore()
    {
        foreach (var item in item_mods)
            ItemService.Instance._dicItem.Add(item.ItemID, item);
    }

    internal static void ValidateItemInfo()
    {
        foreach (var item in ItemService.Instance._dicItem)
        {
            if (!OrangeDataManager.Instance.ITEM_TABLE_DICT.TryGetValue(item.Value.ItemID, out ITEM_TABLE item_TABLE))
                item_mods.Add(item.Value);
            else if (item_TABLE.n_ID >= 839001 && item_TABLE.n_ID <= 839546) // Perfect Save fix - remove cards from "Pack" section of inventory
                item_mods.Add(item.Value);
            else if (item_TABLE.n_ID == 3) // Perfect Save fix - remove paid EM
                item.Value.Stack = 0;
            else if (item.Value.Stack > item_TABLE.n_MAX)
                item.Value.Stack = item_TABLE.n_MAX;
        }
    }
}