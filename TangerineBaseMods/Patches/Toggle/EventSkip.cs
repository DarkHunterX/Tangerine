using CallbackDefs;
using HarmonyLib;
using Il2CppSystem;
using UnityEngine;
using UnityEngine.UI;
using TangerineBaseMods.Config;

namespace TangerineBaseMods;

public class EventSkip
{
    private static int maxMultiSweepCount = 10000;
    private static int sweepCount = 0;
    private static EventStageMain _instance;
    private static bool isEventStageMain = false;

    internal static void InitializeHarmony(Harmony harmony)
    {
        harmony.PatchAll(typeof(EventSkip));
        Plugin.RemoveObsoleteMod_RestoredFunctions();
        SetConfigDelegates();
    }

    internal static void SetConfigDelegates()
    {
        Configuration.EventSkip.SettingChanged += (sender, args) => UpdateSweepBtnVisibility();
        Configuration.EventSkipCount.SettingChanged += (sender, args) => UpdateSweepBtn();
    }

    internal static void UpdateSweepBtnVisibility()
    {
        if (isEventStageMain && _instance.m_currentMode == EventStageMain.ModeType.NONE || _instance.m_currentMode == EventStageMain.ModeType.NORMAL)
        {
            if (Configuration.EventSkip.Value)
                EnableSweepButton(_instance);
            else
                DisableSweepButton(_instance);
        }
    }

    internal static void UpdateSweepBtn()
    {
        if (isEventStageMain)
        {
            UpdateSweepCount();
            _instance.m_btnSweep.GetComponentInChildren<Text>().text = string.Format(LocalizationManager.Instance.GetStr("FUNTION_MULTI_SWEEP"), sweepCount);
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(EventStageMain), nameof(EventStageMain.Setup))]
    private static void Setup_Prefix(EventStageMain __instance)
    {
        isEventStageMain = true;
        _instance = __instance;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(EventStageMain), nameof(EventStageMain.EventTabHelper))]
    private static void EventTabHelper_Postfix(EventStageMain __instance)
    {
        UpdateSweepCount();
        __instance.m_btnSweep.GetComponentInChildren<Text>().text = string.Format(LocalizationManager.Instance.GetStr("FUNTION_MULTI_SWEEP"), sweepCount);
        
        if (Configuration.EventSkip.Value)
            EnableSweepButton(__instance);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(EventStageMain), nameof(EventStageMain.OnClickNormalToggle))]
    private static void OnClickNormalToggle_Postfix(EventStageMain __instance)
    {
        if (Configuration.EventSkip.Value)
            EnableSweepButton(__instance);
    }


    [HarmonyPrefix, HarmonyPatch(typeof(EventStageMain), nameof(EventStageMain.OnClickSweep))]
    private static bool OnClickSweep(EventStageMain __instance)
    {
        if (!__instance.CheckBeforeStart())
        {
            // do not run original code
            return false;
        }
        if (OrangeBattleServerManager.Instance.CheckCardCountMax())
        {
            // do not run original code
            return false;
        }
        if (EquipHelper.Instance.ShowEquipmentLimitReachedDialog())
        {
            // do not run original code
            return false;
        }

        // check if stage was beaten before
        bool flag = false;
        if (__instance.m_stageInfo != null)
            flag = true;
        if (!flag)
        {
            string errorMsg = LocalizationManager.Instance.GetStr("SWEEP_CORP_RESTRICT");
            UIManager.Instance.LoadUI<TipUI>("UI_Tip", new System.Action<TipUI>((TipUI tipUI) =>
            {
                tipUI.Setup(errorMsg, true);
            }));
            // do not run original code
            return false;
        }

        AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
        OrangeGameManager.Instance.StageSweepReq(__instance.m_currentStageTable.n_ID, sweepCount, new System.Action<Il2CppSystem.Object>((Il2CppSystem.Object res) =>
        {
            NetRewardsEntity reward = res.Cast<NetRewardsEntity>();
            if (reward.RewardList.Count > 0)
            {
                UIManager.Instance.LoadUI<RewardPopopUI>("UI_RewardPopup", new System.Action<RewardPopopUI>((RewardPopopUI ui) =>
                {
                    if (OrangeGameManager.Instance.IsLvUp)
                    {
                        ui.closeCB = Delegate.Combine(ui.closeCB, (Callback)new System.Action(() =>
                        {
                            OrangeGameManager.Instance.DisplayLvPerform(null);
                        })).Cast<Callback>();
                    }
                    ui.Setup(reward.RewardList, 0f);
                }));
            }
            else
                OrangeGameManager.Instance.DisplayLvPerform(null);
        }));

        // update event points
        __instance.UpdateEnergyValue();
        __instance.UpdateChallengeCount();
        if (__instance.m_currentSelectedTab == EventStageMain.TabType.TIMELIMITED)
            __instance.UpdateRankingInfoTimeLimited(__instance.GetSelectedTimedEventTable());

        // update difficulty bar
        __instance.m_currentPlayerLV = PlayerHelper.Instance.GetLV();
        __instance.RefreshDifficultyMeter();

        // do not run original code
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(EventStageMain), nameof(EventStageMain.OnClickCloseBtn))]
    private static void OnClickCloseBtn_Postfix(EventStageMain __instance)
    {
        isEventStageMain = false;
    }

    internal static void UpdateSweepCount()
    {
        sweepCount = Configuration.EventSkipCount.Value;
        if (sweepCount > maxMultiSweepCount)
            sweepCount = maxMultiSweepCount;
        if (sweepCount <= 1)
            sweepCount = 1;
    }

    internal static void EnableSweepButton(EventStageMain __instance)
    {
        var btnSweepRect = __instance.m_btnSweep.gameObject.GetComponentInChildren<RectTransform>(true);
        btnSweepRect.anchoredPosition = new Vector2(-250, 38);

        if (__instance.m_currentSelectedTab == EventStageMain.TabType.TIMELIMITED)
            __instance.m_btnSweep.gameObject.SetActive(true);
        else if (__instance.m_currentSelectedTab == EventStageMain.TabType.BOSSRUSH)
            __instance.m_btnSweep.gameObject.SetActive(false);
        else
            __instance.m_btnSweep.gameObject.SetActive(true);
    }

    internal static void DisableSweepButton(EventStageMain __instance)
    {
        __instance.m_btnSweep.gameObject.SetActive(false);
    }
}