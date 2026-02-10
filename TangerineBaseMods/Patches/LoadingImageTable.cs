using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using TangerineBaseMods.Config;
using TangerineBaseMods.Utils;
using UnityEngine;

namespace TangerineBaseMods.Patches
{
    internal class LoadingImageTable
    {
        private static bool isCharTableLoaded = false;
        private const string jsonFile = "LOADING_TABLE.json";
        private static readonly string modDir = Path.Combine(BepInEx.Paths.BepInExRootPath, "mods");
        
        internal static readonly List<LoadingImgInfo> CharLoadingImgs = new List<LoadingImgInfo>();
        internal static readonly List<LoadingImgInfo> SkinLoadingImgs = new List<LoadingImgInfo>();
        internal static readonly List<LoadingImgInfo> WepLoadingImgs = new List<LoadingImgInfo>();
        internal static readonly List<LoadingImgInfo> CardLoadingImgs = new List<LoadingImgInfo>();
        internal static readonly List<LoadingImgInfo> ChipLoadingImgs = new List<LoadingImgInfo>();
        internal static readonly List<LoadingImgInfo> ArmorLoadingImgs = new List<LoadingImgInfo>();
        internal static readonly List<LoadingImgInfo> ItemLoadingImgs = new List<LoadingImgInfo>();
        internal static readonly List<LoadingImgInfo> SkillLoadingImgs = new List<LoadingImgInfo>();
        internal static readonly List<LoadingImgInfo> OtherLoadingImgs = new List<LoadingImgInfo>();
        
        internal static readonly List<string> LockImgList = new List<string>() { string.Empty };
        internal static readonly List<LoadingImgInfo> AllLoadingImgs = new List<LoadingImgInfo>();

        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(LoadingImageTable));
            
