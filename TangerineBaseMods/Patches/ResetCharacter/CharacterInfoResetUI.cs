using CallbackDefs;
using enums;
using Il2CppInterop.Runtime.Injection;
using OrangeConsoleService;
using StageLib;
using System;
using System.Linq;
using System.Collections.Generic;
using TangerineBaseMods.Config;
using Tangerine.Patchers.LogicUpdate;
using UnityEngine;
using UnityEngine.UI;

namespace TangerineBaseMods.Patches;

public class CharacterInfoResetUI : OrangeUIBase //, ITangerineLogicUpdate
{
    /*IntPtr ITangerineLogicUpdate.LogicPointer
    {
        get { return base.Pointer; }
    }

    void ITangerineLogicUpdate.LogicUpdate()
    {
    }*/

    public CharacterInfoResetUI(IntPtr ptr) : base(ptr)
    {
    }

    public CharacterInfoResetUI() : base(ClassInjector.DerivedConstructorPointer<CharacterInfoResetUI>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    private GameObject resetRoot;
    private Button resetBtn;
    private Button closeBtn;
    private Button bgClickBtn;
    private OrangeText resetLbl_Cost;
    private List<Button> resetTypeToggle = new();
    private List<Image> resetTypeMask = new();
    private List<ExpButtonRef> resetItems = new();
    private int resetType = 0;

    private global::CharacterInfo characterInfo;
    private CHARACTER_TABLE characterTable;
    private CharacterResetMaterials resetRewards = new();

    private enum bitIndex
    {
        _0 = 1,
        _1 = 2,
        _2 = 4,
        _3 = 8,
        _4 = 16,
        _5 = 32
    }

    private enum CharResetType : short
    {
        Active = 1,
        Passive = 2,
        DNA = 4,
        Skin = 8,
        Rank = 16,
        Sell = 32
    }

    public void Setup(global::CharacterInfo charInfo)
    {
        // initialize character data
        characterInfo = charInfo;
        characterTable = OrangeDataManager.Instance.CHARACTER_TABLE_DICT[characterInfo.netInfo.CharacterID];

        #region initialize UI gameobjects 
        resetRoot = this.transform.Find("takeoutroot").gameObject;
        foreach (var component in resetRoot.GetComponentsInChildren<Transform>(true))
        {
            if (component.name == "BGClick")
            {
                // set button onClick events
                bgClickBtn = component.GetComponent<Button>();
                bgClickBtn.onClick = new Button.ButtonClickedEvent();
                bgClickBtn.onClick.AddListener(new Action(base.OnClickCloseBtn));
            }
            if (component.name == "TitleText")
            {
                var text = component.GetComponent<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = "COMMON_RESET";
                text.text = LocalizationManager.Instance.GetStr("COMMON_RESET");
            }
            if (component.name == "CloseBtn")
            {
                // set button onClick events
                closeBtn = component.GetComponent<Button>();
                closeBtn.onClick = new Button.ButtonClickedEvent();
                closeBtn.onClick.AddListener(new Action(base.OnClickCloseBtn));
            }
            if (component.name == "TakeOutBtn")
            {
                // set button onClick events
                resetBtn = component.GetComponent<Button>();
                resetBtn.onClick = new Button.ButtonClickedEvent();
                resetBtn.onClick.AddListener(new Action(GoResetCharacter));
                resetBtn.interactable = false;

                var text = component.GetComponentInChildren<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = "FUNCTION_TAKEOUT_START";
                text.text = LocalizationManager.Instance.GetStr("FUNCTION_TAKEOUT_START");
            }
            if (component.name == "HeadBg0")
            {
                var text = component.GetComponentInChildren<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = "TAKEOUT_TYPE_SELECT";
                text.text = LocalizationManager.Instance.GetStr("TAKEOUT_TYPE_SELECT");
            }
            if (component.name == "HeadBg1")
            {
                var text = component.GetComponentInChildren<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = "GET_ITEM";
                text.text = LocalizationManager.Instance.GetStr("GET_ITEM");
            }
            if (component.name == "takeoutlbl0")
            {
                var text = component.GetComponent<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = "TAKEOUT_TIP_CHARACTER";
                text.text = LocalizationManager.Instance.GetStr("TAKEOUT_TIP_CHARACTER");
            }
            if (component.name == "takeoutlbl1")
            {
                resetLbl_Cost = component.GetComponent<OrangeText>();
                resetLbl_Cost.IsLocalizationText = true;
                resetLbl_Cost.LocalizationKey = "TAKEOUT_COST";
                resetLbl_Cost.text = string.Format(LocalizationManager.Instance.GetStr("TAKEOUT_COST"), 0);
            }
            if (component.name.StartsWith("takeouttypebtn"))
            {
                var toggle = component.GetComponent<Button>();
                var bitMask = typeIndexFromString(component.name);
                toggle.onClick = new Button.ButtonClickedEvent();
                toggle.onClick.AddListener(new Action(() => SetResetType(bitMask)));
                resetTypeToggle.Add(toggle);

                var mask = component.GetChild(1).GetComponent<Image>();
                resetTypeMask.Add(mask);

                var localizationKey = string.Empty;
                switch (component.name[^1])
                {
                    case '0':
                        localizationKey = "UI_SKILL_LV";
                        break;
                    case '1':
                        localizationKey = "FUNCTION_PASSIVE_SKILL";
                        break;
                    case '2':
                        localizationKey = "CHARACTER_DNA";
                        break;
                    case '3':
                        localizationKey = "FUNCTION_SKIN";
                        break;
                    case '4':
                        localizationKey = "COMMON_RANK";
                        break;
                    case '5':
                        localizationKey = "FUNCTION_SELL";
                        break;
                }
                var text = component.GetComponentInChildren<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = localizationKey;
                text.text = LocalizationManager.Instance.GetStr(localizationKey);
            }
            if (component.name.StartsWith("takeoutitem"))
            {
                var expBtnRef = new ExpButtonRef() { 
                    BtnLabel = component.GetChild(3).GetComponent<OrangeText>(),
                    BtnImgae = component.GetChild(1).GetComponent<StageLoadIcon>(),
                    Button = component.GetComponent<Button>(),
                    bgimg = component.GetComponent<StageLoadIcon>(),
                    frmimg = component.GetChild(0).GetComponent<StageLoadIcon>(),
                };
                resetItems.Add(expBtnRef);
            }
        }
        #endregion
    }

    private int typeIndexFromString(string componentName)
    {
        var sType = $"_{componentName[^1]}";
        if (Enum.TryParse(sType, out bitIndex idx))
            return (int)idx;
        else
            return 0;
    }

    private int typeIndexFromInt(int nType)
    {
        return Int32.Parse(((bitIndex)nType).ToString().Substring(1));
    }

    private void AddRtItem(ref List<WeaponInfoUI.expiteminfo> rtItems, ref WeaponInfoUI.expiteminfo tItem)
    {
        for (int i = 0; i < rtItems.Count; i++)
        {
            if (rtItems[i].tITEM_TABLE.n_ID == tItem.tITEM_TABLE.n_ID)
            {
                rtItems[i].nUseNum += tItem.nUseNum;
                return;
            }
        }
        rtItems.Add(tItem);
    }

    private void UpdateItemNeedInfo(ITEM_TABLE tITEM_TABLE, StageLoadIcon img, StageLoadIcon frm, StageLoadIcon bg, Text text)
    {
        img.CheckLoadT<Sprite>(AssetBundleScriptableObject.Instance.GetIconItem(tITEM_TABLE.s_ICON), tITEM_TABLE.s_ICON, null);
        OrangeRareText.Rare n_RARE = (OrangeRareText.Rare)tITEM_TABLE.n_RARE;
        frm.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareFrameSmall((int)n_RARE), null);
        bg.CheckLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, AssetBundleScriptableObject.Instance.GetIconRareBgSmall((int)n_RARE), null);
        if (text != null)
            text.text = ManagedSingleton<OrangeTextDataManager>.Instance.ITEMTEXT_TABLE_DICT.GetL10nValue(tITEM_TABLE.w_NAME);
    }

    private void UpdateToggles(int nType)
    {
        var isToggleEnabled = false;
        var typeMaskIndex = typeIndexFromInt(nType);

        if ((resetType & nType) != 0)
        {
            resetType &= ~nType;
            resetTypeMask[typeMaskIndex].gameObject.SetActive(false);
            isToggleEnabled = false;
        }
        else
        {
            resetType |= nType;
            resetTypeMask[typeMaskIndex].gameObject.SetActive(true);
            isToggleEnabled = true;
        }

        if (nType == (int)CharResetType.Rank)
        {
            UpdateToggleLock(isToggleEnabled, (int)CharResetType.Passive);
            UpdateToggleLock(isToggleEnabled, (int)CharResetType.DNA);
        }
        if (nType == (int)CharResetType.Sell)
        {
            UpdateToggleLock(isToggleEnabled, (int)CharResetType.Active);
            UpdateToggleLock(isToggleEnabled, (int)CharResetType.Passive);
            UpdateToggleLock(isToggleEnabled, (int)CharResetType.DNA);
            UpdateToggleLock(isToggleEnabled, (int)CharResetType.Skin);
            UpdateToggleLock(isToggleEnabled, (int)CharResetType.Rank);
        }
    }

    private void UpdateToggleLock(bool isToggleEnabled, int nType)
    {
        var maskIndex = typeIndexFromInt(nType);
        if (!isToggleEnabled)
        {
            resetType &= ~nType;
            resetTypeMask[maskIndex].gameObject.SetActive(false);

            resetTypeMask[maskIndex].color = Color.white;
            resetTypeToggle[maskIndex].interactable = true;
        }
        else
        {
            resetType |= nType;
            resetTypeMask[maskIndex].gameObject.SetActive(true);

            resetTypeMask[maskIndex].color = new Color(1, 0.8f, 1, 0.9f);
            resetTypeToggle[maskIndex].interactable = false;
        }
    }

    private void SetResetType(int nType)
    {
        if (nType == (int)CharResetType.Sell)
        {
            // prevent default character from being sold
            if (characterTable.n_ID == 1)
            {
                UIManager.Instance.LoadResourceUI<CommonUI>("UI_CommonMsg", new Action<CommonUI>((CommonUI ui) =>
                {
                    ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
                    ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
                    ui.SetupConfirmByKey("COMMON_TIP", "ERROR_SELL_DEFAULT_CHARACTER", "COMMON_OK", new Action(() => { }));
                }), true, false);
                return;
            }
        }
        AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK01);
        UpdateToggles(nType);

        var resetCost = 0;
        var resetBtnOverride = false;
        resetRewards = new();
        var list = new List<WeaponInfoUI.expiteminfo>();

        if ((resetType & (int)CharResetType.Active) != 0)
        {
            var skillZenny = 0;
            var skillSP = 0;
            var hasReturnableItems = false;
            foreach (var skillInfo in characterInfo.netSkillDic.Values)
            {
                if (skillInfo.Level > 1)
                {
                    for (int i = 1; i < skillInfo.Level; i++)
                    {
                        EXP_TABLE exp_TABLE = OrangeDataManager.Instance.EXP_TABLE_DICT[i];
                        skillZenny += exp_TABLE.n_SKILLUP_MONEY;
                        skillSP += exp_TABLE.n_SKILLUP_SP;
                    }
                }
            }
            if (skillZenny > 0)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_MONEY];
                expiteminfo.nUseNum = skillZenny;
                AddRtItem(ref list, ref expiteminfo);
                hasReturnableItems = true;
                resetRewards.ActiveMaterials.Add(OrangeConst.ITEMID_MONEY, skillZenny);
            }
            if (skillSP > 0)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[OrangeConst.ITEMID_SKILL_POINT];
                expiteminfo.nUseNum = skillSP;
                AddRtItem(ref list, ref expiteminfo);
                hasReturnableItems = true;
                resetRewards.ActiveMaterials.Add(OrangeConst.ITEMID_SKILL_POINT, skillSP);
            }
            if (hasReturnableItems)
                resetCost += Configuration.ResetCost.Value;
        }

        if ((resetType & (int)CharResetType.Passive) != 0)
        {
            int[] materialId = {
                characterTable.n_PASSIVE_MATERIAL1,
                characterTable.n_PASSIVE_MATERIAL2,
                characterTable.n_PASSIVE_MATERIAL3,
                characterTable.n_PASSIVE_MATERIAL4,
                characterTable.n_PASSIVE_MATERIAL5,
                characterTable.n_PASSIVE_MATERIAL6,
            };

            var hasReturnableItems = false;
            foreach (var netSkillInfo in characterInfo.netSkillDic.Values)
            {
                if (netSkillInfo.Slot >= 3)
                {
                    if (OrangeDataManager.Instance.MATERIAL_TABLE_DICT.TryGetValue(materialId[netSkillInfo.Slot-3], out MATERIAL_TABLE material_TABLE))
                        resetRewards.AddMaterials(ref resetRewards.PassiveMaterials, material_TABLE);
                }
            }
            foreach (var material in resetRewards.PassiveMaterials)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[material.Key];
                expiteminfo.nUseNum = material.Value;
                AddRtItem(ref list, ref expiteminfo);
                hasReturnableItems = true;
            }
            if (hasReturnableItems)
                resetCost += Configuration.ResetCost.Value;
        }

        if ((resetType & (int)CharResetType.DNA) != 0)
        {
            var hasReturnableItems = false;
            foreach (var dna_TABLE in OrangeDataManager.Instance.DNA_TABLE_DICT.Values)
            {
                if (dna_TABLE.n_CHARACTER == characterTable.n_ID)
                {
                    foreach (var dnaInfo in characterInfo.netDNAInfoDic.Values)
                    {
                        if (dnaInfo.SlotID == dna_TABLE.n_SLOT)
                        {
                            if (OrangeDataManager.Instance.MATERIAL_TABLE_DICT.TryGetValue(dna_TABLE.n_COST_ID, out MATERIAL_TABLE material_TABLE))
                                resetRewards.AddMaterials(ref resetRewards.DnaMaterials, material_TABLE);
                            break;
                        }
                    }
                }
            }
            if (characterInfo.netDNALinkInfo != null && resetRewards.DnaMaterials.Count == 0)
                resetBtnOverride = true;
            foreach (var material in resetRewards.DnaMaterials)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[material.Key];
                expiteminfo.nUseNum = material.Value;
                AddRtItem(ref list, ref expiteminfo);
                hasReturnableItems = true;
            }
            if (hasReturnableItems)
                resetCost += Configuration.ResetCost.Value;
        }

        if ((resetType & (int)CharResetType.Skin) != 0)
        {
            var hasReturnableItems = false;
            foreach (var skinId in characterInfo.netSkinList)
            {
                if (OrangeDataManager.Instance.SKIN_TABLE_DICT.TryGetValue(skinId, out SKIN_TABLE skin_TABLE))
                    resetRewards.AddMaterial(ref resetRewards.SkinMaterials, skin_TABLE.n_UNLOCK_ID, skin_TABLE.n_UNLOCK_COUNT);
            }
            foreach (var material in resetRewards.SkinMaterials)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[material.Key];
                expiteminfo.nUseNum = material.Value;
                AddRtItem(ref list, ref expiteminfo);
                hasReturnableItems = true;
            }
            if (hasReturnableItems)
                resetCost += Configuration.ResetCost.Value;
        }

        if ((resetType & (int)CharResetType.Rank) != 0)
        {
            var hasReturnableItems = false;
            foreach (var star_TABLE in OrangeDataManager.Instance.STAR_TABLE_DICT.Values)
            {
                if (star_TABLE.n_TYPE == 1 && star_TABLE.n_MAINID == characterInfo.netInfo.CharacterID)
                {
                    if (star_TABLE.n_STAR + 1 <= characterInfo.netInfo.Star)
                    {
                        if (OrangeDataManager.Instance.MATERIAL_TABLE_DICT.TryGetValue(star_TABLE.n_MATERIAL, out MATERIAL_TABLE material_TABLE))
                            resetRewards.AddMaterials(ref resetRewards.RankMaterials, material_TABLE);
                    }
                    if (star_TABLE.n_STAR + 1 == characterInfo.netInfo.Star)
                        break;
                }
            }
            foreach (var material in resetRewards.RankMaterials)
            {
                var expiteminfo = new WeaponInfoUI.expiteminfo();
                expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[material.Key];
                expiteminfo.nUseNum = material.Value;
                AddRtItem(ref list, ref expiteminfo);
                hasReturnableItems = true;
            }
            if (hasReturnableItems)
                resetCost += Configuration.ResetCost.Value;
        }

        if ((resetType & (int)CharResetType.Sell) != 0)
        {
            var expiteminfo = new WeaponInfoUI.expiteminfo();
            expiteminfo.tITEM_TABLE = ExtendDataHelper.Instance.ITEM_TABLE_DICT[characterTable.n_UNLOCK_ID];
            expiteminfo.nUseNum = characterTable.n_UNLOCK_COUNT;
            AddRtItem(ref list, ref expiteminfo);
            resetRewards.AddMaterial(ref resetRewards.SellMaterials, characterTable.n_UNLOCK_ID, expiteminfo.nUseNum);
        }
        resetLbl_Cost.text = string.Format(LocalizationManager.Instance.GetStr("TAKEOUT_COST"), resetCost);

        int itemIdx = 0;
        while (itemIdx < list.Count && itemIdx < resetItems.Count)
        {
            UpdateItemNeedInfo(list[itemIdx].tITEM_TABLE, resetItems[itemIdx].BtnImgae, resetItems[itemIdx].frmimg, resetItems[itemIdx].bgimg, null);
            resetItems[itemIdx].BtnLabel.text = list[itemIdx].nUseNum.ToString();
            resetItems[itemIdx].Button.gameObject.SetActive(true);
            itemIdx++;
        }
        for (int m = list.Count; m < resetItems.Count; m++)
            resetItems[m].Button.gameObject.SetActive(false);

        if (resetBtnOverride)
            resetBtn.interactable = true;
        else
            resetBtn.interactable = list.Count > 0 && PlayerHelper.Instance.GetTotalJewel() >= resetCost;
    }

    private void GoResetCharacter()
    {
        if (resetType == 0)
            return;

        var resetCost = 0;
        var netRewardsEntity = new NetRewardsEntity();
        
        if ((resetType & (int)CharResetType.Active) != 0)
        {
            var netCharacterSkillInfos = CharacterService.Instance.GetCharacterSkills(characterTable.n_ID);
            var netSkillDic = PlayerNetManager.Instance.dicCharacter[characterTable.n_ID].netSkillDic;
            foreach (var skillInfo in netCharacterSkillInfos)
            {
                if (skillInfo.Slot < 3)
                {
                    skillInfo.Level = 1;
                    netSkillDic[(CharacterSkillSlot)skillInfo.Slot] = skillInfo;
                }
            }
            resetCost += Configuration.ResetCost.Value;
        }

        if ((resetType & (int)CharResetType.Passive) != 0)
        {
            var netCharacterSkillInfos = CharacterService.Instance.GetCharacterSkills(characterTable.n_ID);
            var netSkillDic = PlayerNetManager.Instance.dicCharacter[characterTable.n_ID].netSkillDic;
            foreach (var skillInfo in netCharacterSkillInfos)
            {
                if (skillInfo.Slot >= 3)
                {
                    CharacterService.Instance._listCharacterSkill.Remove(skillInfo);
                    netSkillDic.Remove((CharacterSkillSlot)skillInfo.Slot);
                }
            }
            resetCost += Configuration.ResetCost.Value;
        }

        if ((resetType & (int)CharResetType.DNA) != 0)
        {
            for (int i = 1; i <= 8; i++)
            {
                var dnaInfo = CharacterService.Instance.GetDNASlot(characterTable.n_ID, i);
                CharacterService.Instance._listCharacterDNA.Remove(dnaInfo);
            }
            PlayerNetManager.Instance.dicCharacter[characterTable.n_ID].netDNAInfoDic.Clear();

            CharacterService.Instance._dicCharacterDNALink.Remove(characterTable.n_ID);
            PlayerNetManager.Instance.dicCharacter[characterTable.n_ID].netDNALinkInfo = null;
        }

        if ((resetType & (int)CharResetType.Skin) != 0)
        {
            var skinList = new List<NetCharacterSkinInfo>();
            foreach (var skinInfo in CharacterService.Instance._listCharacterSkin)
            { 
                if (skinInfo.CharacterID == characterTable.n_ID)
                    skinList.Add(skinInfo);
            }
            foreach (var skin in skinList)
                CharacterService.Instance._listCharacterSkin.Remove(skin);
            PlayerNetManager.Instance.dicCharacter[characterTable.n_ID].netSkinList.Clear();
            PlayerNetManager.Instance.mmapSkin.Remove(characterTable.n_ID);

            // remove equipped skin
            var netCharacterInfo = CharacterService.Instance.GetCharacter(characterTable.n_ID);
            netCharacterInfo.Skin = 0;
            PlayerNetManager.Instance.dicCharacter[characterTable.n_ID].netInfo = netCharacterInfo;
        }

        if ((resetType & (int)CharResetType.Rank) != 0)
        {
            var netCharacterInfo = CharacterService.Instance.GetCharacter(characterTable.n_ID);
            netCharacterInfo.Star = 0;
            PlayerNetManager.Instance.dicCharacter[characterTable.n_ID].netInfo = netCharacterInfo;

            // remove equipped skill chip
            var netCharacterSkillInfos = CharacterService.Instance.GetCharacterSkills(characterTable.n_ID);
            var netSkillDic = PlayerNetManager.Instance.dicCharacter[characterTable.n_ID].netSkillDic;
            foreach (var skillInfo in netCharacterSkillInfos)
            {
                if (skillInfo.Slot < 3)
                {
                    skillInfo.Extra = 0;
                    netSkillDic[(CharacterSkillSlot)skillInfo.Slot] = skillInfo;
                }
            }
            resetCost += Configuration.ResetCost.Value;
        }

        if ((resetType & (int)CharResetType.Sell) != 0)
        {
            // delete character from roster
            CharacterService.Instance._dicCharacter.Remove(characterTable.n_ID);
            PlayerNetManager.Instance.dicCharacter.Remove(characterTable.n_ID);

            // remove gallery info
            var characterExp = 0;
            var charGalleryInfo = new List<NetGalleryInfo>();
            foreach (var gallery in PlayerNetManager.Instance.galleryInfo.GalleryList)
            {
                if (OrangeDataManager.Instance.GALLERY_TABLE_DICT.TryGetValue(gallery.GalleryID, out GALLERY_TABLE gallery_TABLE))
                {
                    if (gallery_TABLE.n_TYPE == 1 && gallery_TABLE.n_MAINID == characterTable.n_ID)
                    {
                        charGalleryInfo.Add(gallery);
                        characterExp += gallery_TABLE.n_EXP;
                    }
                }
            }
            foreach (var galleryInfo in charGalleryInfo)
            {
                GalleryService.Instance._dicGallery.Remove(galleryInfo.GalleryID);
                PlayerNetManager.Instance.galleryInfo.GalleryList.Remove(galleryInfo);
            }

            // subtract gallery EXP
            if (GalleryService.Instance._dicGalleryExp.TryGetValue(1, out NetGalleryExpInfo galleryExpInfo))
                galleryExpInfo.Exp -= characterExp;
            foreach (var galleryInfo in PlayerNetManager.Instance.galleryInfo.GalleryExpList)
            {
                if (galleryInfo.GalleryType == 1)
                {
                    galleryInfo.Exp -= characterExp;
                    if (galleryInfo.Exp < 0)
                        galleryInfo.Exp = 0;
                }
            }
        }

        // subtract reset cost
        var resetCostInfo = ItemService.Instance.GetItem(OrangeConst.ITEMID_FREE_JEWEL);
        resetCostInfo.Stack -= resetCost;
        PlayerNetManager.Instance.dicItem.Value(resetCostInfo.ItemID).netItemInfo = resetCostInfo;

        // create reward items
        resetRewards.ComposeRewards(ref netRewardsEntity);

        // send rewards
        if (netRewardsEntity.RewardList != null && netRewardsEntity.RewardList.Count > 0)
        {
            PlayerNetManager.Instance.ComposeRewardEntities(netRewardsEntity);
            UIManager.Instance.LoadUI<RewardPopopUI>("UI_RewardPopup", new Action<RewardPopopUI>((RewardPopopUI ui) =>
            {
                ui.Setup(netRewardsEntity.RewardList, 0f);
            }));
        }
        RefreshUI();

        AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK05);
        base.OnClickCloseBtn();
    }

    private void RefreshUI()
    {
        // get active UI instances
        var mainUI = UIManager.Instance.GetUI<CharacterInfoUI>("UI_CharacterInfo_Main");
        var basicUI = UIManager.Instance.GetUI<CharacterInfoBasic>("UI_CharacterInfo_Basic");
        var goCheckUI = UIManager.Instance.GetUI<GoCheckUI>("UI_GoCheck");

        // sort characters
        Il2CppSystem.Collections.Generic.List<global::CharacterInfo> sortedList = new();
        if (goCheckUI != null)
            sortedList = CharacterHelper.Instance.SortCharacterListNoFragmentNoSave();
        else
            sortedList = CharacterHelper.Instance.SortCharacterList();

        // refresh character model
        var characterChange = false;
        var characterId = characterTable.n_ID;
        if ((resetType & (int)CharResetType.Sell) != 0)
        {
            if (characterId == PlayerNetManager.Instance.playerInfo.netPlayerInfo.StandbyChara)
                OrangeGameManager.Instance.CharacterStandby(1, (Callback)new Action(() => { }), true);
            if (goCheckUI != null)
            {
                var characterIdx = mainUI.GetCurrentSelectionIndex();
                if (characterIdx >= sortedList.Count)
                    characterIdx = sortedList.Count - 1;

                characterId = sortedList[characterIdx].netInfo.CharacterID;
                if (characterId != characterTable.n_ID)
                    characterChange = true;

                mainUI.scrollRect.totalCount = sortedList.Count;
            }
        }

        // refresh character data
        foreach (var charInfo in sortedList)
        {
            if (charInfo.netInfo.CharacterID == characterId)
            {
                mainUI.characterInfo = charInfo;
                mainUI.characterTable = OrangeDataManager.Instance.CHARACTER_TABLE_DICT[mainUI.characterInfo.netInfo.CharacterID];
                basicUI.characterInfo = mainUI.characterInfo;
                mainUI.characterTable = mainUI.characterTable;

                if (OrangeDataManager.Instance.SKIN_TABLE_DICT.TryGetValue(mainUI.characterInfo.netInfo.Skin, out SKIN_TABLE skinTable))
                    mainUI.m_skinTable = skinTable;
                if (characterChange)
                    mainUI.RefreshMenu(false);
                else
                {
                    SKIN_TABLE skin_TABLE;
                    OrangeDataManager.Instance.SKIN_TABLE_DICT.TryGetValue(mainUI.characterInfo.netInfo.Skin, out skin_TABLE);
                    mainUI.RefreshModel(skin_TABLE);
                    mainUI.RefreshPortrait(skin_TABLE);
                }
                break;
            }
        }

        // refresh main UI
        mainUI.CheckCharacterUpgrades();
        mainUI.RefreshSideButtonRedDots();
        mainUI.RefreshBadges();
        mainUI.RefreshQuickSelectBar(true);

        // refresh basic UI
        if (!characterChange)
        {
            basicUI.RefreshDeployButton();
            basicUI.RefreshTips();
            basicUI.RefreshRecordVal();

            // refresh basic UI - unlock group
            if (basicUI.m_materialIcon != null && basicUI.characterInfo.netInfo.State == 2)
            {
                var component = basicUI.m_materialIcon.GetComponent<CommonIconBase>();
                component.SetupItem(basicUI.characterTable.n_UNLOCK_ID, 0, component.callback, true);
            }
            basicUI.RefreshUnlockGroup();

            // refresh basic UI - skill data
            foreach (var skillBtnRoot in basicUI.skillButtonPositions)
            {
                for (int i = 0; i < 4; i++)
                    GameObject.Destroy(skillBtnRoot.transform.GetChild(i).transform.GetChild(0).gameObject);
                GameObject.Destroy(skillBtnRoot.transform.GetChild(3).gameObject);
            }
            basicUI.CreateSkillButton(0);
            basicUI.CreateSkillButton(1);
        }

        // refresh topbar
        GenericEventManager.Instance.NotifyEvent(EventManager.ID.UPDATE_TOPBAR_DATA);
    }
}
