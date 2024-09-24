using HarmonyLib;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TangerineBaseMods.Patches.Toggle
{
    internal class IntroSkip
    {
        internal static void InitializeHarmony(Harmony harmony, JsonNode node)
        {
            if (node["IntroSkip"]["enabled"].Deserialize<bool>())
            {

                harmony.PatchAll(typeof(IntroSkip));
                Plugin.RemoveObsoleteMod_IntroSkip();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OrangeSceneManager), nameof(OrangeSceneManager.ChangeScene))]
        static void ChangeScenePrefix(ref string p_scene)
        {
            if (p_scene == "splash")
                p_scene = "title";
            else if (p_scene == "OpeningStage")
                p_scene = "title";
        }
    }
}
