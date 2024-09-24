using System;
using System.Collections.Generic;
using System.Linq;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class ChipInfo
{
    private static List<NetChipInfo> chip_mods = new();

    private static int cacheExp;
    private static EXP_TABLE cacheExpTable;
    private static Dictionary<int, EXP_TABLE> _dictChipExp = new();

    internal static void Clear()
    {
        chip_mods.Clear();
    }

    internal static bool HasData()
    {
        if (chip_mods.Count > 0)
            return true;
        return false;
    }

    internal static void Remove()
    {
        foreach (var chip in chip_mods)
            ChipService.Instance._dicChip.Remove(chip.ChipID);
    }

    internal static void Restore()
    {
        foreach (var chip in chip_mods)
            ChipService.Instance._dicChip.Add(chip.ChipID, chip);
    }

    internal static void ValidateChipInfo()
    {
        foreach (var chip in ChipService.Instance._dicChip)
        {
            if (!OrangeDataManager.Instance.DISC_TABLE_DICT.TryGetValue(chip.Value.ChipID, out DISC_TABLE chip_TABLE))
                chip_mods.Add(chip.Value);
            else
            {
                var exp_TABLE = GetExpTable(chip.Value.Exp);
                if (chip.Value.Exp >= exp_TABLE.n_TOTAL_DISCEXP)
                    chip.Value.Exp = exp_TABLE.n_TOTAL_DISCEXP - 1;
            }
        }
    }

    private static EXP_TABLE GetExpTable(int nExp)
    {
        if (cacheExp > 0 && cacheExp == nExp)
            return cacheExpTable;

        cacheExpTable = null;
        foreach (var exp_TABLE in OrangeDataManager.Instance.EXP_TABLE_DICT)
        {
            if (!_dictChipExp.TryGetValue(exp_TABLE.Key, out EXP_TABLE exp_TBL))
                _dictChipExp.Add(exp_TABLE.Key, exp_TABLE.Value);

            if (nExp < exp_TABLE.Value.n_TOTAL_DISCEXP)
            {
                cacheExpTable = exp_TABLE.Value;
                int num = exp_TABLE.Value.n_TOTAL_DISCEXP - nExp;
                if (num <= exp_TABLE.Value.n_DISCEXP)
                    break;
            }
        }

        if (cacheExpTable == null)
        {
            if (nExp > 0)
                cacheExpTable = _dictChipExp.Last().Value;
            else
                cacheExpTable = _dictChipExp.First().Value;
        }

        cacheExp = nExp;
        return cacheExpTable;
    }
}