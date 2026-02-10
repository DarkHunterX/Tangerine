using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.UI;
using Tangerine.Utils;

namespace TangerineBaseMods.Patches;

internal static class DiscordInvite
{   
    internal static void InitializeHarmony(Harmony harmony)
    {
        harmony.PatchAll(typeof(DiscordInvite));
    }

    private static void OnClickBtnDiscordInvite()
    {
        Application.OpenURL("https://discord.gg/5DRhJh9wRq");
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
                var temp = UIHelpers.CopyGameObject(component.gameObject);
                temp.name = "btnDiscord";
                temp.transform.position = new Vector3(temp.transform.position.x, temp.transform.position.y - 10.0f, temp.transform.position.z);

                var text = temp.transform.GetChild(0).GetComponent<OrangeText>();
                text.IsLocalizationText = false;
                text.text = "Discord";

                var btn = temp.transform.GetComponent<Button>();
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(new Action(OnClickBtnDiscordInvite));
            }
        }
    }
}