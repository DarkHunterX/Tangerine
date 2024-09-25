using HarmonyLib;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;
internal class SaveValidation
{
    private static bool dnaErase = false;

    internal static void InitializeHarmony(Harmony harmony, JsonNode node)
    {
        if (node["SaveValidation"]["enabled"].Deserialize<bool>())
        {
            if (node["DNA"]["enabled"].Deserialize<bool>()) { CharacterInfo.dnaEnabled = true; }
            if (node["SaveValidation"]["erase disabled DNA"].Deserialize<bool>()) { dnaErase = false; }
            harmony.PatchAll(typeof(SaveValidation));
        }
    }

    private static void ClearModDicts(bool clearDNA = true)
    {
        PlayerInfo.Clear();
        CharacterInfo.Clear(clearDNA);
        WeaponInfo.Clear();
        ChipInfo.Clear();
        CardInfo.Clear();
        FinalStrikeInfo.Clear();
        EquipmentInfo.Clear();
        ItemInfo.Clear();
        GalleryInfo.Clear();
        MissionInfo.Clear();
        ShopInfo.Clear();
        StageInfo.Clear();
    }

    private static bool HasRemovedMods()
    {
        if (PlayerInfo.HasData())
            return true;
        if (CharacterInfo.HasData())
            return true;
        if (WeaponInfo.HasData())
            return true;
        if (ChipInfo.HasData())
            return true;
        if (CardInfo.HasData())
            return true;
        if (FinalStrikeInfo.HasData())
            return true;
        if (EquipmentInfo.HasData())
            return true;
        if (ItemInfo.HasData())
            return true;
        if (GalleryInfo.HasData())
            return true;
        if (MissionInfo.HasData())
            return true;
        if (ShopInfo.HasData())
            return true;
        if (StageInfo.HasData())
            return true;
        return false;
    }

    private static void RemoveModData()
    {
        PlayerInfo.Remove();
        CharacterInfo.Remove();
        WeaponInfo.Remove();
        ChipInfo.Remove();
        CardInfo.Remove();
        FinalStrikeInfo.Remove();
        EquipmentInfo.Remove();
        ItemInfo.Remove();
        GalleryInfo.Remove();
        MissionInfo.Remove();
        ShopInfo.Remove();
        StageInfo.Remove();
    }

    private static void RestoreModData()
    {
        PlayerInfo.Restore();
        CharacterInfo.Restore();
        WeaponInfo.Restore();
        ChipInfo.Restore();
        CardInfo.Restore();
        FinalStrikeInfo.Restore();
        EquipmentInfo.Restore();
        ItemInfo.Restore();
        GalleryInfo.Restore();
        MissionInfo.Restore();
        ShopInfo.Restore();
        StageInfo.Restore();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ConsoleService), nameof(ConsoleService.Load))]
    private static void LoadGame()
    {
        PlayerInfo.ValidatePlayerInfo();
        CharacterInfo.ValidateCharacterInfo();
        WeaponInfo.ValidateWeaponInfo();
        ChipInfo.ValidateChipInfo();
        CardInfo.ValidateCardInfo();
        FinalStrikeInfo.ValidateFinalStrikeInfo();
        EquipmentInfo.ValidateEquipmentInfo();
        ItemInfo.ValidateItemInfo();
        GalleryInfo.ValidateGalleryInfo();
        MissionInfo.ValidateMissionInfo();
        ShopInfo.ValidateShopInfo();
        StageInfo.ValidateStageInfo();
        RemoveModData();
    }

    // hooking this allows dupe card Seq ID error to be handled by the game without it choking
    // the error will be printed to console window and dupe card will be discarded
    [HarmonyPostfix, HarmonyPatch(typeof(CardService), nameof(CardService.Load))]
    private static void CatchCardError()
    {
    }

    [HarmonyPostfix, HarmonyPatch(typeof(StandaloneConsoleStorage), nameof(StandaloneConsoleStorage.LoadGame))]
    private static void ValidateClientSaveInfo(StandaloneConsoleStorage __instance)
    {
        ClearModDicts();

        var exp_TABLE = OrangeTableHelper.Instance.GetExpTable(__instance.SaveInfoEx.PlayerExp);
        if (__instance.SaveInfoEx.PlayerExp >= exp_TABLE.n_TOTAL_RANKEXP)
            __instance.SaveInfoEx.PlayerExp = exp_TABLE.n_TOTAL_RANKEXP - 1;

        StageInfo.ValidateEventStageInfo(exp_TABLE);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(StandaloneConsoleStorage), nameof(StandaloneConsoleStorage.NewGame))]
    private static void NewGame(StandaloneConsoleStorage __instance)
    {
        ClearModDicts();
    }

    [HarmonyPrefix, HarmonyPatch(typeof(StandaloneConsoleStorage), nameof(StandaloneConsoleStorage.AutoSave))]
    private static bool AutoSave(StandaloneConsoleStorage __instance)
    {
        RestoreModData();
        __instance.SaveTo(SaveIndex.AUTO_SAVE);
        RemoveModData();

        // do not run original code
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(StandaloneConsoleStorage), nameof(StandaloneConsoleStorage.SaveGame))]
    private static bool SaveGame(StandaloneConsoleStorage __instance, int saveIdx, Il2CppSystem.Action<bool> cb)
    {
        if (HasRemovedMods())
        {
            UIManager.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", new Action<CommonUI>((CommonUI ui) =>
            {
                ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
                ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
                ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;

                ui.SetupYesNO(LocalizationManager.Instance.GetStr("COMMON_TIP"), LocalizationManager.Instance.GetStr("DELETE_REMOVED_MODS"), LocalizationManager.Instance.GetStr("COMMON_YES"), LocalizationManager.Instance.GetStr("COMMON_NO"), new Action(() =>
                {
                    // on click yes
                    if (!dnaErase && CharacterInfo.HasRemovedDNA())
                    {
                        CharacterInfo.RestoreDnaData();
                        __instance.SaveTo((SaveIndex)saveIdx, cb);
                        CharacterInfo.RemoveDnaData();
                        ClearModDicts(false);
                    }
                    else
                    {
                        __instance.SaveTo((SaveIndex)saveIdx, cb);
                        ClearModDicts();
                    }
                }), new Action(() =>
                {
                    // on click no
                    RestoreModData();
                    __instance.SaveTo((SaveIndex)saveIdx, cb);
                    RemoveModData();
                }));
            }), true, false);

            // do not run original code
            return false;
        }
        else if (CharacterInfo.HasRemovedDNA())
        {
            if (!dnaErase)
            {
                CharacterInfo.RestoreDnaData();
                __instance.SaveTo((SaveIndex)saveIdx, cb);
                CharacterInfo.RemoveDnaData();
                ClearModDicts(false);
            }
            else
            {
                __instance.SaveTo((SaveIndex)saveIdx, cb);
                ClearModDicts();
            }

            // do not run original code
            return false;
        }

        // run original code
        return true;
    }
}