            foreach (var modPath in Directory.GetDirectories(modDir))
                LoadTable(modPath);
        }

        internal struct LoadingImgInfo
        {
            public string s_NAME;
            public string s_IMG;
            public int n_TYPE;
            public string w_NAME;
        }

        internal enum LoadingImgType
        {
            Character = 1,
            Skin,
            Weapon,
            Card,
            Chip,
            Armor,
            Item,
            Skill,
            Other
        }

        internal static bool LoadTable(string modPath)
        {
            try
            {
                var loadingImgTable = Path.Combine(modPath, "Tables", jsonFile);
                if (!File.Exists(loadingImgTable))
                    return false;
                
                var node = JsonNode.Parse(File.ReadAllText(loadingImgTable));
                var list = node["LOADING_TABLE"]?.AsArray();
                
                if (list == null)
                {
                    Plugin.Log.LogError($"Failed to read {jsonFile} for mod \"{modPath}\"");
                    return false;
                }

                foreach (var imgInfo in list.Select(DeserializeLoadingImg))
                {
                    AddtoLoadingImgList(imgInfo);
                    LockImgList.Add($"{imgInfo.s_NAME}");
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to read {jsonFile} for mod \"{modPath}\": {e}");
                return false;
            }

            return true;
        }

        internal static LoadingImgInfo DeserializeLoadingImg(JsonNode node)
        {
            return new LoadingImgInfo()
            {
                s_NAME = node["s_NAME"].Deserialize<string>(),
                s_IMG = node["s_IMG"].Deserialize<string>(),
                n_TYPE = node["n_TYPE"].Deserialize<int>(),
                w_NAME = node["w_NAME"].Deserialize<string>()
            };
        }

        internal static LoadingImgInfo DeserializeLoadingImg_CharTable(CHARACTER_TABLE character_TABLE)
        {
            return new LoadingImgInfo()
            {
                s_NAME = $"{character_TABLE.s_MODEL} - {character_TABLE.s_VOICE}",
                s_IMG = $"{character_TABLE.s_ICON}_loading",
                n_TYPE = (int)LoadingImgType.Character,
                w_NAME = character_TABLE.w_NAME
            };
        }

        internal static void AddtoLoadingImgList(LoadingImgInfo imgInfo)
        {
            // add to master list
            AllLoadingImgs.Add(imgInfo);

            // add to type list
            switch ((LoadingImgType)imgInfo.n_TYPE)
            {
                case LoadingImgType.Character:
                    CharLoadingImgs.Add(imgInfo);
                    break;
                case LoadingImgType.Skin:
                    SkinLoadingImgs.Add(imgInfo);
                    break;
                case LoadingImgType.Weapon:
                    WepLoadingImgs.Add(imgInfo);
                    break;
                case LoadingImgType.Card:
                    CardLoadingImgs.Add(imgInfo);
                    break;
                case LoadingImgType.Chip:
                    ChipLoadingImgs.Add(imgInfo);
                    break;
                case LoadingImgType.Armor:
                    ArmorLoadingImgs.Add(imgInfo);
                    break;
                case LoadingImgType.Item:
                    ItemLoadingImgs.Add(imgInfo);
                    break;
                case LoadingImgType.Skill:
                    SkillLoadingImgs.Add(imgInfo);
                    break;
                default:
                    OtherLoadingImgs.Add(imgInfo);
                    break;
            }
        }

        internal static bool HasLoadingImgs(LoadingImgType imgType)
        {
            switch (imgType)
            {
                case LoadingImgType.Character:
                    return CharLoadingImgs.Count() > 0;
                case LoadingImgType.Skin:
                    return SkinLoadingImgs.Count() > 0;
                case LoadingImgType.Weapon:
                    return WepLoadingImgs.Count() > 0;
                case LoadingImgType.Card:
                    return CardLoadingImgs.Count() > 0;
                case LoadingImgType.Chip:
                    return ChipLoadingImgs.Count() > 0;
                case LoadingImgType.Armor:
                    return ArmorLoadingImgs.Count() > 0;
                case LoadingImgType.Item:
                    return ItemLoadingImgs.Count() > 0;
                case LoadingImgType.Skill:
                    return SkillLoadingImgs.Count() > 0;
                case LoadingImgType.Other:
                    return OtherLoadingImgs.Count() > 0;
                default:
                    return false;
            }
        }

        internal static LoadingImgInfo PickRandomLoadingImg()
        {
            // build list of IMG types that have images available
            var weightList = new List<RandomUtil.WeightedValue>();
            var weightValues = Configuration.LoadingImgGacha.Value.ToIntArray();
            for (int i = 0; i < 9; i++)
            {
                if ((i == 0 || weightValues[i] != 0) && HasLoadingImgs((LoadingImgType)i+1))
                    weightList.Add(new RandomUtil.WeightedValue() { Id = i+1, Weight = weightValues[i] });
            }

            // pick weighted random IMG type
            var randomType = LoadingImgType.Character;
            if (Configuration.LoadingImgGacha.Value.TotalWeight() != 0)
                randomType = (LoadingImgType)RandomUtil.Range(weightList.ToArray());

            // pick random image from the selected IMG type
            switch (randomType)
            {
                case LoadingImgType.Character:
                    return CharLoadingImgs[UnityEngine.Random.Range(0, CharLoadingImgs.Count)];
                case LoadingImgType.Skin:
                    return SkinLoadingImgs[UnityEngine.Random.Range(0, SkinLoadingImgs.Count)];
                case LoadingImgType.Weapon:
                    return WepLoadingImgs[UnityEngine.Random.Range(0, WepLoadingImgs.Count)];
                case LoadingImgType.Card:
                    return CardLoadingImgs[UnityEngine.Random.Range(0, CardLoadingImgs.Count)];
                case LoadingImgType.Chip:
                    return ChipLoadingImgs[UnityEngine.Random.Range(0, ChipLoadingImgs.Count)];
                case LoadingImgType.Armor:
                    return ArmorLoadingImgs[UnityEngine.Random.Range(0, ArmorLoadingImgs.Count)];
                case LoadingImgType.Item:
                    return ItemLoadingImgs[UnityEngine.Random.Range(0, ItemLoadingImgs.Count)];
                case LoadingImgType.Skill:
                    return SkillLoadingImgs[UnityEngine.Random.Range(0, SkillLoadingImgs.Count)];
                case LoadingImgType.Other:
                    return OtherLoadingImgs[UnityEngine.Random.Range(0, OtherLoadingImgs.Count)];
                default:
                    return CharLoadingImgs[UnityEngine.Random.Range(0, CharLoadingImgs.Count)];
            }
        }

        internal static string LoadStringByTableType(LoadingImgInfo imgInfo)
        {
            switch ((LoadingImgType)imgInfo.n_TYPE)
            {
                case LoadingImgType.Character:
                    return OrangeTextDataManager.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(imgInfo.w_NAME);
                case LoadingImgType.Skin:
                    return OrangeTextDataManager.Instance.SKINTEXT_TABLE_DICT.GetL10nValue(imgInfo.w_NAME);
                case LoadingImgType.Weapon:
                    return OrangeTextDataManager.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(imgInfo.w_NAME);
                case LoadingImgType.Card:
                    return OrangeTextDataManager.Instance.CARDTEXT_TABLE_DICT.GetL10nValue(imgInfo.w_NAME);
                case LoadingImgType.Chip:
                    return OrangeTextDataManager.Instance.DISCTEXT_TABLE_DICT.GetL10nValue(imgInfo.w_NAME);
                case LoadingImgType.Armor:
                    return OrangeTextDataManager.Instance.EQUIPTEXT_TABLE_DICT.GetL10nValue(imgInfo.w_NAME);
                case LoadingImgType.Item:
                    return OrangeTextDataManager.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(imgInfo.w_NAME);
                case LoadingImgType.Skill:
                    return OrangeTextDataManager.Instance.SKILLTEXT_TABLE_DICT.GetL10nValue(imgInfo.w_NAME);
                default:
                    return OrangeTextDataManager.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(imgInfo.w_NAME);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OrangeDataManager), nameof(OrangeDataManager.Initialize))]
        internal static void ReadCharTableLoadingInfo(OrangeDataManager __instance)
        {
            if (!isCharTableLoaded)
            {
                var characterByLoading = OrangeTableHelper.Instance.GetCharacterByLoading();
                foreach (var imgInfo in characterByLoading.Select(DeserializeLoadingImg_CharTable))
                {
                    AddtoLoadingImgList(imgInfo);
                    LockImgList.Add($"{imgInfo.s_NAME}");
                }
                Configuration.AddLoadingImgConfigEntry(LockImgList);
                isCharTableLoaded = true;
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(TiptLoadingUI), nameof(TiptLoadingUI.Setup))]
        internal static bool LoadingSetup_Prefix(TiptLoadingUI __instance)
        {
            __instance.textProgess.text = "0%";
            __instance.UpdateTip();

            var characterByLoading = OrangeTableHelper.Instance.GetCharacterByLoading();
            if (__instance.isSystemInit)
            {         
                LoadingImgInfo loadingImgInfo;
                if (!string.IsNullOrEmpty(Configuration.LockImage.Value))
                    loadingImgInfo = AllLoadingImgs.Find(x => x.s_NAME == Configuration.LockImage.Value);
                else
                    loadingImgInfo = PickRandomLoadingImg();

                __instance.textCharacterName.text = LoadStringByTableType(loadingImgInfo);
                var imgName = loadingImgInfo.s_IMG;

                AssetsBundleManager.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.m_texture_loading, imgName, new Action<Sprite>((Sprite spr) =>
                {
                    if (spr != null)
                    {
                        __instance.imgCharacter.sprite = spr;
                        __instance.imgCharacter.color = Color.white;
                    }
                    __instance.IsComplete = true;
                }), AssetKeepMode.KEEP_IN_SCENE);

                // do not run original code
                return false;
            }
            
            __instance.textCharacterName.text = OrangeTextDataManager.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(characterByLoading[0].w_NAME);
            __instance.imgCharacter.color = Color.white;
            __instance.IsComplete = true;

            // do not run original code
            return false;
        }
    }
}
