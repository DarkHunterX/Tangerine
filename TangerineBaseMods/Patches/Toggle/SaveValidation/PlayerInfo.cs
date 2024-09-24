using System;
using System.Collections.Generic;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class PlayerInfo
{
    private static List<NetTutorialInfo> tutorial_mods = new();

    internal static void Clear()
    {
        tutorial_mods.Clear();
    }

    internal static bool HasData()
    {
        if (tutorial_mods.Count > 0)
            return true;
        return false;
    }

    internal static void Remove()
    {
        foreach (var tutorial in tutorial_mods)
            PlayerService.Instance._dicTutorial.Remove(tutorial.TutorialId);
    }

    internal static void Restore()
    {
        foreach (var tutorial in tutorial_mods)
            PlayerService.Instance._dicTutorial.Add(tutorial.TutorialId, tutorial);
    }

    internal static void ValidatePlayerInfo()
    {
        if (!OrangeDataManager.Instance.CHARACTER_TABLE_DICT.TryGetValue(PlayerService.Instance._playerInfo.StandbyChara, out CHARACTER_TABLE character_TABLE))
            PlayerService.Instance._playerInfo.StandbyChara = 1; // B-rank X

        if (!OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(PlayerService.Instance._playerInfo.MainWeaponID, out WEAPON_TABLE MainWeapon_TABLE))
            PlayerService.Instance._playerInfo.MainWeaponID = 100001; // Mega Buster

        if (!OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(PlayerService.Instance._playerInfo.SubWeaponID, out WEAPON_TABLE SubWeapon_TABLE))
            PlayerService.Instance._playerInfo.SubWeaponID = 101001; // Standard Saber

        if (!OrangeDataManager.Instance.FS_TABLE_DICT.TryGetValue(PlayerService.Instance._playerInfo.MainWeaponFSID, out FS_TABLE MainFS_TABLE))
            PlayerService.Instance._playerInfo.MainWeaponFSID = 0; // none

        if (!OrangeDataManager.Instance.FS_TABLE_DICT.TryGetValue(PlayerService.Instance._playerInfo.SubWeaponFSID, out FS_TABLE SubFS_TABLE))
            PlayerService.Instance._playerInfo.SubWeaponFSID = 0; // none

        var exp_TABLE = OrangeTableHelper.Instance.GetExpTable(PlayerService.Instance._playerInfo.Exp);
        if (PlayerService.Instance._playerInfo.Exp >= exp_TABLE.n_TOTAL_RANKEXP)
            PlayerService.Instance._playerInfo.Exp = exp_TABLE.n_TOTAL_RANKEXP - 1;

        foreach (var armorSlot in PlayerService.Instance._dicPlayerEquipInfo)
        {
            if (armorSlot.Value.EnhanceLv > exp_TABLE.n_ID)
                armorSlot.Value.EnhanceLv = (short)exp_TABLE.n_ID;
        }

        foreach (var tutorial in PlayerService.Instance._dicTutorial)
        {
            if (!OrangeDataManager.Instance.TUTORIAL_TABLE_DICT.TryGetValue(tutorial.Value.TutorialId, out TUTORIAL_TABLE tutorial_TABLE))
                tutorial_mods.Add(tutorial.Value);
        }
    }
}