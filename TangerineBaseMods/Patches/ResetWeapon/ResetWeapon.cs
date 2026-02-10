using CallbackDefs;
using enums;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using OrangeConsoleService;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Utils;
using TangerineBaseMods.Config;
using UnityEngine;
using UnityEngine.UI;

namespace TangerineBaseMods.Patches;

internal class ResetWeapon
{
    internal static void InitializeHarmony(Harmony harmony)
    {
        harmony.PatchAll(typeof(ResetWeapon));
    }

    private static List<Button> resetTypeToggle = new();
    private static WeaponResetMaterials resetRewards = new();

    private enum bitIndex
    {
        _0 = 1,
        _1 = 2,
        _2 = 4,
        _3 = 8,
        _4 = 16,
        _5 = 32
    }

    private enum WepResetType : short
    {
        Level = 1,
        Expert = 2,
        Skill = 4,
        Passive = 8,
        Rank = 16,
        Sell = 32
    }

    [HarmonyPostfix, HarmonyPatch(typeof(WeaponInfoUI), nameof(WeaponInfoUI.Start))]
    private static void Start_Postfix(WeaponInfoUI __instance)
    {
        #region resize UI window
        foreach (var component in UIHelpers.GetTopLevelChildren(__instance.takeoutroot))
        {
            if (component.name == "Image" && component.transform.childCount > 0)
            {
                // resize background image
                var bg = component.GetComponent<RectTransform>();
                bg.sizeDelta = new Vector2(1380, 988);

                // move other bg elements
                bg.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(-3, 322);
                bg.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(-3, -396);
                bg.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -304);
                bg.GetChild(3).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 256);
            }
            if (component.name.StartsWith("TitleBG"))
            {
                var rect = component.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 430);
            }
            if (component.name == "TitleText")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(16, 431.9f);
            if (component.name == "CloseBtn")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(620, 430);
            if (component.name == "TakeOutBtn")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(432, -389);
            if (component.name == "HeadBg0")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(-622, 206);
            if (component.name == "HeadBg1")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(-622, -75);
            if (component.name == "takeoutlbl0")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(2, 321);
            if (component.name == "takeoutlbl1")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(-583, -385);
            if (component.name == "takeouttypebtn0")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(-480, 116.5f);
            if (component.name == "takeouttypebtn1")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(-90, 116.5f);
            if (component.name == "takeouttypebtn2")
                component.GetComponent<RectTransform>().anchoredPosition = new Vector2(300, 116.5f);
            if ((component.name.StartsWith("Image") && component.transform.childCount == 0) || component.name.StartsWith("takeoutitem"))
            {
                var rect = component.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -203);
            }
        }
        #endregion
        
        #region add additional UI components
        // get weapon reset type buttons
        var resetWepLevel = __instance.takeoutroot.transform.Find("takeouttypebtn0");
        var resetWepUpgrade = __instance.takeoutroot.transform.Find("takeouttypebtn1");
        var resetWepSkill = __instance.takeoutroot.transform.Find("takeouttypebtn2");

        // create new reset type button for passives
        var resetWepPassive = UIHelpers.CopyGameObject(resetWepSkill.gameObject, resetWepSkill.GetSiblingIndex()+1);
        resetWepPassive.name = "takeouttypebtn3";
        resetWepPassive.GetComponent<RectTransform>().anchoredPosition = new Vector2(-480, 15);

        // set button onClick events
        var resetWepPassiveBtn = resetWepPassive.transform.GetComponent<Button>();
        resetWepPassiveBtn.onClick = new Button.ButtonClickedEvent();
        resetWepPassiveBtn.onClick.AddListener(new Action(() => __instance.SetTakeOutType((int)WepResetType.Passive)));

        // set button text
        var passiveText = resetWepPassive.transform.GetChild(0).GetComponent<OrangeText>();
        passiveText.IsLocalizationText = true;
        passiveText.LocalizationKey = "FUNCTION_PASSIVE_SKILL";
        passiveText.text = LocalizationManager.Instance.GetStr("FUNCTION_PASSIVE_SKILL");

        // set button mask
        var passiveToggle = resetWepPassive.transform.GetChild(1).GetComponent<RectTransform>();
        passiveToggle.name = "takeouttypemask3";
        passiveToggle.gameObject.SetActive(false);

        //========================================

        // create new reset type button for star rank
        var resetWepRank = UIHelpers.CopyGameObject(resetWepSkill.gameObject, resetWepSkill.GetSiblingIndex()+2);
        resetWepRank.name = "takeouttypebtn4";
        resetWepRank.GetComponent<RectTransform>().anchoredPosition = new Vector2(-90, 15);

        // set button onClick events
        var resetWepRankBtn = resetWepRank.transform.GetComponent<Button>();
        resetWepRankBtn.onClick = new Button.ButtonClickedEvent();
        resetWepRankBtn.onClick.AddListener(new Action(() => __instance.SetTakeOutType((int)WepResetType.Rank)));

        // set button text
        var rankText = resetWepRank.transform.GetChild(0).GetComponent<OrangeText>();
        rankText.IsLocalizationText = true;
        rankText.LocalizationKey = "COMMON_RANK";
        rankText.text = LocalizationManager.Instance.GetStr("COMMON_RANK");

        // set button mask
        var rankToggle = resetWepRank.transform.GetChild(1).GetComponent<RectTransform>();
        rankToggle.name = "takeouttypemask4";
        rankToggle.gameObject.SetActive(false);

        //========================================

        // create new reset type button for selling
        var resetWepSell = UIHelpers.CopyGameObject(resetWepSkill.gameObject, resetWepSkill.GetSiblingIndex()+3);
        resetWepSell.name = "takeouttypebtn5";
        resetWepSell.GetComponent<RectTransform>().anchoredPosition = new Vector2(300, 15);

        // set button onClick events
        var resetWepSellBtn = resetWepSell.transform.GetComponent<Button>();
        resetWepSellBtn.onClick = new Button.ButtonClickedEvent();
        resetWepSellBtn.onClick.AddListener(new Action(() => __instance.SetTakeOutType((int)WepResetType.Sell)));

        // set button text
        var sellText = resetWepSell.transform.GetChild(0).GetComponent<OrangeText>();
        sellText.IsLocalizationText = true;
        sellText.LocalizationKey = "FUNCTION_SELL";
        sellText.text = LocalizationManager.Instance.GetStr("FUNCTION_SELL");

        // set button mask
        var sellToggle = resetWepSell.transform.GetChild(1).GetComponent<RectTransform>();
        sellToggle.name = "takeouttypemask5";
        sellToggle.gameObject.SetActive(false);

        //========================================

        // store button component references
        resetTypeToggle.Clear();
        resetTypeToggle.Add(resetWepLevel.transform.GetComponent<Button>());
        resetTypeToggle.Add(resetWepUpgrade.transform.GetComponent<Button>());
        resetTypeToggle.Add(resetWepSkill.transform.GetComponent<Button>());
        resetTypeToggle.Add(resetWepPassiveBtn);
        resetTypeToggle.Add(resetWepRankBtn);
        resetTypeToggle.Add(resetWepSellBtn);

        // add button mask to array
        Image[] _takeouttypemask = { __instance.takeouttypemask[0], __instance.takeouttypemask[1], __instance.takeouttypemask[2], passiveToggle.GetComponent<Image>(), rankToggle.GetComponent<Image>(), sellToggle.GetComponent<Image>() };
        __instance.takeouttypemask = (Il2CppReferenceArray<Image>)_takeouttypemask;
        #endregion
    }

    private static int typeIndexFromString(string componentName)
    {
        var sType = $"_{componentName[^1]}";
        if (Enum.TryParse(sType, out bitIndex idx))
            return (int)idx;
        else
            return 0;
    }

    private static int typeIndexFromInt(int nType)
    {
        return Int32.Parse(((bitIndex)nType).ToString().Substring(1));
    }

    private static void AddRtItem(ref List<WeaponInfoUI.expiteminfo> rtItems, ref WeaponInfoUI.expiteminfo tItem)
    {
        for (int i = 0; i < rtItems.Count; i++)
        {
            if (rtItems[i].tITEM_TABLE.n_ID == tItem.tITEM_TABLE.n_ID)
            {
                rtItems[i].nUseNum += tItem.nUseNum;
                return;
            }
        }
        rtItems.Add(tItem);
    }

    private static void UpdateToggles(WeaponInfoUI ui, int nType)
    {
        var isToggleEnabled = false;
        var typeMaskIndex = typeIndexFromInt(nType);
        
        if ((ui.nTakeOutType & nType) != 0)
        {
            ui.nTakeOutType &= ~nType;
            ui.takeouttypemask[typeMaskIndex].gameObject.SetActive(false);
            isToggleEnabled = false;
        }
        else
        {
            ui.nTakeOutType |= nType;
            ui.takeouttypemask[typeMaskIndex].gameObject.SetActive(true);
            isToggleEnabled = true;
        }

        if (nType == (int)WepResetType.Passive) 
            UpdateToggleLock(ui, isToggleEnabled, (int)WepResetType.Skill);
        if (nType == (int)WepResetType.Rank)
        {
            UpdateToggleLock(ui, isToggleEnabled, (int)WepResetType.Skill);
            UpdateToggleLock(ui, isToggleEnabled, (int)WepResetType.Passive);
        }
        if (nType == (int)WepResetType.Sell)
        {
            UpdateToggleLock(ui, isToggleEnabled, (int)WepResetType.Level);
            UpdateToggleLock(ui, isToggleEnabled, (int)WepResetType.Expert);
            UpdateToggleLock(ui, isToggleEnabled, (int)WepResetType.Skill);
            UpdateToggleLock(ui, isToggleEnabled, (int)WepResetType.Passive);
            UpdateToggleLock(ui, isToggleEnabled, (int)WepResetType.Rank);
        }
    }

    private static void UpdateToggleLock(WeaponInfoUI ui, bool isToggleEnabled, int nType)
    {
        var maskIndex = typeIndexFromInt(nType);
        if (!isToggleEnabled)
        {
            ui.nTakeOutType &= ~nType;
            ui.takeouttypemask[maskIndex].gameObject.SetActive(false);

            ui.takeouttypemask[maskIndex].color = Color.white;
            resetTypeToggle[maskIndex].interactable = true;
        }
        else
        {
            ui.nTakeOutType |= nType;
            ui.takeouttypemask[maskIndex].gameObject.SetActive(true);

            ui.takeouttypemask[maskIndex].color = new Color(1, 0.8f, 1, 0.9f);
            resetTypeToggle[maskIndex].interactable = false;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(WeaponInfoUI), nameof(WeaponInfoUI.SetTakeOutType))]
    private static bool SetTakeOutType_Prefix(WeaponInfoUI __instance, int nType)
    {
        if (nType == (int)WepResetType.Sell)
        {
            // prevent default weapons from being sold
            if (__instance.nTargetWeaponID == 100001 || __instance.nTargetWeaponID == 101001)
            {
                UIManager.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", new Action<CommonUI>((CommonUI ui) =>
                {
                    ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
                    ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
                    ui.SetupConfirmByKey("COMMON_TIP", "ERROR_SELL_DEFAULT_WEAPON", "COMMON_OK", new Action(() => { }));
                }), true, false);

                // do not run original code
                return false;
            }
        }    
        AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
        UpdateToggles(__instance, nType);

        int resetCost = 0;
        var list = new List<WeaponInfoUI.expiteminfo>();
        resetRewards = new();

        if ((__instance.nTakeOutType & (int)WepResetType.Level) != 0)
        {
            int exp = __instance.tWeaponInfo.netInfo.Exp;
            ITEM_TABLE item_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_WEAPON_TAKEOUT];
            int num2 = (int)item_TABLE.f_VALUE_X;
            if (exp > 0)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.nUseNum = (exp - exp % num2) / num2;
                if (exp % num2 > 0)
                    expiteminfo.nUseNum++;
                expiteminfo.tITEM_TABLE = item_TABLE;
                AddRtItem(ref list, ref expiteminfo);
                resetRewards.AddMaterial(ref resetRewards.LevelMaterials, OrangeConst.ITEMID_WEAPON_TAKEOUT, expiteminfo.nUseNum);
                resetCost += Configuration.ResetCost.Value;
            }
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Expert) != 0)
        {
            int upgradeZenny = 0;
            int upgradeProf = 0;
            bool hasReturnableItems = false;
            
            for (int i = 0; i < __instance.tWeaponInfo.netExpertInfos.Count; i++)
            {
                NetWeaponExpertInfo netWeaponExpertInfo = __instance.tWeaponInfo.netExpertInfos[i];
                if (netWeaponExpertInfo.ExpertLevel > 0)
                {
                    var upgrade_TABLE_DICT = new Dictionary<int, UPGRADE_TABLE>();
                    foreach (var kvp in OrangeDataManager.Instance.UPGRADE_TABLE_DICT)
                        upgrade_TABLE_DICT.Add(kvp.Key, kvp.Value);

                    var enumerable = upgrade_TABLE_DICT.Where((KeyValuePair<int, UPGRADE_TABLE> obj) => obj.Value.n_GROUP == __instance.tWEAPON_TABLE.n_UPGRADE && obj.Value.n_LV < (int)netWeaponExpertInfo.ExpertLevel);
                    for (int j = 0; j < enumerable.Count(); j++)
                    {
                        var keyValuePair = enumerable.ElementAt(j);
                        upgradeZenny += keyValuePair.Value.n_MONEY;
                        upgradeProf += keyValuePair.Value.n_PROF;
                    }
                }
            }
            if (upgradeZenny > 0)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_MONEY];
                expiteminfo.nUseNum = upgradeZenny;
                AddRtItem(ref list, ref expiteminfo);
                resetRewards.AddMaterial(ref resetRewards.ExpertMaterials, OrangeConst.ITEMID_MONEY, upgradeZenny);
                hasReturnableItems = true;
            }
            if (upgradeProf > 0)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_SHARE_PROF];
                expiteminfo.nUseNum = upgradeProf;
                AddRtItem(ref list, ref expiteminfo);
                resetRewards.AddMaterial(ref resetRewards.ExpertMaterials, OrangeConst.ITEMID_SHARE_PROF, upgradeProf);
                hasReturnableItems = true;
            }
            if (hasReturnableItems)
                resetCost += Configuration.ResetCost.Value;
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Skill) != 0)
        {
            int skillZenny = 0;
            int skillSP = 0;
            bool hasReturnableItems = false;
            
            var netSkillInfos = __instance.tWeaponInfo.netSkillInfos;
            if (netSkillInfos != null)
            {
                for (int k = 0; k < netSkillInfos.Count; k++)
                {
                    if (netSkillInfos[k].Level > 1)
                    {
                        for (int l = 1; l < netSkillInfos[k].Level; l++)
                        {
                            EXP_TABLE exp_TABLE = OrangeDataManager.Instance.EXP_TABLE_DICT[l];
                            skillZenny += exp_TABLE.n_SKILLUP_MONEY;
                            skillSP += exp_TABLE.n_SKILLUP_SP;
                        }
                    }
                }
                if (skillZenny > 0)
                {
                    var expiteminfo = new WeaponInfoUI.expiteminfo();
                    expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_MONEY];
                    expiteminfo.nUseNum = skillZenny;
                    AddRtItem(ref list, ref expiteminfo);
                    resetRewards.AddMaterial(ref resetRewards.SkillMaterials, OrangeConst.ITEMID_MONEY, skillZenny);
                    hasReturnableItems = true;
                }
                if (skillSP > 0)
                {
                    var expiteminfo = new WeaponInfoUI.expiteminfo();
                    expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_SKILL_POINT];
                    expiteminfo.nUseNum = skillSP;
                    AddRtItem(ref list, ref expiteminfo);
                    resetRewards.AddMaterial(ref resetRewards.SkillMaterials, OrangeConst.ITEMID_SKILL_POINT, skillSP);
                    hasReturnableItems = true;
                }
            }
            if (hasReturnableItems)
                resetCost += Configuration.ResetCost.Value;
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Passive) != 0)
        {
            int[] materialId = {
                __instance.tWEAPON_TABLE.n_PASSIVE_MATERIAL1,
                __instance.tWEAPON_TABLE.n_PASSIVE_MATERIAL2,
                __instance.tWEAPON_TABLE.n_PASSIVE_MATERIAL3,
                __instance.tWEAPON_TABLE.n_PASSIVE_MATERIAL4,
                __instance.tWEAPON_TABLE.n_PASSIVE_MATERIAL5,
                __instance.tWEAPON_TABLE.n_PASSIVE_MATERIAL6,
            };

            bool hasReturnableItems = false;
            var netSkillInfos = __instance.tWeaponInfo.netSkillInfos;
            if (netSkillInfos != null)
            {
                for (int k = 0; k < netSkillInfos.Count; k++)
                {
                    if (OrangeDataManager.Instance.MATERIAL_TABLE_DICT.TryGetValue(materialId[netSkillInfos[k].Slot-1], out MATERIAL_TABLE material_TABLE))
                        resetRewards.AddMaterials(ref resetRewards.PassiveMaterials, material_TABLE);
                } 
                if (__instance.tWeaponInfo.netDiveSkillInfo != null)
                {
                    if (OrangeDataManager.Instance.MATERIAL_TABLE_DICT.TryGetValue(__instance.tWEAPON_TABLE.n_DIVE_MATERIAL, out MATERIAL_TABLE material_TABLE))
                        resetRewards.AddMaterials(ref resetRewards.PassiveMaterials, material_TABLE);
                }
                foreach (var material in resetRewards.PassiveMaterials)
                {
                    var expiteminfo = new WeaponInfoUI.expiteminfo();
                    expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[material.Key];
                    expiteminfo.nUseNum = material.Value;
                    AddRtItem(ref list, ref expiteminfo);
                    hasReturnableItems = true;
                }       
                if (hasReturnableItems)
                    resetCost += Configuration.ResetCost.Value;
            }
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Rank) != 0)
        {
            var hasReturnableItems = false;
            foreach (var star_TABLE in OrangeDataManager.Instance.STAR_TABLE_DICT.Values)
            {
                if (star_TABLE.n_TYPE == 2 && star_TABLE.n_MAINID == __instance.tWeaponInfo.netInfo.WeaponID)
                {
                    if (star_TABLE.n_STAR + 1 <= __instance.tWeaponInfo.netInfo.Star)
                    {
                        if (OrangeDataManager.Instance.MATERIAL_TABLE_DICT.TryGetValue(star_TABLE.n_MATERIAL, out MATERIAL_TABLE material_TABLE))
                            resetRewards.AddMaterials(ref resetRewards.RankMaterials, material_TABLE);
                    }
                    if (star_TABLE.n_STAR + 1 == __instance.tWeaponInfo.netInfo.Star)
                        break;
                }
            }
            foreach (var material in resetRewards.RankMaterials)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[material.Key];
                expiteminfo.nUseNum = material.Value;
                AddRtItem(ref list, ref expiteminfo);
                hasReturnableItems = true;
            }
            if (hasReturnableItems)
                resetCost += Configuration.ResetCost.Value;
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Sell) != 0)
        {
            var expiteminfo = new WeaponInfoUI.expiteminfo();
            expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[__instance.tWEAPON_TABLE.n_UNLOCK_ID];
            expiteminfo.nUseNum = __instance.tWEAPON_TABLE.n_UNLOCK_COUNT;
            AddRtItem(ref list, ref expiteminfo);
            resetRewards.AddMaterial(ref resetRewards.SellMaterials, __instance.tWEAPON_TABLE.n_UNLOCK_ID, expiteminfo.nUseNum);
        }
        __instance.takeoutlbl[1].text = string.Format(LocalizationManager.Instance.GetStr("TAKEOUT_COST"), resetCost);
        
        int itemIdx = 0;
        while (itemIdx < list.Count && itemIdx < __instance.takeoutitems.Length)
        {
            __instance.UpdateItemNeedInfo(list[itemIdx].tITEM_TABLE, __instance.takeoutitems[itemIdx].BtnImgae, __instance.takeoutitems[itemIdx].frmimg, __instance.takeoutitems[itemIdx].bgimg, null);
            __instance.takeoutitems[itemIdx].BtnLabel.text = list[itemIdx].nUseNum.ToString();
            __instance.takeoutitems[itemIdx].Button.gameObject.SetActive(true);
            itemIdx++;
        }
        for (int m = list.Count; m < __instance.takeoutitems.Length; m++)
            __instance.takeoutitems[m].Button.gameObject.SetActive(false);
        __instance.TakeOutBtn.interactable = list.Count > 0 && PlayerHelper.Instance.GetTotalJewel() >= resetCost;

        // do run original code
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(WeaponInfoUI), nameof(WeaponInfoUI.TakeOutWeapon))]
    private static bool TakeOutWeapon_Prefix(WeaponInfoUI __instance)
    {
        if (__instance.nTakeOutType == 0)
            return false;

        var resetCost = 0;
        var netRewardsEntity = new NetRewardsEntity();

        if ((__instance.nTakeOutType & (int)WepResetType.Level) != 0)
        {
            var netWeaponInfo = WeaponService.Instance.GetWeapon(__instance.nTargetWeaponID);
            netWeaponInfo.Exp = 0;
            PlayerNetManager.Instance.dicWeapon[__instance.nTargetWeaponID].netInfo = netWeaponInfo;
            resetCost += Configuration.ResetCost.Value;
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Expert) != 0)
        {
            var netWeaponExpertInfos = WeaponService.Instance.GetWeaponExperts(__instance.nTargetWeaponID);
            foreach (var expertInfo in netWeaponExpertInfos)
                expertInfo.ExpertLevel = 0;
            PlayerNetManager.Instance.dicWeapon[__instance.nTargetWeaponID].netExpertInfos = netWeaponExpertInfos;
            resetCost += Configuration.ResetCost.Value;
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Skill) != 0)
        {
            var netWeaponSkillInfos = WeaponService.Instance.GetWeaponSkills(__instance.nTargetWeaponID);
            foreach (var skillInfo in netWeaponSkillInfos)
                skillInfo.Level = 1;
            PlayerNetManager.Instance.dicWeapon[__instance.nTargetWeaponID].netSkillInfos = netWeaponSkillInfos;
            resetCost += Configuration.ResetCost.Value;
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Passive) != 0)
        {
            // clear passive skills
            foreach (var skillInfo in WeaponService.Instance.GetWeaponSkills(__instance.nTargetWeaponID))
                WeaponService.Instance._listWeaponSkill.Remove(skillInfo);
            PlayerNetManager.Instance.dicWeapon[__instance.nTargetWeaponID].netSkillInfos.Clear();

            // clear hidden skill
            WeaponService.Instance._dicWeaponDiVESkill.Remove(__instance.nTargetWeaponID);
            PlayerNetManager.Instance.dicWeapon[__instance.nTargetWeaponID].netDiveSkillInfo = null;
            resetCost += Configuration.ResetCost.Value;
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Rank) != 0)
        {
            var netWeaponInfo = WeaponService.Instance.GetWeapon(__instance.nTargetWeaponID);
            netWeaponInfo.Star = 0;
            PlayerNetManager.Instance.dicWeapon[__instance.nTargetWeaponID].netInfo = netWeaponInfo;
            resetCost += Configuration.ResetCost.Value;
        }

        if ((__instance.nTakeOutType & (int)WepResetType.Sell) != 0)
        {
            // remove remaining weapon data
            foreach (var expertInfo in WeaponService.Instance.GetWeaponExperts(__instance.nTargetWeaponID))
                WeaponService.Instance._listWeaponExpert.Remove(expertInfo);

            // delete weapon from roster
            WeaponService.Instance._dicWeapon.Remove(__instance.nTargetWeaponID);
            PlayerNetManager.Instance.dicWeapon.Remove(__instance.nTargetWeaponID);

            // remove gallery info
            var weaponExp = 0;
            var wepGalleryInfo = new List<NetGalleryInfo>();
            foreach (var gallery in PlayerNetManager.Instance.galleryInfo.GalleryList)
            {
                if (OrangeDataManager.Instance.GALLERY_TABLE_DICT.TryGetValue(gallery.GalleryID, out GALLERY_TABLE gallery_TABLE))
                {
                    if (gallery_TABLE.n_TYPE == 2 && gallery_TABLE.n_MAINID == __instance.nTargetWeaponID)
                    {
                        wepGalleryInfo.Add(gallery);
                        weaponExp += gallery_TABLE.n_EXP;
                    }
                }
            }
            foreach (var galleryInfo in wepGalleryInfo)
            {
                GalleryService.Instance._dicGallery.Remove(galleryInfo.GalleryID);
                PlayerNetManager.Instance.galleryInfo.GalleryList.Remove(galleryInfo);
            }

            // subtract gallery EXP
            if (GalleryService.Instance._dicGalleryExp.TryGetValue(2, out NetGalleryExpInfo galleryExpInfo))
                galleryExpInfo.Exp -= weaponExp;
            foreach (var galleryInfo in PlayerNetManager.Instance.galleryInfo.GalleryExpList)
            {
                if (galleryInfo.GalleryType == 2)
                {
                    galleryInfo.Exp -= weaponExp;
                    if (galleryInfo.Exp < 0)
                        galleryInfo.Exp = 0;
                }
            }
        }

        // subtract reset cost
        var resetCostInfo = ItemService.Instance.GetItem(OrangeConst.ITEMID_FREE_JEWEL);
        resetCostInfo.Stack -= resetCost;
        PlayerNetManager.Instance.dicItem.Value(resetCostInfo.ItemID).netItemInfo = resetCostInfo;

        // create reward items
        resetRewards.ComposeRewards(ref netRewardsEntity);

        // send rewards
        if (netRewardsEntity.RewardList != null && netRewardsEntity.RewardList.Count > 0)
        {
            PlayerNetManager.Instance.ComposeRewardEntities(netRewardsEntity);
            UIManager.Instance.LoadUI<RewardPopopUI>("UI_RewardPopup", new Action<RewardPopopUI>((RewardPopopUI ui) =>
            {
                ui.Setup(netRewardsEntity.RewardList, 0f);
            }));
        }
        RefreshUI();

        AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
        __instance.CloseTakeoutDetailPopup();

        // do run original code
        return false;
    }

    private static void RefreshUI()
    {
        // get active UI instances
        var mainUI = UIManager.Instance.GetUI<WeaponMainUI>("UI_WEAPONMAIN");
        var infoUI = UIManager.Instance.GetUI<WeaponInfoUI>("UI_WEAPONINFO");
        var goCheckUI = UIManager.Instance.GetUI<GoCheckUI>("UI_GoCheck");

        // refresh weapon model
        if ((infoUI.nTakeOutType & (int)WepResetType.Sell) != 0)
        {
            if (infoUI.nTargetWeaponID == PlayerNetManager.Instance.playerInfo.netPlayerInfo.MainWeaponID)
                OrangeGameManager.Instance.WeaponWield(100001, WeaponWieldType.MainWeapon, (Callback)new Action(() => { }), true);
            if (infoUI.nTargetWeaponID == PlayerNetManager.Instance.playerInfo.netPlayerInfo.SubWeaponID)
                OrangeGameManager.Instance.WeaponWield(101001, WeaponWieldType.SubWeapon, (Callback)new Action(() => { }), true);
            if (goCheckUI != null)
            {
                for (int i = 0; i < infoUI.listHasWeapons.Count; i++)
                {
                    if (infoUI.listHasWeapons[i] == infoUI.nTargetWeaponID)
                    {
                        if (i+1 < infoUI.listHasWeapons.Count)
                            infoUI.nTargetWeaponID = infoUI.listHasWeapons[i + 1];
                        else
                            infoUI.nTargetWeaponID = infoUI.listHasWeapons[i - 1];
                        break;
                    }
                }
            }
        }

        // sort weapons
        infoUI.SortWeaponList();
        infoUI.listHasWeapons = EquipHelper.Instance.GetUnlockedWeaponList();
        infoUI.listFragWeapons = EquipHelper.Instance.GetFragmentWeaponList();
        RefreshQuickSelectBar(infoUI);

        // refresh weapon data
        infoUI.nNowWeaponID = -1;
        infoUI.InitWeapon();

        // refresh main UI
        if (mainUI != null )
            mainUI.ReFresh();

        // refresh topbar
        GenericEventManager.Instance.NotifyEvent(EventManager.ID.UPDATE_TOPBAR_DATA);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(WeaponInfoUI), nameof(WeaponInfoUI.CloseTakeoutDetailPopup))]
    private static bool CloseTakeoutDetailPopup_Prefix(WeaponInfoUI __instance)
    {
        __instance.ObjScale(1f, 0f, 0.2f, __instance.takeoutroot, new Action(() =>
        {
            __instance.takeoutroot.SetActive(false);
            
            foreach (var toggle in resetTypeToggle)
                toggle.interactable = true;
            foreach (var mask in __instance.takeouttypemask)
                mask.color = Color.white;
        }));

        // do not run original code
        return false;
    }

    private static void RefreshQuickSelectBar(WeaponInfoUI ui)
    {
        int iconCount = 10;
        bool flag = false;
        ui.tLHSR.totalCount = ui.listHasWeapons.Count + ui.listFragWeapons.Count;
        for (int i = ui.listHasWeapons.Count - 1; i >= 0; i--)
        {
            if (ui.nTargetWeaponID == ui.listHasWeapons[i])
            {
                int iconIdx = (EquipHelper.Instance.WeaponSortDescend == 0) ? (i + ui.listFragWeapons.Count) : i;
                if (ui.tLHSR.totalCount > iconCount && iconIdx >= ui.tLHSR.totalCount - iconCount)
                    ui.tLHSR.RefillCells(ui.tLHSR.totalCount - iconCount);
                else
                    ui.tLHSR.RefillCells(iconIdx);
                flag = true;
                break;
            }
        }
        for (int i = ui.listFragWeapons.Count - 1; i >= 0; i--)
        {
            if (ui.nTargetWeaponID == ui.listFragWeapons[i])
            {
                int iconIdx = (EquipHelper.Instance.WeaponSortDescend == 1) ? (i + ui.listHasWeapons.Count) : i;
                if (ui.tLHSR.totalCount > iconCount && iconIdx >= ui.tLHSR.totalCount - iconCount)
                    ui.tLHSR.RefillCells(ui.tLHSR.totalCount - iconCount);
                else
                    ui.tLHSR.RefillCells(iconIdx);
                flag = true;
                break;
            }
        }
        if (!flag)
            ui.tLHSR.RefillCells(0);
    }
}
