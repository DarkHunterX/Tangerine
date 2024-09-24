using HarmonyLib;

namespace TangerineBaseMods.Patches
{
    internal class CheatEngineFix
    {
        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(CheatEngineFix));
            Plugin.RemoveObsoleteMod_FixSavingCrash();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StandaloneAntiCheat), nameof(StandaloneAntiCheat.IsCheatEngineRunning))]
        static void IsCheatEngineRunningPostfix(ref bool __result)
        {
            __result = false;
        }
    }
}
