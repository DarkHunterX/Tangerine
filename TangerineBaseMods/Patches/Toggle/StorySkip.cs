using HarmonyLib;
using Tangerine.Manager.Mod;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TangerineBaseMods;

public class StorySkip
{
    private static int maxMultiSweepCount = 10000;
    private static int sweepCount = 0;

    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony, JsonNode node)
    {
        if (node["StorySkip"]["enabled"].Deserialize<bool>())
        {
            harmony.PatchAll(typeof(StorySkip));
            Plugin.RemoveObsoleteMod_RestoredFunctions();

            sweepCount = node["StorySkip"]["sweep count"].Deserialize<int>();
            if (sweepCount > maxMultiSweepCount)
                sweepCount = maxMultiSweepCount;
            if (sweepCount <= 1)
                sweepCount = 1;
        }
        else
            tangerine.Loader.RemoveAssetBundleId("ui/ui_challengepopup");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(UI_ChallengePopup), nameof(UI_ChallengePopup.Setup))]
    private static void Setup_Prefix(UI_ChallengePopup __instance)
    {
        __instance.maxMultiSweepCount = sweepCount;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(UI_ChallengePopup), nameof(UI_ChallengePopup.OnClickSweep))]
    private static void OnClickSweep_Prefix(UI_ChallengePopup __instance, ref int count)
    {
        __instance.maxMultiSweepCount = sweepCount;
        if (count == 10)
            count = sweepCount;
    }
}