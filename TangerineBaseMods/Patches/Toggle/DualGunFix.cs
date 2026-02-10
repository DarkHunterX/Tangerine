using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using enums;
using UnityEngine;
using Tangerine.Manager.Mod;
using TangerineBaseMods.Config;

namespace TangerineBaseMods;

public class DualGunFix
{
    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony)
    {
        if (Configuration.DualGunFix.Value)
        {
            harmony.PatchAll(typeof(DualGunFix));
            Plugin.RemoveObsoleteMod_DualGunFix();
        }
        else
        {
            tangerine.Loader.RemoveAssetBundleId("model/animation/dualgun/c");
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(PlayerBuilder._Build_d__52), nameof(PlayerBuilder._Build_d__52.MoveNext))]
    private static void InitWepPostFix(PlayerBuilder._Build_d__52 __instance, bool __result)
    {
        bool flag = !__result;
        if (flag)
        {
            CHARACTER_TABLE character_TABLE = OrangeDataManager.Instance.CHARACTER_TABLE_DICT[__instance.__4__this.SetPBP.CharacterID];
            if (character_TABLE.s_ANIMATOR.StartsWith("female"))
            {
                PlayerBuilder pbInstance = __instance.__4__this;
                GameObject playerInstance = __instance._playerInstance_5__3;
                bool flag2 = playerInstance;
                
                if (flag2)
                {
                    Transform[] bodyInstance = playerInstance.transform.GetComponentsInChildren<Transform>(true);
                    WEAPON_TABLE[] loadout = new WEAPON_TABLE[2];
                    int[] WeaponList = pbInstance.SetPBP.WeaponList;
                    int num = 0;

                    foreach (Il2CppReferenceArray<Object> array2 in pbInstance._loadedWeapons)
                    {
                        loadout[num] = OrangeDataManager.Instance.WEAPON_TABLE_DICT[WeaponList[num]];
                        WeaponType weaponType = (WeaponType)loadout[num].n_TYPE;

                        if (weaponType == WeaponType.DualGun)
                        {
                            Transform currentWep = OrangeBattleUtility.FindChildRecursive(bodyInstance, "L WeaponPoint", true);
                            currentWep = OrangeBattleUtility.FindChildRecursive(currentWep, "NormalWeapon" + num.ToString(), true);
                            currentWep.localRotation = Quaternion.Euler(0, 0, 270);

                            currentWep = OrangeBattleUtility.FindChildRecursive(bodyInstance, "R WeaponPoint", true);
                            currentWep = OrangeBattleUtility.FindChildRecursive(currentWep, "NormalWeapon" + num.ToString(), true);
                            currentWep.localRotation = Quaternion.Euler(0, 0, -270);
                        }
                        num++;
                    }
                }
            }
        }
    }
}