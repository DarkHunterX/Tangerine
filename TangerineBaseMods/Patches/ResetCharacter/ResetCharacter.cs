using HarmonyLib;
using System;
using Tangerine.Manager.Mod;
using Tangerine.Utils;
using TangerineBaseMods.Config;
using TangerineBaseMods.Patches;
using UnityEngine;
using UnityEngine.UI;

namespace TangerineBaseMods;

public class ResetCharacter
{
    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony)
    {
        harmony.PatchAll(typeof(ResetCharacter));
        tangerine.UI.AddController("UI_CharacterInfo_Reset", typeof(CharacterInfoResetUI), null, true, null);
        SetConfigDelegates();
    }

    private static void SetConfigDelegates()
    {
        Configuration.CharacterIntroViewer.SettingChanged += (sender, args) => UpdateResetBtnPosition();
    }

    private static Button openResetUIBtn;

    private static void UpdateResetBtnPosition()
    {
        if (Configuration.CharacterIntroViewer.Value)
            openResetUIBtn.transform.position = new Vector3(openResetUIBtn.transform.position.x - 10f, openResetUIBtn.transform.position.y, openResetUIBtn.transform.position.z);
        else
            openResetUIBtn.transform.position = new Vector3(openResetUIBtn.transform.position.x + 10f, openResetUIBtn.transform.position.y, openResetUIBtn.transform.position.z);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CharacterInfoBasic), nameof(CharacterInfoBasic.Setup), new[] { typeof(CharacterInfo) })]
    private static void Setup_Postfix(CharacterInfoBasic __instance)
    {
        CreateResetBtn();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CharacterInfoBasic), nameof(CharacterInfoBasic.OnClickUnlockBtn))]
    private static void OnClickUnlockBtn_Postfix(CharacterInfoBasic __instance)
    {
        CreateResetBtn();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CharacterInfoBasic), nameof(CharacterInfoBasic.RefreshUnlockGroup))]
    private static void RefreshUnlockGroup_Postfix(CharacterInfoBasic __instance)
    {
        var basicUI = UIManager.Instance.GetUI<CharacterInfoBasic>("UI_CharacterInfo_Basic");
        
        var mainTrans = basicUI.btnDeploy.GetComponentInParent<Transform>().GetParent();
        Transform[] componentsInChildren = mainTrans.transform.GetComponentsInChildren<Transform>(true);
        foreach (var component in componentsInChildren)
        {
            if (component.name == "btnReset")
            {
                component.gameObject.SetActive(basicUI.characterUnlocked);
                break;
            }
        }
    }

    private static void CreateResetBtn()
    {
        var basicUI = UIManager.Instance.GetUI<CharacterInfoBasic>("UI_CharacterInfo_Basic");
        if (basicUI == null || basicUI.characterInfo.netInfo.State == 2)
            return;

        var mainTrans = basicUI.btnDeploy.GetComponentInParent<Transform>().GetParent();
        Transform[] componentsInChildren = mainTrans.transform.GetComponentsInChildren<Transform>(true);
        foreach (var component in componentsInChildren)
        {
            if (component.name == "Btn2DSwitch")
            {
                var shiftPos = 10f;
                if (Configuration.CharacterIntroViewer.Value)
                    shiftPos = 20f;

                var temp = UIHelpers.CopyGameObject(component.gameObject, component.transform.GetSiblingIndex() - 1);
                temp.name = "btnReset";
                temp.transform.position = new Vector3(temp.transform.position.x - shiftPos, temp.transform.position.y, temp.transform.position.z);

                var text = temp.transform.GetChild(0).GetComponent<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = "COMMON_RESET";
                text.text = LocalizationManager.Instance.GetStr("COMMON_RESET");

                openResetUIBtn = temp.transform.GetComponent<Button>();
                openResetUIBtn.onClick = new Button.ButtonClickedEvent();
                openResetUIBtn.onClick.AddListener(new Action(OnClickResetBtn));
                break;
            }
        }
    }

    private static void OnClickResetBtn()
    {
        var basicUI = UIManager.Instance.GetUI<CharacterInfoBasic>("UI_CharacterInfo_Basic");
        if (basicUI == null)
            return;

        AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
        UIHelpers.LoadUI<CharacterInfoResetUI>("UI_CharacterInfo_Reset", new Action<OrangeUIBase>((OrangeUIBase ui) => 
        {
            ui.hasBlackBg = true;
            ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
            ui.effectTypeOpen = UIManager.EffectType.EXPAND;
            ui.effectTypeClose = UIManager.EffectType.EXPAND;
            ui._EscapeEvent = OrangeUIBase.EscapeEvent.CLOSE_UI;
        }), new Action<CharacterInfoResetUI>((CharacterInfoResetUI ui) => 
        {
            ui.Setup(basicUI.characterInfo);
        }));
    }
}

