using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using CallbackDefs;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.UI;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace TangerineBaseMods.Patches;

internal static class ExpandedShopTabs
{
    private static ShopTopUI uiShop;
    private static bool updatedShopValues = false;
    private static ShopSelectTab _shopSelectTab = ShopSelectTab.item_shop;
    
    private static ShopMainType _shopMainType = ShopMainType.permanent;
    private static ShopCollabType _shopCollabType = ShopCollabType.character;
    private static ShopCustomType _shopCustomType = ShopCustomType.character;

    private static List<ShopMainType> _listMain = new();
    private static List<ShopCollabType> _listCollab = new();
    private static List<ShopCustomType> _listCustom = new();

    private static List<SHOP_TABLE> _listShopItemAll = new();
    private static List<SHOP_TABLE> _listShopItemNow = new();

    private static readonly int COLLAB_TYPE = 100;
    private static readonly int CUSTOM_TYPE = 200;

    internal static void InitializeHarmony(Harmony harmony)
    {
        harmony.PatchAll(typeof(ExpandedShopTabs));

        EnumInjector.InjectEnumValues<ShopTopUI.ShopSelectTab>(new Dictionary<string, object>() {
            { "collab", ShopSelectTab.collab },
            { "custom", ShopSelectTab.custom }
        });
    }

    private static string GetSubKeyByMainType(ShopMainType type)
    {
        return "SHOP_TAB_001_" + ((int)type).ToString("D3");
    }

    private static string GetSubKeyByCollabType(ShopCollabType type)
    {
        return "SHOP_TAB_002_" + ((int)type).ToString("D3");
    }

    private static string GetSubKeyByCustomType(ShopCustomType type)
    {
        return "SHOP_TAB_003_" + ((int)type).ToString("D3");
    }

    private static void MainTypeExist(ShopMainType p_type)
    {
        if (_listShopItemAll.Exists((SHOP_TABLE x) => x.n_MAIN_TYPE == (int)p_type))
            _listMain.Add(p_type);
    }

    private static void CollabTypeExist(ShopCollabType p_type)
    {
        if (_listShopItemAll.Exists((SHOP_TABLE x) => x.n_MAIN_TYPE == (int)p_type + COLLAB_TYPE))
            _listCollab.Add(p_type);
    }

    private static void CustomTypeExist(ShopCustomType p_type)
    {
        if (_listShopItemAll.Exists((SHOP_TABLE x) => x.n_MAIN_TYPE == (int)p_type + CUSTOM_TYPE))
            _listCustom.Add(p_type);
    }

    private static bool SubTypeExist(ref List<SHOP_TABLE> refList, int type)
    {
        return refList.Exists((SHOP_TABLE x) => x.n_SUB_TYPE == type);
    }

    private static List<SHOP_TABLE> GetShopListByMainType(int typeMain)
    {
        return _listShopItemAll.Where((SHOP_TABLE x) => x.n_MAIN_TYPE == typeMain).ToList();
    }

    private static List<SHOP_TABLE> GetShopListByMainAndSubType(int typeMain, int tpyeSub)
    {
        return _listShopItemAll.Where((SHOP_TABLE x) => x.n_MAIN_TYPE == typeMain && x.n_SUB_TYPE == tpyeSub && uiShop.CheckPre(ref x)).ToList();
    }

    private static List<T> ConvertIl2CppList<T>(Il2CppSystem.Collections.Generic.List<T> Il2CppList)
    {
        var _list = new List<T>();
        foreach (var item in Il2CppList) _list.Add(item); 
        return _list;
    }

    private static Il2CppSystem.Collections.Generic.List<T> ConvertSystemList<T>(List<T> SystemList)
    {
        var _list = new Il2CppSystem.Collections.Generic.List<T>();
        SystemList.ForEach(_list.Add);
        return _list;
    }

    internal enum ShopSelectTab
    {
        directproduct,
        item_shop,
        collab,
        custom
    }

    internal enum ShopMainType
    {
        permanent = 1,
        character,
        weapon,
        card = 5,
        events,
        skin
    }

    internal enum ShopCollabType
    {
        character = 1,
        weapon,
        card,
        skin,
        chip,
        item
    }

    internal enum ShopCustomType
    {
        character = 1,
        weapon,
        card,
        skin,
        chip,
        item
    }

