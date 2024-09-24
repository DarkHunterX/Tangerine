using HarmonyLib;
using Il2CppSystem;
using CallbackDefs;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tangerine.Manager.Mod;
using UnityEngine.UI;

namespace TangerineBaseMods;

public class EventSkip
{
    private static int maxMultiSweepCount = 10000;
    private static int sweepCount = 0;

    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony, JsonNode node)
    {
        if (node["EventSkip"]["enabled"].Deserialize<bool>())
        {
            harmony.PatchAll(typeof(EventSkip));
            sweepCount = node["EventSkip"]["sweep count"].Deserialize<int>();
            if (sweepCount > maxMultiSweepCount)
                sweepCount = maxMultiSweepCount;
            if (sweepCount <= 1)
                sweepCount = 1;
        }
        else
            tangerine.Loader.RemoveAssetBundleId("ui/ui_eventstage");
    }


    [HarmonyPostfix, HarmonyPatch(typeof(EventStageMain), nameof(EventStageMain.EventTabHelper))]
    private static void EnableSweepBtn_TabStart(EventStageMain __instance)
    {
        EnableSweepBtn_Normal(__instance);
        __instance.m_btnSweep.GetComponentInChildren<Text>().text = string.Format(LocalizationManager.Instance.GetStr("FUNTION_MULTI_SWEEP"), sweepCount);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(EventStageMain), nameof(EventStageMain.OnClickNormalToggle))]
    private static void EnableSweepBtn_Normal(EventStageMain __instance)
    {
        if (__instance.m_currentSelectedTab == EventStageMain.TabType.TIMELIMITED)
            __instance.m_btnSweep.gameObject.SetActive(true);
        else if (__instance.m_currentSelectedTab == EventStageMain.TabType.BOSSRUSH)
            __instance.m_btnSweep.gameObject.SetActive(false);
        else
            __instance.m_btnSweep.gameObject.SetActive(true);
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
}