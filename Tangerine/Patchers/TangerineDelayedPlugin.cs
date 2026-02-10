using HarmonyLib;
using System;
using System.Collections.Generic;
using Tangerine.Manager;
using Tangerine.Utils;

namespace Tangerine.Patchers
{
    /// <summary>
    /// Contains methods for delayed code patching for code that can't be patched at game bootup
    /// </summary>
    public class TangerineDelayedPlugin
    {
        private readonly string _modGuid;

        internal static readonly Dictionary<Harmony, List<Type>> hookDict = new();
        private static bool isPatched = false;

        internal TangerineDelayedPlugin(string modGuid)
        {
            _modGuid = modGuid;
        }

        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(TangerineDelayedPlugin));
        }

        /// <summary>
        /// Register a Harmony instance and a class containing Harmony patches for delayed patching
        /// </summary>
        public void AddPatchClass(Harmony harmony, Type patchClass)
        {
            LogMessage.LogWarning($"Registering delayed patch class: {patchClass.FullName}", ManagerConfig.DebugLogPluginDll.Value);

            if (hookDict.TryGetValue(harmony, out List<Type> pClass))
                pClass.Add(patchClass);
            else
                hookDict.Add(harmony, new List<Type> { patchClass });
        }

        /// <summary>
        /// Unregister a single patch class linked to a Harmony instance
        /// </summary>
        public void RemovePatchClass(Harmony harmony, Type patchClass)
        {
            if (hookDict.TryGetValue(harmony, out List<Type> pClass))
                pClass.Remove(patchClass);
        }

        /// <summary>
        /// Unregister all patch classes linked to a Harmony instance
        /// </summary>
        public void RemoveAllPatchClass(Harmony harmony)
        {
            hookDict.Remove(harmony);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GoCheckUI), nameof(GoCheckUI.OnGoBattle))]
        private static void fw_GoCheckUI_StartStage()
        {
            if (isPatched)
            {
                foreach (var hook in hookDict)
                    hook.Key.UnpatchSelf();
                isPatched = false;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StageSyncManager), nameof(StageSyncManager.LoadPlayerEnd))]
        private static void fw_GetPlayerData()
        {
            if (!isPatched)
            {
                foreach (var hook in hookDict)
                {
                    foreach (var patchClass in hook.Value)
                        hook.Key.PatchAll(patchClass);
                }
                isPatched = true;
            }
        }
    }
}
