using System;
using System.Collections.Generic;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class ShopInfo
{
    private static List<NetShopRecordDB> shop_mods = new();

    internal static void Clear()
    {
        shop_mods.Clear();
    }

    internal static bool HasData()
    {
        if (shop_mods.Count > 0)
            return true;
        return false;
    }

    internal static void Remove()
    {
        foreach (var shopRecord in shop_mods)
            ShopService.Instance._dicShopRecord.Remove(shopRecord.ShopID);
    }

    internal static void Restore()
    {
        foreach (var shopRecord in shop_mods)
            ShopService.Instance._dicShopRecord.Add(shopRecord.ShopID, shopRecord);
    }

    internal static void ValidateShopInfo()
    {
        foreach (var shopRecord in ShopService.Instance._dicShopRecord)
        {
            if (!OrangeDataManager.Instance.SHOP_TABLE_DICT.TryGetValue(shopRecord.Value.ShopID, out SHOP_TABLE shop_TABLE))
                shop_mods.Add(shopRecord.Value);
        }
    }
}