    [HarmonyPrefix, HarmonyPatch(typeof(HometopSceneController), nameof(HometopSceneController.SceneInit))]
    private static void UpdateOldShopValues(HometopSceneController __instance)
    {
        if (!updatedShopValues)
        {
            foreach (var shopItem in OrangeDataManager.Instance.SHOP_TABLE_DICT)
            {
                if (shopItem.Value.n_MAIN_TYPE == 8)
                {
                    shopItem.Value.n_MAIN_TYPE = shopItem.Value.n_SUB_TYPE + COLLAB_TYPE;
                    shopItem.Value.n_SUB_TYPE = 1;
                    shopItem.Value.w_SHEET_NAME = "SHOP_SHEET_NORMAL";
                }

                if (shopItem.Value.n_MAIN_TYPE == 9)
                {
                    shopItem.Value.n_MAIN_TYPE = shopItem.Value.n_SUB_TYPE + CUSTOM_TYPE;
                    shopItem.Value.n_SUB_TYPE = 1;
                    shopItem.Value.w_SHEET_NAME = "SHOP_SHEET_NORMAL";
                }
            }

            foreach (var item in OrangeDataManager.Instance.ITEM_TABLE_DICT)
            {
                var getValues = item.Value.s_HOWTOGET.Split(",");
                for (int i = 0; i < getValues.Length; i++)
                {
                    switch (getValues[i])
                    {
                        // collab character
                        case "42":
                            getValues[i] = "10101";
                            break;

                        // collab weapon
                        case "43":
                            getValues[i] = "10201";
                            break;

                        // collab card
                        case "44":
                            getValues[i] = "10301";
                            break;

                        // custom character
                        case "45":
                            getValues[i] = "20101";
                            break; ;

                        // custom weapon
                        case "46":
                            getValues[i] = "20201";
                            break;

                        // custom card
                        case "47":
                            getValues[i] = "20301";
                            break;
                    }
                }
                item.Value.s_HOWTOGET = string.Join(",", getValues);
            }
            updatedShopValues = true;
        }
    }

