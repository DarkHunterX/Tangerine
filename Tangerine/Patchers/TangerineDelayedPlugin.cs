using Fasterflect;
using HarmonyLib;
using System;
using System.Collections.Generic;
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
        private const string delayedGUID = $"{Plugin.GUID}_DelayedPlugin";

        internal static readonly List<Type> hookList = new();
        private static bool isPatched = false;

        internal TangerineDelayedPlugin(string modGuid)
        {
            _modGuid = modGuid;
        }

        internal static void InitializeHarmony(Harmony harmony)
        {
            _harmony = new Harmony(delayedGUID);
            _harmony.PatchAll(typeof(TangerineDelayedPlugin));
        }

        /// <summary>
        /// TODO
        /// </summary>
        public void AddPatch(Type patchClass)
        {
            hookList.Add(patchClass);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GoCheckUI), nameof(GoCheckUI.OnGoBattle))]
        private static void fw_GoCheckUI_StartStage()
        {
            if (isPatched)
            {
                Plugin.Log.LogError($"unpatching delayed plugins");
                _harmony.UnpatchSelf();

                isPatched = false;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StageSyncManager), nameof(StageSyncManager.LoadPlayerEnd))]
        private static void fw_GetPlayerData()
        {
            if (!isPatched)
            {
                Plugin.Log.LogError($"patching delayed plugins");
                foreach (var patchClass in hookList)
                    _harmony.PatchAll(patchClass);

                isPatched = true;
            }
        }
    }
}
