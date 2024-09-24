using HarmonyLib;
using System;
using enums;
using CallbackDefs;
using Il2CppSystem.Collections.Generic;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using OrangeApi;

namespace TangerineBaseMods.Patches;

internal static class ChipIdRangeFix
{   
    internal static void InitializeHarmony(Harmony harmony)
    {
        harmony.PatchAll(typeof(ChipIdRangeFix));
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PlayerNetManager), nameof(PlayerNetManager.WeaponChipSetReq))]
    private static bool WeaponChipSetReq(PlayerNetManager __instance, int p_weaponID, int p_chipID, ChipSetState nSetState, Callback p_cb = null)
    {
        __instance.WebService.SendRequest<WeaponChipSetRes>(new WeaponChipSetReq
        {
            WeaponID = p_weaponID,
            ChipID = p_chipID,
            SetState = (sbyte)nSetState
        }, new Action<WeaponChipSetRes>((WeaponChipSetRes res) =>
        {
            if (res.WeaponInfo != null)
                __instance.dicWeapon.Value(res.WeaponInfo.WeaponID).netInfo = res.WeaponInfo;
            p_cb.CheckTargetToInvoke();
        }), false);

        // do not run original code
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PlayerNetManager), nameof(PlayerNetManager.WeaponChipSetReqs))]
    private static bool WeaponChipSetReqs(PlayerNetManager __instance, Il2CppStructArray<int> parray_AddWeaponID, int p_chipID, Il2CppStructArray<int> parray_RemoveWeaponID, Callback p_cb = null)
    {
        var list = new List<NetWeaponIDInfo>();
        for (int i = 0; i < parray_AddWeaponID.Length; i++)
            list.Add(new NetWeaponIDInfo { WeaponID = parray_AddWeaponID[i] });

        var list2 = new List<NetWeaponIDInfo>();
        for (int j = 0; j < parray_RemoveWeaponID.Length; j++)
            list2.Add(new NetWeaponIDInfo { WeaponID = parray_RemoveWeaponID[j] });

        __instance.WebService.SendRequest<EquipChipRes>(new EquipChipReq
        {
            AddWeaponIDList = list,
            RemoveWeaponIDList = list2,
            ChipID = p_chipID
        }, new Action<EquipChipRes>((EquipChipRes res) =>
        {
            for (int k = 0; k < res.WeaponList.Count; k++)
                __instance.dicWeapon.Value(res.WeaponList[k].WeaponID).netInfo = res.WeaponList[k];
            p_cb.CheckTargetToInvoke();
        }), false);

        // do not run original code
        return false;
    }
}