    private static void ClearDicts()
    {
        _listMain.Clear();
        _listCollab.Clear();
        _listCustom.Clear();

        _listShopItemAll.Clear();
        _listShopItemNow.Clear();
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ShopTopUI), nameof(ShopTopUI.Setup))]
    private static bool Setup(ShopTopUI __instance, ShopTopUI.ShopSelectTab p_selectTab, ShopTopUI.ShopSubType p_nowSub)
    {
        uiShop = __instance;
        _shopSelectTab = (ShopSelectTab)p_selectTab;
        ClearDicts();

        uiShop.shopBlock.enabled = true;
        uiShop.nowSub = p_nowSub;
        uiShop.m_preSceneBgm = new string[]
        {
            AudioManager.Instance.bgmSheet,
            AudioManager.Instance.bgmCue
        };
    
        if (OrangePlayerLocalData.Instance.saveData.NewShopItemFlag)
            uiShop.OnShowNewShopItemMessage();
    
        PlayerNetManager.Instance.RetrieveShopRecordReq(new Action(() =>
        {
            _listShopItemAll = ConvertIl2CppList(ExtendDataHelper.Instance.GetShopTableByOpening(OrangeGameManager.Instance.ServerUnixTimeNowUTC));
            uiShop.TimeNow = OrangeGameManager.Instance.ServerUnixTimeNowLocale;
            uiShop.coroutineHandle = uiShop.StartCoroutine(uiShop.OnStartUpdateTime());
    
            foreach (var type in Enum.GetValues(typeof(ShopMainType)))
                MainTypeExist((ShopMainType)type);
            foreach (var type in Enum.GetValues(typeof(ShopCollabType)))
                CollabTypeExist((ShopCollabType)type);
            foreach (var type in Enum.GetValues(typeof(ShopCustomType)))
                CustomTypeExist((ShopCustomType)type);
            CreateNewStorageTab();
        }));
        
        AssetsBundleManager.Instance.GetAssetAndAsyncLoad<GameObject>(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, "ch_navi_0"), "ch_navi_0_db", new Action<GameObject>((GameObject obj) =>
        {
            StandNaviDb component = UnityEngine.Object.Instantiate(obj, uiShop.stParent, false).GetComponent<StandNaviDb>();
            if (component)
                component.Setup(StandNaviDb.NAVI_DB_TYPE.NORMAL, 0, null);
            uiShop.PlayBgm();
        }), AssetKeepMode.KEEP_IN_SCENE);
    
        // do not run original code
        return false;
    }
    
    private static void CreateNewStorageTab()
    {
        var tabList = new List<ShopSelectTab>();
        if (_listMain.Count > 0)
        {
            var storageInfo = new StorageInfo("SHOP_TAB_001_000", false, _listMain.Count);
            for (int i = 0; i < _listMain.Count; i++)
            {
                storageInfo.Sub[i] = new StorageInfo(GetSubKeyByMainType(_listMain[i]), false, 0, (CallbackObj)new Action<Il2CppSystem.Object>(uiShop.OnClickTab));
                storageInfo.Sub[i].Param = new Il2CppSystem.Object[]
                {
                    (int)ShopSelectTab.item_shop,
                    (int)_listMain[i]
                };
            }
            uiShop.listStorage.Add(storageInfo);
            tabList.Add(ShopSelectTab.item_shop);
        }
        else
        {
            StorageInfo storageInfo = new StorageInfo("SHOP_TAB_001_000", false, 0, (CallbackObj)new Action<Il2CppSystem.Object>(uiShop.OnClickTab));
            storageInfo.Param = new Il2CppSystem.Object[]
            {
                (int)ShopSelectTab.item_shop
            };
            uiShop.listStorage.Add(storageInfo);
            tabList.Add(ShopSelectTab.item_shop);
        }

        if (_listCollab.Count > 0)
        {
            var storageInfo2 = new StorageInfo("SHOP_TAB_002_000", false, _listCollab.Count);
            for (int i = 0; i < _listCollab.Count; i++)
            {
                storageInfo2.Sub[i] = new StorageInfo(GetSubKeyByCollabType(_listCollab[i]), false, 0, (CallbackObj)new Action<Il2CppSystem.Object>(uiShop.OnClickTab));
                storageInfo2.Sub[i].Param = new Il2CppSystem.Object[]
                {
                    (int)ShopSelectTab.collab,
                    (int)_listCollab[i]
                };
            }
            uiShop.listStorage.Add(storageInfo2);
            tabList.Add(ShopSelectTab.collab);
        }
        if (_listCustom.Count > 0)
        {
            var storageInfo3 = new StorageInfo("SHOP_TAB_003_000", false, _listCustom.Count);
            for (int i = 0; i < _listCustom.Count; i++)
            {
                storageInfo3.Sub[i] = new StorageInfo(GetSubKeyByCustomType(_listCustom[i]), false, 0, (CallbackObj)new Action<Il2CppSystem.Object>(uiShop.OnClickTab));
                storageInfo3.Sub[i].Param = new Il2CppSystem.Object[]
                {
                    (int)ShopSelectTab.custom,
                    (int)_listCustom[i]
                };
            }
            uiShop.listStorage.Add(storageInfo3);
            tabList.Add(ShopSelectTab.custom);
        }

        // get store tab and subtab
        var tabIdx = 0;
        var shopIdx = tabList.FindIndex((ShopSelectTab x) => x == _shopSelectTab);
        if (shopIdx == -1) shopIdx = 0;
        else
        {
            switch (_shopSelectTab)
            {
                case ShopSelectTab.item_shop:
                    tabIdx = _listMain.FindIndex((ShopMainType x) => x == (ShopMainType)uiShop.DefaultSubIdx);
                    break;

                case ShopSelectTab.collab:
                    tabIdx = _listCollab.FindIndex((ShopCollabType x) => x == (ShopCollabType)uiShop.DefaultSubIdx);
                    break;

                case ShopSelectTab.custom:
                    tabIdx = _listCustom.FindIndex((ShopCustomType x) => x == (ShopCustomType)uiShop.DefaultSubIdx);
                    break;
            }
            if (tabIdx == -1) tabIdx = 0;
        }
        StorageGenerator.Load("StorageComp00", uiShop.listStorage, shopIdx, tabIdx, uiShop.storageRoot);
    
        // create scrolling tabs
        /*AssetsBundleManager.Instance.GetAssetAndAsyncLoad<GameObject>("ui/tabscrollviewcomp00", "TabScrollViewComp00", new Action<GameObject>((GameObject obj) =>
        {
            var tabScrollView = UnityEngine.Object.Instantiate(obj.transform.Find("TabScrollView"), uiShop.storageRoot);
            var scrollRect = tabScrollView.GetComponent<ScrollRect>();
            //scrollRect.content.sizeDelta = new Vector2(0f, 150f * (float)12 - 1f + 100f);
    
            var rectTransform = uiShop.storageRoot.GetComponent<RectTransform>();
            //rectTransform.anchoredPosition = new Vector2(122, -315.7002f);

            StorageGenerator.Load("StorageComp00", uiShop.listStorage, (int)_shopSelectTab, num, uiShop.storageRoot);
        }), AssetKeepMode.KEEP_IN_SCENE);*/
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ShopTopUI), nameof(ShopTopUI.OnClickTab))]
    private static bool OnClickTab(Il2CppSystem.Object p_param)
    {
        uiShop.scrollRect.ClearCells();
        var storageInfo = p_param.Cast<StorageInfo>();
        var shopSelectTab = (ShopSelectTab)storageInfo.Param[0].Unbox<int>();
        if (_shopSelectTab != shopSelectTab)
            uiShop.nowSub = ShopTopUI.ShopSubType.sub_1;
        _shopSelectTab = shopSelectTab;
    
        switch (_shopSelectTab)
        {
            case ShopSelectTab.item_shop:
                uiShop.contentGridLayout.SetPadding(uiShop.offsetShop);
                uiShop.contentGridLayout.UpdateValue(uiShop.cellSizeShop, uiShop.spacingShop, uiShop.constraintCountShop);
                if (_listMain.Count > 0)
                    _shopMainType = (ShopMainType)storageInfo.Param[1].Unbox<int>();
                OnClickSubTab(uiShop.nowSub);
                break;
    
            case ShopSelectTab.collab:
                uiShop.contentGridLayout.SetPadding(uiShop.offsetShop);
                uiShop.contentGridLayout.UpdateValue(uiShop.cellSizeShop, uiShop.spacingShop, uiShop.constraintCountShop);
                _shopCollabType = (ShopCollabType)storageInfo.Param[1].Unbox<int>();
                OnClickSubTab(uiShop.nowSub);
                break;
    
            case ShopSelectTab.custom:
                uiShop.contentGridLayout.SetPadding(uiShop.offsetShop);
                uiShop.contentGridLayout.UpdateValue(uiShop.cellSizeShop, uiShop.spacingShop, uiShop.constraintCountShop);
                _shopCustomType = (ShopCustomType)storageInfo.Param[1].Unbox<int>();
                OnClickSubTab(uiShop.nowSub);
                break;
        }
        UpdateTab();
    
        // do not run original code
        return false;
    }
    
    private static void OnClickSubTab(ShopTopUI.ShopSubType shopSubType)
    {
        uiShop.shopBlock.enabled = true;
        _listShopItemNow.Clear();
        uiShop.nowSub = shopSubType;
        uiShop.tabSub1.UpdateState(uiShop.nowSub != ShopTopUI.ShopSubType.sub_1);
        uiShop.tabSub2.UpdateState(uiShop.nowSub != ShopTopUI.ShopSubType.sub_2);
        uiShop.tabSub3.UpdateState(uiShop.nowSub != ShopTopUI.ShopSubType.sub_3);
    
        switch (_shopSelectTab)
        {
            case ShopSelectTab.item_shop:
                SetItemShop(_shopMainType);
                uiShop.SetDialog();
                uiShop.SetCoinData();
                break;
    
            case ShopSelectTab.collab:
                SetCollabShop(_shopCollabType);
                uiShop.SetDialog();
                uiShop.SetCoinData();
                break;
    
            case ShopSelectTab.custom:
                SetCustomShop(_shopCustomType);
                uiShop.SetDialog();
                uiShop.SetCoinData();
                break;
        }
    
        LeanTween.delayedCall(uiShop.gameObject, 0.2f, new Action(() =>
        {
            uiShop.shopBlock.enabled = false;
        }));
    }
    
    private static void SetItemShop(ShopMainType type)
    {
        _shopMainType = type;
        _listShopItemNow = GetShopListByMainAndSubType((int)_shopMainType, (int)uiShop.nowSub);
        _listShopItemNow.Sort((x, y) => x.n_STAGE.CompareTo(y.n_STAGE));
        uiShop.scrollRect.OrangeInit(uiShop.shopItemUnit, 10, _listShopItemNow.Count, 0);
        CreateSubTabButtons();
    }

    private static void SetCollabShop(ShopCollabType type)
    {
        _shopCollabType = type;
        _listShopItemNow = GetShopListByMainAndSubType((int)_shopCollabType + COLLAB_TYPE, (int)uiShop.nowSub);
        _listShopItemNow.Sort((x, y) => x.n_STAGE.CompareTo(y.n_STAGE));
        uiShop.scrollRect.OrangeInit(uiShop.shopItemUnit, 10, _listShopItemNow.Count, 0);
        CreateSubTabButtons();
    }

    private static void SetCustomShop(ShopCustomType type)
    {
        _shopCustomType = type;
        _listShopItemNow = GetShopListByMainAndSubType((int)_shopCustomType + CUSTOM_TYPE, (int)uiShop.nowSub);
        _listShopItemNow.Sort((x, y) => x.n_STAGE.CompareTo(y.n_STAGE));
        uiShop.scrollRect.OrangeInit(uiShop.shopItemUnit, 10, _listShopItemNow.Count, 0);
        CreateSubTabButtons();
    }

    private static void CreateSubTabButtons()
    {
        uiShop.tabSub1.AddBtnCB(new Action(() =>
        {
            OnClickSubTab(ShopTopUI.ShopSubType.sub_1);
        }));
        uiShop.tabSub2.AddBtnCB(new Action(() =>
        {
            OnClickSubTab(ShopTopUI.ShopSubType.sub_2);
        }));
        uiShop.tabSub3.AddBtnCB(new Action(() =>
        {
            OnClickSubTab(ShopTopUI.ShopSubType.sub_3);
        }));
    }

    private static void UpdateTab()
    {
        switch (_shopSelectTab)
        {
            case ShopSelectTab.item_shop:
                UpdateSubTab(GetShopListByMainType((int)_shopMainType));
                break;
    
            case ShopSelectTab.collab:
                UpdateSubTab(GetShopListByMainType((int)_shopCollabType + COLLAB_TYPE));
                break;
    
            case ShopSelectTab.custom:
                UpdateSubTab(GetShopListByMainType((int)_shopCustomType + CUSTOM_TYPE));
                break;
        }
    }

    private static void UpdateSubTab(List<SHOP_TABLE> shopListByMainType)
    {
        uiShop.tabSub1.gameObject.SetActive(false);
        uiShop.tabSub2.gameObject.SetActive(false);
        uiShop.tabSub3.gameObject.SetActive(false);

        bool[] array = new bool[3];
        if (SubTypeExist(ref shopListByMainType, 1))
        {
            uiShop.tabSub1.gameObject.SetActive(true);
            var shop_TABLE = shopListByMainType.First((SHOP_TABLE x) => x.n_SUB_TYPE == 1);
            uiShop.tabSub1.SetTextStr(OrangeTextDataManager.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(shop_TABLE.w_SHEET_NAME));
            array[0] = true;
        }
        if (SubTypeExist(ref shopListByMainType, 2))
        {
            uiShop.tabSub2.gameObject.SetActive(true);
            var shop_TABLE = shopListByMainType.FirstOrDefault((SHOP_TABLE x) => x.n_SUB_TYPE == 2);
            uiShop.tabSub2.SetTextStr(OrangeTextDataManager.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(shop_TABLE.w_SHEET_NAME));
            array[1] = true;
        }
        if (SubTypeExist(ref shopListByMainType, 3))
        {
            uiShop.tabSub3.gameObject.SetActive(true);
            var shop_TABLE = shopListByMainType.FirstOrDefault((SHOP_TABLE x) => x.n_SUB_TYPE == 3);
            uiShop.tabSub3.SetTextStr(OrangeTextDataManager.Instance.LOCALIZATION_TABLE_DICT.GetL10nValue(shop_TABLE.w_SHEET_NAME));
            array[2] = true;
        }
        
        if (!array[uiShop.nowSub - ShopTopUI.ShopSubType.sub_1])
        {
            uiShop.nowSub = ShopTopUI.ShopSubType.sub_1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i])
                {
                    uiShop.nowSub = i + ShopTopUI.ShopSubType.sub_1;
                    break;
                }
            }
            OnClickSubTab(uiShop.nowSub);
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ShopItemUnit), nameof(ShopItemUnit.ScrollCellIndex))]
    private static void ScrollCellIndex()
    {
        uiShop.ListShopItemNow = ConvertSystemList(_listShopItemNow);
    }
}