using enums;
using CallbackDefs;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tangerine.Manager.Mod;

namespace TangerineBaseMods;

public class CharacterPassives
{
    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony, JsonNode node)
    {
        
        if (node["CharacterPassives"]["enabled"].Deserialize<bool>())
        {
            harmony.PatchAll(typeof(CharacterPassives));
        }
        else
        {
            tangerine.Loader.RemoveAssetBundleId("ui/ui_characterinfo_skill");
            tangerine.Loader.RemoveAssetBundleId("texture/2d/ui/ui_character");
        }
    }


    [HarmonyPrefix, HarmonyPatch(typeof(CharacterInfoSkill), nameof(CharacterInfoSkill.SetupPassiveSkillButton))]
    private static bool SetupPassiveSkillButton_Prefix(CharacterInfoSkill __instance, CharacterSkillSlot skillSlot, SkillButton skillButton)
    {
        SkillButton.StatusType statusType = SkillButton.StatusType.LOCKED;
        if (__instance.IsSkillUnlocked(skillSlot, CharacterSkillEnhanceSlot.None))
        {
            statusType = SkillButton.StatusType.UNLOCKED;
        }
        skillButton.SetStyle(SkillButton.StyleType.WIDE);
        if (skillSlot == CharacterSkillSlot.PassiveSkill1)
        {
            skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, __instance.characterTable.n_PASSIVE_1, 1, statusType);
        }
        else if (skillSlot == CharacterSkillSlot.PassiveSkill2)
        {
            skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, __instance.characterTable.n_PASSIVE_2, 1, statusType);
        }
        else if (skillSlot == CharacterSkillSlot.PassiveSkill3)
        {
            skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, __instance.characterTable.n_PASSIVE_3, 1, statusType);
        }
        else if (skillSlot == CharacterSkillSlot.PassiveSkill4)
        {
            skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, __instance.characterTable.n_PASSIVE_4, 1, statusType);
        }
        else if (skillSlot == CharacterSkillSlot.PassiveSkill5)
        {
            skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, __instance.characterTable.n_PASSIVE_5, 1, statusType);
        }
        else if (skillSlot == CharacterSkillSlot.PassiveSkill6)
        {
            skillButton.Setup(skillSlot, CharacterSkillEnhanceSlot.None, __instance.characterTable.n_PASSIVE_6, 1, statusType);
        }

        skillButton.EnableRedDot(__instance.bIsCharacterUnlocked && statusType == SkillButton.StatusType.LOCKED && __instance.IsPassiveSkillUnlockable(skillSlot));
        NetCharacterSkillInfo netCharacterSkillInfo;
        if (__instance.characterInfo.netSkillDic.TryGetValue(skillSlot, out netCharacterSkillInfo))
        {
            string text = string.Format("{0}:{1}", MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RANKING_PERSONAL_LEVEL"), netCharacterSkillInfo.Level);
            skillButton.OverrideText(text);

            // do not run original code
            return false;
        }
        string str = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("UI_LOCKED");
        skillButton.OverrideText(str);

        // do not run original code
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterInfoSkill), nameof(CharacterInfoSkill.CreateSkillButtons))]
    private static bool CreateSkillButtons_Prefix(CharacterInfoSkill __instance)
    {
        __instance.m_activeSkill1Buttons.Clear();
        __instance.m_activeSkill2Buttons.Clear();

        MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>("ui/skillbutton", "SkillButton", new System.Action<GameObject>((GameObject asset) =>
        {
            if (asset != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(asset, __instance.selectedSkillIconPos.transform, false);
                gameObject.GetComponent<SkillButton>().SetStyle(SkillButton.StyleType.SQUARE);
                gameObject.GetComponent<SkillButton>().Setup(10001, SkillButton.StatusType.DEFAULT);

                // skills + skill chips
                for (int i = 0; i < 4; i++)
                {
                    int indexForDelegate2 = i;
                    GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(asset, __instance.activeSkill1ButtonPos[i].transform, false);
                    gameObject2.GetComponentInChildren<Button>().onClick.AddListener(new System.Action(() =>
                    {
                        __instance.OnClickActiveSkillButton(indexForDelegate2);
                    }));

                    GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(asset, __instance.activeSkill2ButtonPos[i].transform, false);
                    gameObject3.GetComponentInChildren<Button>().onClick.AddListener(new System.Action(() =>
                    {
                        __instance.OnClickActiveSkillButton(indexForDelegate2 + 4);
                    }));

                    __instance.m_activeSkill1Buttons.Add(gameObject2.GetComponent<SkillButton>());
                    __instance.m_activeSkill2Buttons.Add(gameObject3.GetComponent<SkillButton>());
                    __instance.SetupActiveSkillButton(CharacterSkillSlot.ActiveSkill1, (CharacterSkillEnhanceSlot)i, gameObject2.GetComponent<SkillButton>());
                    __instance.SetupActiveSkillButton(CharacterSkillSlot.ActiveSkill2, (CharacterSkillEnhanceSlot)i, gameObject3.GetComponent<SkillButton>());
                }

                // passive skills
                for (int j = 0; j < 6; j++)
                {
                    int indexForDelegate = j;
                    GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(asset, __instance.passiveSkillButtonPos[j].transform, false);
                    gameObject4.GetComponentInChildren<Button>().onClick.AddListener(new System.Action(() =>
                    {
                        __instance.OnClickPassiveSkillButton(indexForDelegate);
                    }));

                    CharacterSkillSlot characterSkillSlot = (CharacterSkillSlot)(3 + j);
                    __instance.SetupPassiveSkillButton(characterSkillSlot, gameObject4.GetComponent<SkillButton>());
                }
            }
        }), 0); // AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE

        // do not run original code
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterInfoSkill), nameof(CharacterInfoSkill.DisplayPassiveSkillInfo))]
    private static bool DisplayPassiveSkillInfo_Prefix(CharacterInfoSkill __instance, SkillButton skillButton, int index)
    {
        __instance.requirementGroup.gameObject.SetActive(true);
        if (skillButton.GetSkillName() != null)
        {
            __instance.selectedSkillName.text = skillButton.GetSkillName();
        }
        if (skillButton.GetSkillDescription() != null)
        {
            __instance.selectedSkillDescription.SetText(string.Format(skillButton.GetSkillDescription(), skillButton.GetSkillEffect()));
        }

        SkillButton componentInChildren = __instance.selectedSkillIconPos.GetComponentInChildren<SkillButton>();
        componentInChildren.SetStyle(SkillButton.StyleType.SQUARE);
        componentInChildren.Setup(skillButton.GetSkillID(), SkillButton.StatusType.DEFAULT);
        componentInChildren.OverrideText("");

        // get passive skill unlock requirement ("Character must be {0} Star Rank(s).")
        int num = 0;
        if (index == 0)
        {
            num = __instance.characterTable.n_PASSIVE_UNLOCK1;
        }
        else if (index == 1)
        {
            num = __instance.characterTable.n_PASSIVE_UNLOCK2;
        }
        else if (index == 2)
        {
            num = __instance.characterTable.n_PASSIVE_UNLOCK3;
        }
        else if (index == 3)
        {
            num = __instance.characterTable.n_PASSIVE_UNLOCK4;
        }
        else if (index == 4)
        {
            num = __instance.characterTable.n_PASSIVE_UNLOCK5;
        }
        else if (index == 5)
        {
            num = __instance.characterTable.n_PASSIVE_UNLOCK6;
        }
        __instance.UnlockRequirement.text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("RESTRICT_CHARA_STAR"), num);
        if (num > (int)__instance.characterInfo.netInfo.Star)
        {
            __instance.UnlockRequirement.color = new Color(0.98f, 0.24f, 0.23f);
        }
        else
        {
            __instance.UnlockRequirement.color = new Color(1f, 1f, 1f);
        }

        // get passive skill unlock materials
        MATERIAL_TABLE passiveSkillUnlockMaterialTable = null;
        if (index == 0)
        {
            ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(__instance.characterTable.n_PASSIVE_MATERIAL1, out passiveSkillUnlockMaterialTable);
        }
        else if (index == 1)
        {
            ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(__instance.characterTable.n_PASSIVE_MATERIAL2, out passiveSkillUnlockMaterialTable);
        }
        else if (index == 2)
        {
            ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(__instance.characterTable.n_PASSIVE_MATERIAL3, out passiveSkillUnlockMaterialTable);
        }
        else if (index == 3)
        {
            ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(__instance.characterTable.n_PASSIVE_MATERIAL4, out passiveSkillUnlockMaterialTable);
        }
        else if (index == 4)
        {
            ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(__instance.characterTable.n_PASSIVE_MATERIAL5, out passiveSkillUnlockMaterialTable);
        }
        else if (index == 5)
        {
            ManagedSingleton<OrangeDataManager>.Instance.MATERIAL_TABLE_DICT.TryGetValue(__instance.characterTable.n_PASSIVE_MATERIAL6, out passiveSkillUnlockMaterialTable);
        }
        __instance.passiveSkillUnlockMaterialTable = passiveSkillUnlockMaterialTable;

        // get the items from material table
        ITEM_TABLE item_TABLE = null;
        bool flag = true;
        for (int i = 0; i < 4; i++)
        {
            if (i == 0)
            {
                ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(__instance.passiveSkillUnlockMaterialTable.n_MATERIAL_1, out item_TABLE);
            }
            else if (i == 1)
            {
                ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(__instance.passiveSkillUnlockMaterialTable.n_MATERIAL_2, out item_TABLE);
            }
            else if (i == 2)
            {
                ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(__instance.passiveSkillUnlockMaterialTable.n_MATERIAL_3, out item_TABLE);
            }
            else if (i == 3)
            {
                ManagedSingleton<ExtendDataHelper>.Instance.ITEM_TABLE_DICT.TryGetValue(__instance.passiveSkillUnlockMaterialTable.n_MATERIAL_4, out item_TABLE);
            }
            CommonIconBase componentInChildren2 = __instance.passiveSkillMaterialPos[i].GetComponentInChildren<CommonIconBase>();
            componentInChildren2.transform.parent.gameObject.SetActive(item_TABLE != null);
            if (item_TABLE != null)
            {
                flag &= componentInChildren2.SetupMaterialEx(__instance.passiveSkillUnlockMaterialTable.n_ID, i, (CallbackIdx)new System.Action<int>(__instance.OnClickMaterialIcon));
            }
        }

        __instance.PassiveSkillUnlockMoney.text = __instance.passiveSkillUnlockMaterialTable.n_MONEY.ToString();
        flag &= __instance.passiveSkillUnlockMaterialTable.n_MONEY <= ManagedSingleton<PlayerHelper>.Instance.GetZenny();
        flag &= num <= (int)__instance.characterInfo.netInfo.Star;
        __instance.UpgradeUnlockButton.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("FUNCTION_UNLOCK");
        __instance.UpgradeUnlockButton.gameObject.SetActive(true);
        __instance.UpgradeUnlockButton2.gameObject.SetActive(false);
        __instance.QUpgradeButton.gameObject.SetActive(false);

        if (__instance.bIsCharacterUnlocked)
        {
            bool flag2 = __instance.IsSkillUnlocked(skillButton.GetCharacterSkillSlot(), CharacterSkillEnhanceSlot.None);
            __instance.UpgradeUnlockButton.interactable = !flag2 && flag;
            __instance.requirementGroup.gameObject.SetActive(!flag2);
            __instance.imgUnlockedBg.gameObject.SetActive(flag2);
            if (flag2)
            {
                __instance.UpgradeUnlockButton.GetComponentInChildren<Text>().text = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("REACH_MAX");

                // do not run original code
                return false;
            }
        }
        else
        {
            __instance.UpgradeUnlockButton.interactable = false;
            __instance.requirementGroup.gameObject.SetActive(false);
            __instance.imgUnlockedBg.gameObject.SetActive(true);
        }

        // do not run original code
        return false;
    }
}