using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using CallbackDefs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tangerine.Manager.Mod;

namespace TangerineBaseMods;

public class BorderExAddon
{
    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony, JsonNode node)
    {
        if (node["BorderExAddon"]["enabled"].Deserialize<bool>())
        {
            harmony.PatchAll(typeof(BorderExAddon));
            Plugin.RemoveObsoleteMod_SSBorder();
        }
        else
        {
            tangerine.Loader.RemoveAssetBundleId("texture/2d/ui/+ui_common");
            tangerine.Loader.RemoveAssetBundleId("ui/ui_illustration");
        }
    }

    #region Variable
    private static List<Sprite> PreLoadSprite = new List<Sprite>();
    private static int _rarity = 3;
    private enum CardPart
    {
        Background_SS = 0,
        Frame_SS,
        Background_C,
        Frame_C,
        Background_D,
        Frame_D
    }
    #endregion

    #region Fix bad coding (CardBase)
    [HarmonyPrefix, HarmonyPatch(typeof(CardBase), nameof(CardBase.SetColorType))]
    static bool fw_CardBase_SetColorType(CardBase __instance, int typ)
    {
        for (int i = 0; i < __instance.TypeRoots.Length; i++)
            __instance.TypeRoots[i].gameObject.SetActive(i == __instance.GetTypeIndex(typ));
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CardBase), nameof(CardBase.SetStar))]
    static bool fw_CardBase_SetStar(CardBase __instance, int nStar)
    {
        for (int i = 0; i < __instance.StarRoots.Length; i++)
            __instance.StarRoots[i].gameObject.SetActive(nStar > i);
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CardBase), nameof(CardBase.SetRarity))]
    static bool fw_CardBase_Rarity(CardBase __instance, int nRarity)
    {
        for (int i = 0; i < __instance.FrameRoots.Length; i++)
        {
            __instance.BackgroundRoots[i].gameObject.SetActive((nRarity - 1) == i);
            __instance.FrameRoots[i].gameObject.SetActive((nRarity - 1) == i);
        }
        return false;
    }
    #endregion

    #region Card Setup (CardBase)
    [HarmonyPrefix, HarmonyPatch(typeof(CardBase), nameof(CardBase.CardSetup))]
    static bool fw_CardBase_CardSetup(CardBase __instance, NetCardInfo info, bool bSmall = false)
    {
        try
        {
            if (info != null)
            {
                EXP_TABLE cardExpTable = OrangeTableHelper.Instance.GetCardExpTable(info.Exp);
                CARD_TABLE tCardTable = OrangeDataManager.Instance.CARD_TABLE_DICT[info.CardID];
                _rarity = tCardTable.n_RARITY;

                LoadSpriteFromBundle(__instance);
                __instance.LevelText.text = cardExpTable.n_ID.ToString();
                if (!__instance.MiddleVer)
                {
                    __instance.NameText.text = OrangeTextDataManager.Instance.CARDTEXT_TABLE_DICT.GetL10nValue(tCardTable.w_NAME);
                    __instance.NumberText.text = tCardTable.n_ID.ToString();
                }
                __instance.LockImage.gameObject.SetActive(info.Protected == 1);
                __instance.SetRarity(_rarity);
                __instance.SetColorType(tCardTable.n_TYPE);
                __instance.SetStar((int)info.Star);
                if (__instance.FavoriteImage != null)
                    __instance.FavoriteImage.gameObject.SetActive(info.Favorite == 1);
                
                string text;
                if (bSmall)
                    text = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_m_format, tCardTable.n_PATCH);
                else
                    text = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_l_format, tCardTable.n_PATCH);

                AssetsBundleManager.Instance.GetAssetAndAsyncLoad<Sprite>(text, tCardTable.s_CARDIMG, new Action<Sprite>((Sprite asset) =>
                {
                    if (asset != null)
                    {
                        __instance.IconImage.sprite = asset;
                        return;
                    }
                    Plugin.Log.LogWarning("CardTable: unable to load sprite " + tCardTable.s_ICON);
                }));
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogError("Error when Setup Cards");
            Plugin.Log.LogError(e.Message);
        }
        return false;
    }
    #endregion

    #region Change Border (GalleryCardCell)
    [HarmonyPostfix, HarmonyPatch(typeof(GalleryCardCell), nameof(GalleryCardCell.SetName))]
    static void fw_GalleryCardCell_SetName(GalleryCardCell __instance)
    {
        for (int i = 0; i < __instance.RareFrames.Count; i++)
        {
            bool flag = i + 1 == __instance.tCardTable.n_RARITY;
            __instance.RareBGs[i].SetActive(flag);
            __instance.RareFrames[i].SetActive(flag);
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GalleryCardCell), nameof(GalleryCardCell.OnPointerClick))]
    static void fw_GalleryCardCell_OnPointerClick(GalleryCardCell __instance, PointerEventData eventData)
    {
        if (eventData.pointerEnter.name.Equals("imgRareBgFrame_SS"))
            __instance.OnCellCkick();
    }

    #endregion

    #region Utility
    private static void LoadSpriteFromBundle(CardBase __instance)
    {
        if (PreLoadSprite.Count > 0)
        {
            var gc = false;
            foreach (var sprite in PreLoadSprite)
            {
                try
                {
                    var name = sprite.name;
                }
                catch (Exception)
                {
                    gc = true;
                    break;
                }
            }
            if (gc)
            {
                PreLoadSprite.Clear();
                LoadSpriteFromBundle(__instance);
            }
            else
                AddFramesAndBackground(__instance, PreLoadSprite);
        }
        else
        {
            string[] bundle = new string[] { "texture/2d/ui/+ui_common" };
            var act = new Action(() =>
            {
                List<Sprite> lstSprite = new List<Sprite>
            {
                AssetsBundleManager.Instance.GetAssstSync<Sprite>(bundle[0], "UI_IconSource_Card_Bg_SS"),
                AssetsBundleManager.Instance.GetAssstSync<Sprite>(bundle[0], "UI_IconSource_Card_Frame_SS"),
                AssetsBundleManager.Instance.GetAssstSync<Sprite>(bundle[0], "UI_IconSource_Card_Bg_C"),
                AssetsBundleManager.Instance.GetAssstSync<Sprite>(bundle[0], "UI_IconSource_Card_Frame_C"),
                AssetsBundleManager.Instance.GetAssstSync<Sprite>(bundle[0], "UI_IconSource_Card_Bg_D"),
                AssetsBundleManager.Instance.GetAssstSync<Sprite>(bundle[0], "UI_IconSource_Card_Frame_D")
            };

                PreLoadSprite = lstSprite;
                AddFramesAndBackground(__instance, lstSprite);
            });
            AssetsBundleManager.Instance.LoadAssets(new Il2CppStringArray(bundle), (Callback)act, AssetKeepMode.KEEP_IN_SCENE);
        }
    }

    static GameObject CopyTransform(GameObject source, int childPosition = -1)
    {
        GameObject NewObj = GameObject.Instantiate(source);
        NewObj.transform.SetParent(source.transform.parent);
        if (childPosition != -1) NewObj.transform.SetSiblingIndex(childPosition);
        NewObj.transform.position = source.transform.position;
        NewObj.transform.rotation = source.transform.rotation;
        NewObj.transform.localScale = source.transform.localScale;
        return NewObj;
    }

    static Image CloneFrameBackGroundObject(CardBase __instance, int index, GameObject obj, Sprite sprite, string name = "")
    {
        if (obj == null)
            return new Image();

        GameObject frame = CopyTransform(obj, index);
        frame.name = obj.name;
        frame.GetComponent<Image>().sprite = sprite;
        if (name != "") frame.GetComponent<Image>().name = name;

        return frame.GetComponent<Image>();
    }

    static void AddFramesAndBackground(CardBase __instance, List<Sprite> lstSprite)
    {
        if (__instance.FrameRoots.Length == 6)
            return;

        List<Image> lstFrame = new List<Image>();
        List<Image> lstBG = new List<Image>();
        int id = 0;
        try
        {
            // D Rank
            id = __instance.FrameRoots[0].gameObject.transform.GetSiblingIndex();
            lstFrame.Add(CloneFrameBackGroundObject(__instance, id, __instance.FrameRoots[0].gameObject, lstSprite[(int)CardPart.Frame_D], "D_Frame"));

            id = __instance.BackgroundRoots[0].gameObject.transform.GetSiblingIndex();
            lstBG.Add(CloneFrameBackGroundObject(__instance, id, __instance.BackgroundRoots[0].gameObject, lstSprite[(int)CardPart.Background_D], "D_Background"));

            // C Rank
            id = __instance.FrameRoots[0].gameObject.transform.GetSiblingIndex();
            lstFrame.Add(CloneFrameBackGroundObject(__instance, id, __instance.FrameRoots[0].gameObject, lstSprite[(int)CardPart.Frame_C], "C_Frame"));

            id = __instance.BackgroundRoots[0].gameObject.transform.GetSiblingIndex();
            lstBG.Add(CloneFrameBackGroundObject(__instance, id, __instance.BackgroundRoots[0].gameObject, lstSprite[(int)CardPart.Background_C], "C_Background"));

            // B Rank
            __instance.FrameRoots[2].transform.name = "B_Frame";
            __instance.BackgroundRoots[2].transform.name = "B_Background";
            lstFrame.Add(__instance.FrameRoots[2]);
            lstBG.Add(__instance.BackgroundRoots[2]);

            // A Rank
            __instance.FrameRoots[3].transform.name = "A_Frame";
            __instance.BackgroundRoots[3].transform.name = "A_Background";
            lstFrame.Add(__instance.FrameRoots[3]);
            lstBG.Add(__instance.BackgroundRoots[3]);

            // S Rank
            __instance.FrameRoots[4].transform.name = "S_Frame";
            __instance.BackgroundRoots[4].transform.name = "S_Background";
            lstFrame.Add(__instance.FrameRoots[4]);
            lstBG.Add(__instance.BackgroundRoots[4]);

            // SS Rank
            id = __instance.FrameRoots[4].gameObject.transform.GetSiblingIndex() + 1;
            lstFrame.Add(CloneFrameBackGroundObject(__instance, id, __instance.FrameRoots[4].gameObject, lstSprite[(int)CardPart.Frame_SS], "SS_Frame"));

            id = __instance.BackgroundRoots[4].gameObject.transform.GetSiblingIndex() + 1;
            lstBG.Add(CloneFrameBackGroundObject(__instance, id, __instance.BackgroundRoots[4].gameObject, lstSprite[(int)CardPart.Background_SS], "SS_Background"));

            // remove leftover Rarity C and D
            Transform[] ts = __instance.transform.GetComponentsInChildren<Transform>();
            if (ts != null)
            {
                foreach (Transform t in ts)
                    if (t.name.Equals("Image") && (t.parent.name.Equals("BackgroundRoots") || t.parent.name.Equals("FrameRoots")))
                        GameObject.Destroy(t.gameObject);
            }

            // update array
            Image[] aFrame = lstFrame.ToArray();
            Image[] aBackground = lstBG.ToArray();
            __instance.FrameRoots = new Il2CppReferenceArray<Image>(aFrame);
            __instance.BackgroundRoots = new Il2CppReferenceArray<Image>(aBackground);

            // update rarity
            __instance.SetRarity(_rarity);
        }
        catch (Exception)
        {
        }
    }
    #endregion
}