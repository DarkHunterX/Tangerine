using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace TangerineBaseMods.Patches;

internal static class HometopAnimationFix
{
    internal static bool hometop = false;
    
    internal static void InitializeHarmony(Harmony harmony)
    {
        harmony.PatchAll(typeof(HometopAnimationFix));
    }


    [HarmonyPrefix, HarmonyPatch(typeof(HometopSceneController), nameof(HometopSceneController.UpdateCharacter))]
    private static void UpdateCharacter_Prefix()
    {
        hometop = true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(HometopSceneController), nameof(HometopSceneController.UpdateCharacter))]
    private static void UpdateCharacter_Postfix()
    {
        hometop = false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(HometopSceneController.__c__DisplayClass19_1), nameof(HometopSceneController.__c__DisplayClass19_1._UpdateCharacter_b__1))]
    private static void LoadGame_UpdateCharacter_Prefix()
    {
        hometop = true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(HometopSceneController.__c__DisplayClass19_1), nameof(HometopSceneController.__c__DisplayClass19_1._UpdateCharacter_b__1))]
    private static void LoadGame_UpdateCharacter_Postfix()
    {
        hometop = false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(OrangeAnimatonHelper), nameof(OrangeAnimatonHelper.GetUniqueDebutName))]
    private static void GetUniqueDebutName_Postfix(string s_modelName, ref string bundleName, ref Il2CppStringArray __result)
    {
        // check if called from the hometop
        if (hometop)
        {
            bundleName = "model/animation/character/" + s_modelName;

            var s_modelBaseName = s_modelName[..^4];
            __result = new Il2CppStringArray(
            [
                s_modelBaseName + "_ui_debut_start",
                s_modelBaseName + "_ui_debut_loop",
                s_modelBaseName + "_ui_debut_loop_egg"
            ]);
        }
    }
}