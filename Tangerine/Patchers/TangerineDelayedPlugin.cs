using HarmonyLib;
using Reloaded.Hooks.Internal;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;

namespace Tangerine.Patchers
{
    /// <summary>
    /// TODO
    /// </summary>
    public class TangerineDelayedPlugin
    {
        private static Harmony _harmony;
        private readonly string _modGuid;

        //internal static readonly List<(Type test, MethodInfo test2)> hookList = new();
        internal static readonly List<(Type test, string test2)> hookList = new();

        private static bool isPatched = false;

        internal TangerineDelayedPlugin(string modGuid)
        {
            _modGuid = modGuid;
        }

        internal static void InitializeHarmony(Harmony harmony)
        {
            _harmony = harmony;
            _harmony.PatchAll(typeof(TangerineDelayedPlugin));
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void AddPatch(Type patchClass)
        {
            var methods = patchClass.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            foreach (var m in methods)
            {
                var attr = m.GetCustomAttributes<HarmonyPatch>();
                foreach (var patch in attr)
                {
                    var _originMethod = patch.info.declaringType.GetMethod(patch.info.methodName);
                    Plugin.Log.LogError($"test = {patch.info.declaringType}, {patch.info.methodName}");
                }       
            }          

            hookList.Add((patchClass, _modGuid));
        }

        /*[HarmonyPostfix, HarmonyPatch(typeof(GoCheckUI), nameof(GoCheckUI.OnGoBattle))]
        private static void fw_GoCheckUI_StartStage()
        {
            if (isPatched)
            {
                foreach (var patchClass in hookList)
                {
                    _harmony2.UnpatchSelf();
                }
                isPatched = false;
            }
        }*/

        [HarmonyPostfix, HarmonyPatch(typeof(StageSyncManager), nameof(StageSyncManager.LoadPlayerEnd))]
        private static void fw_GetPlayerData()
        {
            if (!isPatched)
            {
                foreach (var patchClass in hookList)
                    _harmony.PatchAll(patchClass.test);

                isPatched = true;
            }
        }
    }
}
