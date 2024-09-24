using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using CallbackDefs;
using enums;
using UnityEngine;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tangerine.Manager.Mod;

namespace TangerineBaseMods;

public class DualGunFix
{
    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony, JsonNode node)
    {
        if (node["DualGunFix"]["enabled"].Deserialize<bool>())
        {
            harmony.PatchAll(typeof(DualGunFix));
            Plugin.RemoveObsoleteMod_DualGunFix();
        }
        else
        {
            tangerine.Loader.RemoveAssetBundleId("model/animation/dualgun/c");
        }
    }

    public static GameObject weapon2;
    public static GameObject weapon7;
    public static GameObject weapon_link;

    public static int idStorage = 0;
    public static bool subCheck = true;
    public static int SpecialPoseCheck = 0;

    [HarmonyPostfix, HarmonyPatch(typeof(HometopSceneController), nameof(HometopSceneController.UpdateCharacter))]
    static void UpdateCharacterCheckPose(CHARACTER_TABLE character)
    {
        // Store value of n_SPECIAL_SHOWPOSE
        SpecialPoseCheck = character.n_SPECIAL_SHOWPOSE;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterAnimatorStandBy), nameof(CharacterAnimatorStandBy.UpdateWeapon), new[] { typeof(int), typeof(bool), typeof(Callback) })]
    static void SetWeaponPreAppend(int weaponId, bool updatePos, Callback p_cb)
    {
        if (SpecialPoseCheck == 0 && !weapon_link)
        {
            WEAPON_TABLE equipWeapon = null;
            OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(weaponId, out equipWeapon);

            if (equipWeapon.n_SUB_LINK != -1)
            {
                WEAPON_TABLE equipWeapon2 = null;
                OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(equipWeapon.n_SUB_LINK, out equipWeapon2);

                if (equipWeapon.s_MODEL != equipWeapon2.s_MODEL)
                {
                    string sub_link = AssetBundleScriptableObject.Instance.m_newmodel_weapon + equipWeapon2.s_MODEL;

                    AssetsBundleManager.Instance.GetAssetAndAsyncLoad<GameObject>(sub_link, equipWeapon2.s_MODEL + "_U.prefab", new System.Action<GameObject>((GameObject gameObject) =>
                    {
                        weapon_link = gameObject;
                    }), AssetKeepMode.KEEP_IN_SCENE);
                }
            }
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterAnimatorStandBy), nameof(CharacterAnimatorStandBy.SetWeapon))]
    static void SetWeaponAppend(CharacterAnimatorStandBy __instance, ref GameObject p_weapon, WEAPON_TABLE p_weaponData)
    {
        if (SpecialPoseCheck == 0)
        {
            if (idStorage == 0)
                idStorage = __instance.GetInstanceID();

            if (__instance.GetInstanceID() == idStorage)
            {
                if (weapon2)
                {
                    CharacterMaterial cmOld2 = weapon2.GetComponent<CharacterMaterial>();
                    cmOld2.ChangeDissolveTime(0.3f);
                    cmOld2.Disappear(null, -1f);
                    Object.DestroyImmediate(cmOld2.gameObject);
                }

                Vector3 zero = Vector3.zero;
                WeaponType weaponType2 = (WeaponType)p_weaponData.n_TYPE;

                if (weaponType2 == WeaponType.DualGun)
                {
                    weapon2 = p_weapon;
                    weapon2 = Object.Instantiate<GameObject>(weapon2);
                    Transform[] componentsInChildren7 = __instance.gameObject.GetComponentsInChildren<Transform>(true);

                    weapon2.transform.SetParent(OrangeBattleUtility.FindChildRecursive((Il2CppReferenceArray<Transform>)componentsInChildren7, "L WeaponPoint", false));
                    weapon2.transform.localPosition = zero;
                    weapon2.transform.localEulerAngles = zero;

                    if (weapon2.transform.parent != null)
                    {
                        OrangeBattleUtility.ChangeLayersRecursively<Transform>(weapon2.transform, weapon2.transform.parent.gameObject.layer);
                    }

                    weapon2.transform.localScale = new Vector3(__instance.extraSize[0], __instance.extraSize[0], __instance.extraSize[0]);

                    CharacterMaterial component2 = weapon2.GetComponent<CharacterMaterial>();
                    component2.SetBaseRenderForUI();
                    component2.ChangeDissolveTime(0.3f);
                    component2.Appear(null, -1f);
                    OrangeBattleUtility.Instance.WeaponForceRotate(weapon2);
                }
            }
            else
            {
                if (weapon7)
                {
                    CharacterMaterial cmOld2 = weapon7.GetComponent<CharacterMaterial>();
                    cmOld2.ChangeDissolveTime(0.3f);
                    cmOld2.Disappear(null, -1f);
                    Object.DestroyImmediate(cmOld2.gameObject);
                }

                Vector3 zero = Vector3.zero;
                WeaponType weaponType2 = (WeaponType)p_weaponData.n_TYPE;

                if (weaponType2 == WeaponType.DualGun)
                {
                    weapon7 = p_weapon;
                    weapon7 = Object.Instantiate<GameObject>(weapon7);
                    Transform[] componentsInChildren7 = __instance.gameObject.GetComponentsInChildren<Transform>(true);

                    weapon7.transform.SetParent(OrangeBattleUtility.FindChildRecursive((Il2CppReferenceArray<Transform>)componentsInChildren7, "L WeaponPoint", false));
                    weapon7.transform.localPosition = zero;
                    weapon7.transform.localEulerAngles = zero;

                    if (weapon7.transform.parent != null)
                    {
                        OrangeBattleUtility.ChangeLayersRecursively<Transform>(weapon7.transform, weapon7.transform.parent.gameObject.layer);
                    }

                    weapon7.transform.localScale = new Vector3(__instance.extraSize[0], __instance.extraSize[0], __instance.extraSize[0]);

                    CharacterMaterial component2 = weapon7.GetComponent<CharacterMaterial>();
                    component2.SetBaseRenderForUI();
                    component2.ChangeDissolveTime(0.3f);
                    component2.Appear(null, -1f);
                    OrangeBattleUtility.Instance.WeaponForceRotate(weapon7);
                }
            }

            if (!weapon_link)
            {
                if (p_weaponData.n_SUB_LINK != -1)
                {
                    WEAPON_TABLE equipWeapon2 = null;
                    OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(p_weaponData.n_SUB_LINK, out equipWeapon2);

                    if (p_weaponData.s_MODEL != equipWeapon2.s_MODEL)
                    {
                        string sub_link = AssetBundleScriptableObject.Instance.m_newmodel_weapon + equipWeapon2.s_MODEL;

                        AssetsBundleManager.Instance.GetAssetAndAsyncLoad<GameObject>(sub_link, equipWeapon2.s_MODEL + "_U.prefab", new System.Action<GameObject>((GameObject gameObject) =>
                        {
                            weapon_link = gameObject;
                        }), AssetKeepMode.KEEP_IN_SCENE);
                    }
                }
            }

            if (p_weaponData.n_SUB_LINK != -1 && p_weaponData.n_TYPE == 16 && weapon_link)
            {
                WEAPON_TABLE equipWeapon2 = null;
                OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(p_weaponData.n_SUB_LINK, out equipWeapon2);

                if (p_weaponData.s_MODEL != equipWeapon2.s_MODEL)
                    p_weapon = weapon_link.gameObject;
            }
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterAnimatorStandBy), nameof(CharacterAnimatorStandBy.OnDisable))]
    public static void SetWepPostFix(CharacterAnimatorStandBy __instance)
    {
        if (SpecialPoseCheck == 0 && __instance.GetInstanceID() == idStorage)
            idStorage = 0;
    }
}