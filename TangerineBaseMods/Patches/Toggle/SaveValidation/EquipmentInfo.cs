using System;
using System.Collections.Generic;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class EquipmentInfo
{
    private static List<NetEquipmentInfo> armor_mods = new();

    internal static void Clear()
    {
        armor_mods.Clear();
    }

    internal static bool HasData()
    {
        if (armor_mods.Count > 0)
            return true;
        return false;
    }

    internal static void Remove()
    {
        foreach (var armor in armor_mods)
            EquipmentService.Instance._dicEquipment.Remove(armor.EquipmentID);
    }

    internal static void Restore()
    {
        foreach (var armor in armor_mods)
            EquipmentService.Instance._dicEquipment.Add(armor.EquipmentID, armor);
    }

    internal static void ValidateEquipmentInfo()
    {
        foreach (var armor in EquipmentService.Instance._dicEquipment)
        {
            if (armor.Key > EquipmentService.Instance._equipmentId)
                EquipmentService.Instance._equipmentId = armor.Key;

            if (!OrangeDataManager.Instance.EQUIP_TABLE_DICT.TryGetValue(armor.Value.EquipItemID, out EQUIP_TABLE armor_TABLE))
                armor_mods.Add(armor.Value);
            else
            {
                if (armor.Value.DefParam > armor_TABLE.n_DEF_MAX)
                    armor.Value.DefParam = armor_TABLE.n_DEF_MAX;
                if (armor.Value.HpParam > armor_TABLE.n_HP_MAX)
                    armor.Value.HpParam = armor_TABLE.n_HP_MAX;
                if (armor.Value.LukParam > armor_TABLE.n_LUK_MAX)
                    armor.Value.LukParam = armor_TABLE.n_LUK_MAX;
            }
        }
    }
}