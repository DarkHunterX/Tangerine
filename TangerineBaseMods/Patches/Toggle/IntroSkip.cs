using HarmonyLib;
using TangerineBaseMods.Config;

namespace TangerineBaseMods.Patches.Toggle
{
    internal class IntroSkip
    {
        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(IntroSkip));
            Plugin.RemoveObsoleteMod_IntroSkip();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OrangeSceneManager), nameof(OrangeSceneManager.ChangeScene))]
        static void ChangeScenePrefix(ref string p_scene)
        {
            if (Configuration.IntroSkip.Value)
            {
                if (p_scene == "splash")
                    p_scene = "title";
                else if (p_scene == "OpeningStage")
                    p_scene = "title";
            }
        }
    }
}
