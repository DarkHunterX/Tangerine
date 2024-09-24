using HarmonyLib;
using Il2CppSystem;
using CallbackDefs;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tangerine.Manager.Mod;

namespace TangerineBaseMods;

public class DNA
{
    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony, JsonNode node)
    {
        if (node["DNA"]["enabled"].Deserialize<bool>())
        {
            harmony.PatchAll(typeof(DNA));
            Plugin.RemoveObsoleteMod_RestoredFunctions();
        }
        else
        {
            tangerine.Loader.RemoveAssetBundleId("ui/ui_characterinfo_dna");
            tangerine.Loader.RemoveAssetBundleId("ui/ui_dnalink");
            tangerine.TextDataManager.UnpatchTable("RULE_DNA", "LOCALIZATION_TABLE_DICT");
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterInfoUI), nameof(CharacterInfoUI.RefreshMenu))]
    private static bool RefreshMenu(CharacterInfoUI __instance, bool bFullRefresh)
    {
        __instance.CloseSubMenu();
        __instance.RefreshCharacterInfo();
        CommonTab component = __instance.m_DNABtnRoot.GetComponent<CommonTab>();
        component.SetButtonLock(false);
        if (PlayerHelper.Instance.GetLV() < OrangeConst.DNA_LV_LIMIT)
        {
            component.SetButtonLock(true);
        }
        var m_skinTable = __instance.m_skinTable;
        OrangeDataManager.Instance.SKIN_TABLE_DICT.TryGetValue(__instance.characterInfo.netInfo.Skin, out m_skinTable);
        __instance.m_skinTable = m_skinTable;
        __instance.RefreshPortrait(__instance.m_skinTable);
        int siblingIndex = __instance.transform.GetSiblingIndex();

        switch (__instance.currentTab)
        {
            default:
                {
                    UIManager.Instance.LoadUI<CharacterInfoBasic>("UI_CharacterInfo_Basic", new System.Action<CharacterInfoBasic>((CharacterInfoBasic ui) =>
                    {
                        ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(__instance.OnClickCloseBtn)).Cast<Callback>();
                        ui.Setup(__instance.characterInfo);
                        __instance.SetModelPreview(true);
                        __instance.m_currentUI = ui;
                        __instance.bIsUIActive = true;
                        __instance.RefreshModel(__instance.m_skinTable);
                        ui.transform.SetSiblingIndex(siblingIndex + 1);
                    }));
                    break;
                }
            case CharacterInfoUI.TAB_TYPE.SKILL_ACTIVE:
                {
                    UIManager.Instance.LoadUI<CharacterInfoSkill>("UI_CharacterInfo_Skill", new System.Action<CharacterInfoSkill>((CharacterInfoSkill ui) =>
                    {
                        ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(__instance.OnClickCloseBtn)).Cast<Callback>();
                        ui.Setup(__instance.characterInfo, CharacterInfoSkill.SKILLTAB_TYPE.ACTIVE);
                        __instance.SetModelPreview(false);
                        __instance.m_currentUI = ui;
                        __instance.bIsUIActive = true;
                        ui.transform.SetSiblingIndex(siblingIndex + 1);
                    }));
                    break;
                }
            case CharacterInfoUI.TAB_TYPE.SKILL_PASSIVE:
                {
                    UIManager.Instance.LoadUI<CharacterInfoSkill>("UI_CharacterInfo_Skill", new System.Action<CharacterInfoSkill>((CharacterInfoSkill ui) =>
                    {
                        ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(__instance.OnClickCloseBtn)).Cast<Callback>();
                        ui.Setup(__instance.characterInfo, CharacterInfoSkill.SKILLTAB_TYPE.PASSIVE);
                        __instance.SetModelPreview(false);
                        __instance.m_currentUI = ui;
                        __instance.bIsUIActive = true;
                        ui.transform.SetSiblingIndex(siblingIndex + 1);
                    }));
                    break;
                }
            case CharacterInfoUI.TAB_TYPE.UPGRADE:
                {
                    UIManager.Instance.LoadUI<CharacterInfoUpgrade>("UI_CharacterInfo_Upgrade", new System.Action<CharacterInfoUpgrade>((CharacterInfoUpgrade ui) =>
                    {
                        ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(__instance.OnClickCloseBtn)).Cast<Callback>();
                        ui.Setup(__instance.characterInfo);
                        __instance.SetModelPreview(true);
                        __instance.m_currentUI = ui;
                        __instance.bIsUIActive = true;
                        __instance.RefreshModel(__instance.m_skinTable);
                        ui.transform.SetSiblingIndex(siblingIndex + 1);
                    }));
                    break;
                }
            case CharacterInfoUI.TAB_TYPE.SKIN:
                {
                    UIManager.Instance.LoadUI<CharacterInfoSkin>("UI_CharacterInfo_Skin", new System.Action<CharacterInfoSkin>((CharacterInfoSkin ui) =>
                    {
                        ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(__instance.OnClickCloseBtn)).Cast<Callback>();
                        ui.Setup(__instance.characterInfo);
                        __instance.SetModelPreview(true);
                        __instance.m_currentUI = ui;
                        __instance.bIsUIActive = true;
                        __instance.RefreshModel(__instance.m_skinTable);
                        ui.transform.SetSiblingIndex(siblingIndex + 1);
                    }));
                    break;
                }
            case CharacterInfoUI.TAB_TYPE.CARD:
                {
                    UIManager.Instance.LoadUI<CharacterInfoCard>("UI_CharacterInfo_Card", new System.Action<CharacterInfoCard>((CharacterInfoCard ui) =>
                    {
                        ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(__instance.OnClickCloseBtn)).Cast<Callback>();
                        ui.Setup(__instance.characterInfo, false, false);
                        __instance.SetModelPreview(true);
                        __instance.m_currentUI = ui;
                        __instance.bIsUIActive = true;
                        __instance.RefreshModel(__instance.m_skinTable);
                        ui.transform.SetSiblingIndex(siblingIndex + 1);
                    }));
                    break;
                }
            case CharacterInfoUI.TAB_TYPE.DNA:
                {
                    UIManager.Instance.LoadUI<CharacterInfoDNA>("UI_CharacterInfo_DNA", new System.Action<CharacterInfoDNA>((CharacterInfoDNA ui) =>
                    {
                        ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(__instance.OnClickCloseBtn)).Cast<Callback>();
                        ui.Setup(__instance.characterInfo);
                        __instance.SetModelPreview(false);
                        __instance.m_currentUI = ui;
                        __instance.bIsUIActive = true;
                        ui.transform.SetSiblingIndex(siblingIndex + 1);
                    }));
                    break;
                }
        }

        __instance.RefreshBadges();
        if (bFullRefresh)
        {
            __instance.RefreshQuickSelectBar(false);
        }
        __instance.RefreshSideButtonRedDots();

        // do not run original code
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterInfoDNA), nameof(CharacterInfoDNA.UpdateDNAFX))]
    private static bool UpdateDNAFX(CharacterInfoDNA __instance)
    {
        if (__instance._characterInfo.netDNAInfoDic.Count == 8)
        {
            __instance.EnableDNAFX(false);
            __instance.EnableDNAFX2(true);
        }
        else
        {
            __instance.EnableDNAFX(true);
            __instance.EnableDNAFX2(false);
        }

        // do not run original code
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CommonIconBase), nameof(CommonIconBase.SetupCharacter))]
    private static void SetupCharacter(CommonIconBase __instance, ref NetCharacterInfo inputNetCharacterInfo, bool bSetFavorite)
    {
        var character_TABLE = OrangeDataManager.Instance.CHARACTER_TABLE_DICT[inputNetCharacterInfo.CharacterID];
        bool flag = false;
        if (PlayerNetManager.Instance.dicCharacter.TryGetValue(inputNetCharacterInfo.CharacterID, out CharacterInfo characterInfo))
        {
            flag = (characterInfo.netDNAInfoDic.Count >= 8);
        }
        bool flag2 = inputNetCharacterInfo.State == 1;
        bool flag3 = bSetFavorite && inputNetCharacterInfo.Favorite != 0;

        if (__instance.SmallVer)
        {
            if (flag && __instance.imgRareFrameShiny != null)
            {
                __instance.imgRareFrame.gameObject.SetActive(false);
                __instance.imgRareFrameShiny.gameObject.SetActive(true);
                __instance.SetRareInfo(__instance.imgRareFrameShiny, string.Format(__instance.rare_asset_name, __instance.frameName, "UP_" + __instance.strRarity[character_TABLE.n_RARITY]), true);
                __instance.SetRareInfo(__instance.imgRareBg, string.Format(__instance.rare_asset_name, __instance.bgName, "UP_" + __instance.strRarity[character_TABLE.n_RARITY] + __instance.small), flag2 && !flag3);
            }
            else
            {
                __instance.imgRareFrame.gameObject.SetActive(true);
                __instance.imgRareFrameShiny.gameObject.SetActive(false);
                __instance.SetRareInfo(__instance.imgRareFrame, string.Format(__instance.rare_asset_name, __instance.frameName, __instance.strRarity[character_TABLE.n_RARITY]), true);
                __instance.SetRareInfo(__instance.imgRareBg, string.Format(__instance.rare_asset_name, __instance.bgName, __instance.strRarity[character_TABLE.n_RARITY] + __instance.small), flag2 && !flag3);
            }
        }
        // display icon frame (large)
        else
        {
            if (flag && __instance.imgRareFrameShiny != null)
            {
                __instance.imgRareFrame.gameObject.SetActive(false);
                __instance.imgRareFrameShiny.gameObject.SetActive(true);
                __instance.SetRareInfo(__instance.imgRareFrameShiny, string.Format(__instance.rare_asset_name, __instance.frameName, "UP_" + __instance.strRarity[character_TABLE.n_RARITY] + "_L"), true);
                __instance.SetRareInfo(__instance.imgRareBg, string.Format(__instance.rare_asset_name, __instance.bgName, "UP_" + __instance.strRarity[character_TABLE.n_RARITY]), flag2 && !flag3);
            }
            else
            {
                __instance.imgRareFrame.gameObject.SetActive(true);
                __instance.imgRareFrameShiny.gameObject.SetActive(false);
                __instance.SetRareInfo(__instance.imgRareFrame, string.Format(__instance.rare_asset_name, __instance.frameName, __instance.strRarity[character_TABLE.n_RARITY] + "_L"), true);
                __instance.SetRareInfo(__instance.imgRareBg, string.Format(__instance.rare_asset_name, __instance.bgName, __instance.strRarity[character_TABLE.n_RARITY]), flag2 && !flag3);
            }
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CommonIconBase), nameof(CommonIconBase.SetOtherInfo), new[] { typeof(NetCharacterInfo), typeof(bool), typeof(bool), typeof(bool) })]
    private static void SetOtherInfo(CommonIconBase __instance, NetCharacterInfo p_netCharacter, bool bUsed)
    {
        var character_TABLE = OrangeDataManager.Instance.CHARACTER_TABLE_DICT[p_netCharacter.CharacterID];
        bool flag = false;
        if (PlayerNetManager.Instance.dicCharacter.TryGetValue(p_netCharacter.CharacterID, out CharacterInfo characterInfo))
        {
            flag = (characterInfo.netDNAInfoDic.Count >= 8);
        }

        if (__instance.SmallVer)
        {
            if (flag && __instance.imgRareFrameShiny != null)
            {
                __instance.imgRareFrame.gameObject.SetActive(false);
                __instance.imgRareFrameShiny.gameObject.SetActive(true);
                __instance.SetRareInfo(__instance.imgRareFrame, string.Format(__instance.rare_asset_name, __instance.frameName, "UP_" + __instance.strRarity[character_TABLE.n_RARITY]), true);
                __instance.SetRareInfo(__instance.imgRareBg, string.Format(__instance.rare_asset_name, __instance.bgName, "UP_" + __instance.strRarity[character_TABLE.n_RARITY] + __instance.small), !bUsed);
            }
            else
            {
                __instance.imgRareFrame.gameObject.SetActive(true);
                __instance.imgRareFrameShiny.gameObject.SetActive(false);
                __instance.SetRareInfo(__instance.imgRareFrame, string.Format(__instance.rare_asset_name, __instance.frameName, __instance.strRarity[character_TABLE.n_RARITY]), true);
                __instance.SetRareInfo(__instance.imgRareBg, string.Format(__instance.rare_asset_name, __instance.bgName, __instance.strRarity[character_TABLE.n_RARITY] + __instance.small), !bUsed);
            }
        }
        else
        {
            if (flag && __instance.imgRareFrameShiny != null)
            {
                __instance.imgRareFrame.gameObject.SetActive(false);
                __instance.imgRareFrameShiny.gameObject.SetActive(true);
                __instance.SetRareInfo(__instance.imgRareFrame, string.Format(__instance.rare_asset_name, __instance.frameName, "UP_" + __instance.strRarity[character_TABLE.n_RARITY] + "_L"), true);
                __instance.SetRareInfo(__instance.imgRareBg, string.Format(__instance.rare_asset_name, __instance.bgName, "UP_" + __instance.strRarity[character_TABLE.n_RARITY] + __instance.small), !bUsed);
            }
            else
            {
                __instance.imgRareFrame.gameObject.SetActive(true);
                __instance.imgRareFrameShiny.gameObject.SetActive(false);
                __instance.SetRareInfo(__instance.imgRareFrame, string.Format(__instance.rare_asset_name, __instance.frameName, __instance.strRarity[character_TABLE.n_RARITY] + "_L"), true);
                __instance.SetRareInfo(__instance.imgRareBg, string.Format(__instance.rare_asset_name, __instance.bgName, __instance.strRarity[character_TABLE.n_RARITY] + __instance.small), !bUsed);
            }
        }
    }
}