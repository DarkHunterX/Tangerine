using DragonBones;
using enums;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.Reflection;
using Tangerine.Manager;
using Tangerine.Utils;
using UnityEngine;

namespace Tangerine.Patchers
{
    /// <summary>
    /// Contains methods for adding and updating character debut animation easter egg triggers
    /// </summary>
    public class TangerineAnimator
    {
        internal static readonly Dictionary<int, EggInfo> _dictDebutEgg = new();
        internal static readonly ModDictionary<int, EggInfo> DebutEgg = new();

        private readonly string _modGuid;

        internal struct EggInfo
        {
            public int Trigger;
            public string[] AnimClips;
        }

        static TangerineAnimator()
        {
            DebutEgg.BaseChangedEvent += ApplyDebutEggPatch;
            DebutEgg.BaseResetEvent += ResetDebutEggPatch;
        }

        internal TangerineAnimator(string modGuid)
        {
            _modGuid = modGuid;
        }

        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(TangerineAnimator));
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

        [HarmonyPrefix, HarmonyPatch(typeof(OrangeAnimatonHelper), nameof(OrangeAnimatonHelper.AnimatorShort))]
        private static bool AnimatorShort(string animator, ref string __result)
        {
            __result = GetAnimatorType(animator);

            // do not run original code
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HumanBase), nameof(HumanBase.GetWeaponMotionBundlePath))]
        private static bool GetWeaponMotionBundlePath(string animatorType, WeaponType weaponType, ref string __result)
        {
            var weaponTypeName = HumanBase.GetWeaponTypeName(weaponType);
            var animatorTypeName = GetAnimatorType(animatorType);
            __result = $"model/animation/{weaponTypeName}/{animatorType}";

            // do not run original code
            return false;
        }

        private static string GetAnimatorType(string animator)
        {
            var animatorTypeList = new Dictionary<string, string>()
            {
                { "male", "m" },
                { "female", "f" },
                { "classic", "c" },
            };

            foreach (var animatorType in animatorTypeList)
            {
                if (animator.Contains(animatorType.Key) ? true : false)
                    return animatorType.Value;
            }

            Plugin.Log.LogError($"Unknown Animator Type: {animator}");
            return "m";
        }
    }
}