using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Tangerine.Manager.Mod;
using TangerineBaseMods.Config;

namespace TangerineBaseMods;

public class StorySkip
{
    private static int maxMultiSweepCount = 10000;
    private static int sweepCount = 0;
    private static UI_ChallengePopup _instance;
    private static bool ischallengePopupUI = false;

    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony)
    {
        harmony.PatchAll(typeof(StorySkip));
        Plugin.RemoveObsoleteMod_RestoredFunctions();
        SetConfigDelegates();
    }

    internal static void SetConfigDelegates()
    {
        Configuration.StorySkip.SettingChanged += (sender, args) => UpdateSweepBtnVisibility();
        Configuration.StorySkipCount.SettingChanged += (sender, args) => UpdateSweepBtn();
    }

    internal static void UpdateSweepBtnVisibility()
    {
        if (ischallengePopupUI && _instance.NormalRoot.active)
        {
            if (Configuration.StorySkip.Value)
                EnableSweepButton(_instance);
            else
                DisableSweepButton(_instance);
        }
    }

    internal static void UpdateSweepBtn()
    {
        if (ischallengePopupUI)
        {
            UpdateSweepCount(_instance);
            _instance.SetTextInfoByClear(_instance.hasClearData);
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(UI_ChallengePopup), nameof(UI_ChallengePopup.Setup))]
    private static void Setup_Prefix(UI_ChallengePopup __instance)
    {
        ischallengePopupUI = true;
        _instance = __instance;       
        UpdateSweepCount(__instance);

        if (Configuration.StorySkip.Value && __instance.NormalRoot.active)
            EnableSweepButton(__instance);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(UI_ChallengePopup), nameof(UI_ChallengePopup.OnClickSweep))]
    private static void OnClickSweep_Prefix(UI_ChallengePopup __instance, ref int count)
    {
        UpdateSweepCount(__instance);
        if (count == 10)
            count = sweepCount;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(UI_ChallengePopup), nameof(UI_ChallengePopup.OnClickCloseBtn))]
    private static void OnClickCloseBtn_Postfix(UI_ChallengePopup __instance)
    {
        ischallengePopupUI = false;
    }

    internal static void UpdateSweepCount(UI_ChallengePopup __instance)
    {
        sweepCount = Configuration.StorySkipCount.Value;
        if (sweepCount > maxMultiSweepCount)
            sweepCount = maxMultiSweepCount;
        if (sweepCount <= 1)
            sweepCount = 1;
        __instance.maxMultiSweepCount = sweepCount;
    }

    internal static void EnableSweepButton(UI_ChallengePopup __instance)
    {
        var btnSweep = __instance.textSweepMulti.GetComponentsInParent<Button>(true)[0];
        var mainTrans = btnSweep.GetComponentInParent<Transform>().GetParent();
        Transform[] componentsInChildren = mainTrans.transform.GetComponentsInChildren<Transform>(true);
        foreach (var component in componentsInChildren)
        {
            if (component.name == "BtnPlay")
            {
                // move "Execute" button over a little bit
                var btnPlayRect = component.transform.GetComponentInChildren<RectTransform>(true);
                Plugin.Log.LogInfo($"execute pos = {btnPlayRect.anchoredPosition}");
                btnPlayRect.anchoredPosition = new Vector2((float)415.6, (float)-328.7);

                // enable skip dive button and move it into position
                var btnSweepRect = btnSweep.transform.GetComponentInChildren<RectTransform>(true);
                btnSweepRect.anchoredPosition = new Vector2(70, -330);
                btnSweep.gameObject.SetActive(true);
            }
        }
    }

    internal static void DisableSweepButton(UI_ChallengePopup __instance)
    {
        var btnSweep = __instance.textSweepMulti.GetComponentsInParent<Button>(true)[0];
        var mainTrans = btnSweep.GetComponentInParent<Transform>().GetParent();
        Transform[] componentsInChildren = mainTrans.transform.GetComponentsInChildren<Transform>(true);
        foreach (var component in componentsInChildren)
        {
            if (component.name == "BtnPlay")
            {
                // move "Execute" button back to original position
                var btnPlayRect = component.transform.GetComponentInChildren<RectTransform>(true);
                btnPlayRect.anchoredPosition = new Vector2((float)381.6, (float)-328.7);

                // disable skip dive button
                btnSweep.gameObject.SetActive(false);
            }
        }
    }

}