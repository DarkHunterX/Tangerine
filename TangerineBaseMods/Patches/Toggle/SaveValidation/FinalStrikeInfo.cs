using System;
using System.Collections.Generic;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class FinalStrikeInfo
{
    private static List<NetFinalStrikeInfo> finalStrike_mods = new();

    internal static void Clear()
    {
        finalStrike_mods.Clear();
    }

    internal static bool HasData()
    {
        if (finalStrike_mods.Count > 0)
            return true;
        return false;
    }

    internal static void Remove()
    {
        foreach (var fsSkill in finalStrike_mods)
            FinalStrikeService.Instance._dicFinalStrike.Remove(fsSkill.FinalStrikeID);
    }

    internal static void Restore()
    {
        foreach (var fsSkill in finalStrike_mods)
            FinalStrikeService.Instance._dicFinalStrike.Add(fsSkill.FinalStrikeID, fsSkill);
    }

    internal static void ValidateFinalStrikeInfo()
    {
        foreach (var fsSkill in FinalStrikeService.Instance._dicFinalStrike)
        {
            if (!OrangeDataManager.Instance.FS_TABLE_DICT.TryGetValue(fsSkill.Value.FinalStrikeID, out FS_TABLE fs_TABLE))
                finalStrike_mods.Add(fsSkill.Value);
        }
    }
}