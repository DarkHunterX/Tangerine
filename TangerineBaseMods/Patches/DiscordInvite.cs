using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TangerineBaseMods.Patches;

internal static class DiscordInvite
{   
    internal static void InitializeHarmony(Harmony harmony)
    {
        harmony.PatchAll(typeof(DiscordInvite));
    }

    private static void OnClickBtnDiscordInvite()
    {
        UIManager.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", new Action<CommonUI>((CommonUI ui) =>
        {
            ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP;
            ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
            ui.YesSE = SystemSE.CRI_SYSTEMSE_SYS_OK17;

            ui.SetupConfirmByKey("COMMON_TIP", "MESSAGE_DISCORD_INVITE", "COMMON_OK", new Action(() => {} ));
        }), true, false);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(TitleNewUI), nameof(TitleNewUI.Awake))]
    private static void Setup(TitleNewUI __instance)
    {
        var mainTrans = __instance.btnExitGame.GetComponentInParent<Transform>().GetParent();
        Transform[] componentsInChildren = mainTrans.transform.GetComponentsInChildren<Transform>(true);
        foreach (var component in componentsInChildren)
        {     
            if (component.name == "BtnLanguage")
            {
                var temp = CopyTransform(component.gameObject);
                temp.name = "btnDiscord";
                temp.transform.position = new Vector3(temp.transform.position.x, temp.transform.position.y - 10.0f, temp.transform.position.z);

                var text = temp.transform.GetChild(0).GetComponent<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = "FUNCTION_DISCORD";
                text.text = LocalizationManager.Instance.GetStr("FUNCTION_DISCORD");

                var btn = temp.transform.GetComponent<Button>();
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(new Action(OnClickBtnDiscordInvite));
            }
        }
    }

    private static GameObject CopyTransform(GameObject source, int childPosition = -1)
    {
        var NewObj = GameObject.Instantiate(source);
        NewObj.transform.SetParent(source.transform.parent);
        if (childPosition != -1) NewObj.transform.SetSiblingIndex(childPosition);
        NewObj.transform.position = source.transform.position;
        NewObj.transform.rotation = source.transform.rotation;
        NewObj.transform.localScale = source.transform.localScale;
        return NewObj;
    }
}