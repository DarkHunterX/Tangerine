using HarmonyLib;
using Il2CppSystem;
using CallbackDefs;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tangerine.Manager.Mod;

namespace TangerineBaseMods;

public class CardLoadout
{
    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony, JsonNode node)
    {
        if (node["CardLoadout"]["enabled"].Deserialize<bool>())
        {
            harmony.PatchAll(typeof(CardLoadout));
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CardDeployGoCheck), nameof(CardDeployGoCheck.OnClickDeployBtn))]
    private static bool UI_UpdateLoadoutName_SideBar_Deploy_Click(CardDeployGoCheck __instance, ref int idx)
    {
        __instance.CurrentDeployIndex = idx;
        if (__instance.bIsDeploy && !__instance.bIsSetDeploy)
        {
            string arg = "------";
            if (PlayerNetManager.Instance.dicCardDeployNameInfo.ContainsKey(__instance.CurrentDeployIndex))
            {
                string text = PlayerNetManager.Instance.dicCardDeployNameInfo[__instance.CurrentDeployIndex];
                if (text != null && text != "")
                {
                    arg = PlayerNetManager.Instance.dicCardDeployNameInfo[__instance.CurrentDeployIndex];
                }
            }
            string msg = string.Format(LocalizationManager.Instance.GetStr("CARD_DEPLOY_SURE"), arg);

            UIManager.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", new System.Action<CommonUI>((CommonUI ui) =>
            {
                ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
                ui.SetupYesNO(LocalizationManager.Instance.GetStr("ARMSSKILL_SURE"), msg, LocalizationManager.Instance.GetStr("COMMON_YES"), LocalizationManager.Instance.GetStr("COMMON_CANCEL"), new System.Action(() =>
                {
                    __instance.OnEquipDeploy();
                }), null);
                ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(__instance.ResetCurrentDeployIndex)).Cast<Callback>();
            }), false, false);

            // do not run original code
            return false;
        }
        __instance.PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01, null, false);
        __instance.Setup(new CharacterInfo
        {
            netInfo = new NetCharacterInfo()
            {
                CharacterID = 1,
                Star = 5,
                State = 1
            }
        }, __instance.bIsDeploy, __instance.bIsSetDeploy);
        __instance.OnDeployBackup();

        // do not run original code
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CardDeployCell), nameof(CardDeployCell.OnUpdateName))]
    private static bool UI_UpdateLoadoutName_SideBar_Character(CardDeployCell __instance)
    {
        __instance.BtnText.text = "------";
        int num = __instance.pid + 1;
        //__instance.BtnText.text = String.Format(LocalizationManager.Instance.GetStr("CARD_DEPLOY_LIST"), num);

        if (PlayerNetManager.Instance.dicCardDeployNameInfo.ContainsKey(num))
        {
            string text = PlayerNetManager.Instance.dicCardDeployNameInfo[num];
            if (text != null && text != "")
            {
                __instance.BtnText.text = PlayerNetManager.Instance.dicCardDeployNameInfo[num];
            }
        }
        // do not run original code
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CardDeploySetCell), nameof(CardDeploySetCell.OnUpdateName))]
    private static bool UI_UpdateLoadoutName_SideBar_Menu(CardDeploySetCell __instance)
    {
        __instance.BtnText.text = "------";
        int num = __instance.pid + 1;
        //__instance.BtnText.text = String.Format(LocalizationManager.Instance.GetStr("CARD_DEPLOY_LIST"), num);

        if (PlayerNetManager.Instance.dicCardDeployNameInfo.ContainsKey(num))
        {
            string text = PlayerNetManager.Instance.dicCardDeployNameInfo[num];
            if (text != null && text != "")
            {
                __instance.BtnText.text = PlayerNetManager.Instance.dicCardDeployNameInfo[num];
            }
        }
        // do not run original code
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CardDeployGoCheckCell), nameof(CardDeployGoCheckCell.OnUpdateName))]
    private static bool UI_UpdateLoadoutName_SideBar_Deploy(CardDeployGoCheckCell __instance)
    {
        __instance.BtnText.text = "------";
        int num = __instance.pid + 1;
        //__instance.BtnText.text = String.Format(LocalizationManager.Instance.GetStr("CARD_DEPLOY_LIST"), num);

        if (PlayerNetManager.Instance.dicCardDeployNameInfo.ContainsKey(num))
        {
            string text = PlayerNetManager.Instance.dicCardDeployNameInfo[num];
            if (text != null && text != "")
            {
                __instance.BtnText.text = PlayerNetManager.Instance.dicCardDeployNameInfo[num];
            }
        }
        // do not run original code
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CardDeployMain), nameof(CardDeployMain.Setup))]
    private static void UI_UpdateLoadoutName_Menu(CardDeployMain __instance)
    {
        // enable rename button
        __instance.BtnRename.gameObject.SetActive(__instance.CurrentDeployIndex != -1);

        // display loadout names
        if (__instance.CurrentDeployIndex != -1)
        {
            string text = "------";
            //string text = String.Format(LocalizationManager.Instance.GetStr("CARD_DEPLOY_LIST"), __instance.CurrentDeployIndex);
            if (PlayerNetManager.Instance.dicCardDeployNameInfo.ContainsKey(__instance.CurrentDeployIndex))
            {
                string text2 = PlayerNetManager.Instance.dicCardDeployNameInfo[__instance.CurrentDeployIndex];
                if (text2 != null && text2 != "")
                {
                    text = PlayerNetManager.Instance.dicCardDeployNameInfo[__instance.CurrentDeployIndex];
                }
            }
            __instance.CurrentTargetDeployText.text = text;
        }
        return;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CardDeployMain), nameof(CardDeployMain.OnChangeDeployName))]
    private static bool UI_InputLoadoutName(CardDeployMain __instance)
    {
        UIManager.Instance.LoadUI<InputTextUI>("UI_InputText", new System.Action<InputTextUI>((InputTextUI ui) =>
        {
            ui.SetupCardDeploy(new System.Action<string>(__instance.ChangeDeployName), LocalizationManager.Instance.GetStr("CARD_DEPLOY_NAME"), __instance.CurrentTargetDeployText.text, 10);
        }));
        // do not run original code
        return false;
    }


    // CardDeployMain.OnChangeDeployName()
    [HarmonyPrefix, HarmonyPatch(typeof(CharacterInfoCard), nameof(CharacterInfoCard.OnClickDeployBtn))]
    private static bool UI_UpdateLoadoutName_SideBar_Main_Click(CharacterInfoCard __instance, int idx)
    {
        __instance.CurrentDeployIndex = idx;
        if (__instance.bIsDeploy && !__instance.bIsSetDeploy)
        {
            string text = "------";
            if (PlayerNetManager.Instance.dicCardDeployNameInfo.ContainsKey(__instance.CurrentDeployIndex))
            {
                string text2 = PlayerNetManager.Instance.dicCardDeployNameInfo[__instance.CurrentDeployIndex];
                if (text2 != null && text2 != "")
                {
                    text = PlayerNetManager.Instance.dicCardDeployNameInfo[__instance.CurrentDeployIndex];
                }
            }
            string msg = string.Format(LocalizationManager.Instance.GetStr("CARD_DEPLOY_SURE"), text);
            UIManager.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", new System.Action<CommonUI>((CommonUI ui) =>
            {
                ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;
                ui.SetupYesNO(LocalizationManager.Instance.GetStr("ARMSSKILL_SURE"), msg, LocalizationManager.Instance.GetStr("COMMON_YES"), LocalizationManager.Instance.GetStr("COMMON_CANCEL"), new System.Action(() =>
                {
                    __instance.OnEquipDeploy();
                }), null);
                ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(__instance.ResetCurrentDeployIndex)).Cast<Callback>();
            }), false, false);
            // do not run original code
            return false;
        }
        __instance.PlayUISE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01, null, false);
        __instance.Setup(new CharacterInfo
        {
            netInfo = new NetCharacterInfo()
            {
                CharacterID = 1,
                Star = 5,
                State = 1
            }
        }, __instance.bIsDeploy, __instance.bIsSetDeploy);
        __instance.OnDeployBackup();

        // do not run original code
        return false;
    }
}