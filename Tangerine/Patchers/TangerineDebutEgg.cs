using System;
using System.Reflection;
using System.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using HarmonyLib;
using Tangerine.Manager;
using Tangerine.Utils;

namespace Tangerine.Patchers
{
    /// <summary>
    /// Contains methods for adding and updating character debut animation easter egg triggers
    /// </summary>
    public class TangerineDebutEgg
    {
        private static Harmony _harmony;

        internal static readonly Dictionary<int, EggInfo> _dictDebutEgg = new();
        internal static readonly ModDictionary<int, EggInfo> DebutEgg = new();

        private readonly string _modGuid;

        internal struct EggInfo
        {
            public int Trigger;
            public string[] AnimClips;
        }

        static TangerineDebutEgg()
        {
            DebutEgg.BaseChangedEvent += ApplyDebutEggPatch;
            DebutEgg.BaseResetEvent += ResetDebutEggPatch;
        }

        internal TangerineDebutEgg(string modGuid)
        {
            _modGuid = modGuid;
        }

        internal static void InitializeHarmony(Harmony harmony)
        {
            _harmony = harmony;
            _harmony.PatchAll(typeof(TangerineDebutEgg));
        }

        /// <summary>
        /// Adds the character debut animation Easter egg info to allow the game to trigger it
        /// </summary>
        /// <param name="model">Character model number</param>
        /// <param name="trigger">Animation play count until triggering the Easter egg</param>
        /// <param name="eggStart">Easter egg animation name</param>
        /// <param name="eggLoop">Easter egg animation name (idle loop)</param>
        public void AddDebutEggData(int model, int trigger, string eggStart, string eggLoop)
        {
            DebutEgg.Set(_modGuid, model, new EggInfo() 
            { 
                Trigger = trigger,
                AnimClips = [eggStart, eggLoop]
            });
        }

        /// <summary>
        /// Removes the character debut animation Easter egg info that was added before
        /// </summary>
        /// <param name="id">ID of the Character model</param>
        public bool RemoveDebutEggData(int id)
        {
            if (DebutEgg.TryGetValue(_modGuid, id, out var animClips))
            {
                DebutEgg.Remove(_modGuid, id);

                // No need to apply anything to the game here, as the event in the Base dictionary will do it
                return true;
            }

            return false;
        }

        private static void ApplyDebutEggPatch(int id, BaseChangeType changeType)
        {
            switch (changeType)
            {
                case BaseChangeType.Add:
                case BaseChangeType.Update:
                    _dictDebutEgg[id] = DebutEgg.Base[id];
                    break;
                case BaseChangeType.Remove:
                    _dictDebutEgg.Remove(id);
                    break;
            }
        }

        private static void ResetDebutEggPatch(IEnumerable<int> ids)
        {
            // Unpatch existing Base
            foreach (int id in ids)
                _dictDebutEgg.Remove(id);

            // Patch new Base
            foreach (var pair in DebutEgg.Base)
                _dictDebutEgg[pair.Key] = pair.Value;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RenderTextureObj), nameof(RenderTextureObj.UpdateBonusClip))]
        private static bool UpdateBonusClip(RenderTextureObj __instance, ref Il2CppStringArray clips)
        {
            int playCount;
            int characterModel = __instance.characterId;

            if (!__instance.dictEXclipsCount.TryGetValue(characterModel, out playCount))
                playCount = 0;

            if (_dictDebutEgg.TryGetValue(characterModel, out EggInfo eggInfo))
            {
                LogMessage.LogWarning($"Debut animation play count for character model {characterModel} = {playCount}", LogMessage.DebutEgg);
                if (playCount >= eggInfo.Trigger)
                {
                    clips = eggInfo.AnimClips;
                    LogMessage.LogWarning($"Playing debut animation easter egg for character model {characterModel}", LogMessage.DebutEgg);
                }
            }
            else
            {
                // First Armor X
                if (characterModel == 23 && playCount >= 5)
                    clips = new string[] { "ch023_ui_debut_egg_start", "ch023_ui_debut_loop" };
                
                // Third Armor X
                else if (characterModel == 43 && playCount >= 3)
                    clips = new string[] { "ch043_ui_debut_egg_start", "ch043_ui_debut_egg_loop" };
            }

            // do not run original code
            return false;
        }
    }
}