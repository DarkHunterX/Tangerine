using HarmonyLib;
using System;
using UnityEngine;

namespace TangerineBaseMods.Patches
{
    internal class GoUIMenuFixes
    {
        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(GoUIMenuFixes));
        }

        // fix weapon swap button only updating local variables (this caused changing equipped cards to swap back old subweapon)
        [HarmonyPostfix, HarmonyPatch(typeof(GoCheckUI), nameof(GoCheckUI.OnExchangeWeapon))]
        static void OnExchangeWeapon_Postfix(GoCheckUI __instance)
        {
            PlayerNetManager.Instance.playerInfo.netPlayerInfo.MainWeaponID = __instance.nMainWeaponID;
            PlayerNetManager.Instance.playerInfo.netPlayerInfo.SubWeaponID = __instance.nSubWeaponID;
        }

        // fix stage ready menu not showing character skin art after clicking to go to weapon menu and then returning
        [HarmonyPostfix, HarmonyPatch(typeof(GoCheckUI), nameof(GoCheckUI.CheckUIReFocus))]
        internal static void CheckUIReFocus_Postfix(GoCheckUI __instance)
        {
            if (__instance.refSelectCharacter != null)
            {
                NetCharacterInfo netCharacterInfo = __instance.refSelectCharacter.tNetCharacterInfo.Cast<NetCharacterInfo>();
                CHARACTER_TABLE character_TABLE = OrangeDataManager.Instance.CHARACTER_TABLE_DICT[netCharacterInfo.CharacterID];

                SKIN_TABLE skin_TABLE = null;
                OrangeDataManager.Instance.SKIN_TABLE_DICT.TryGetValue(netCharacterInfo.Skin, out skin_TABLE);

                string text = $"St_{character_TABLE.s_ICON}";
                if (skin_TABLE != null)
                    text = $"St_{skin_TABLE.s_ICON}";

                __instance.characertimg.enabled = false;
                __instance.BgCharacter.alpha = 0f;
                __instance.BgCharacter.transform.Cast<RectTransform>().anchoredPosition = new Vector2(-1000f, 0f);
                __instance.characertimg.CheckLoadPerfab(string.Format(AssetBundleScriptableObject.Instance.m_texture_2d_stand_st, text), text, new Action(__instance.ShowCharacterImg));
            }
        }
    }
}
