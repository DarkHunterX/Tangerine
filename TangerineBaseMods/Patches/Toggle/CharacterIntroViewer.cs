using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tangerine.Utils;
using TangerineBaseMods.Config;

namespace TangerineBaseMods;

public class CharacterIntroViewer
{
    internal static void InitializeHarmony(Harmony harmony)
    {
        harmony.PatchAll(typeof(CharacterIntroViewer));
        Plugin.RemoveObsoleteMod_IntroViewer();
        SetConfigDelegates();
    }

    private static void SetConfigDelegates()
    {
        Configuration.CharacterIntroViewer.SettingChanged += (sender, args) => UpdateIntroViewerBtnVisibility();
    }

    private static bool isViewing = false;
    private static Button viewBtn;
    private static CharacterInfoUI ui;
    private static Image backgroundImg;
    private static Vector3 modelAnchorPosition;

    private static int clickCount;
    private static float clickTime;
    private static Vector3 mousePos;
    private static Vector3 mousePosStart;

    private static int bgIndex = 0;
    private static readonly List<string> bgGameObj = new()
    {
        "Bg_CharacterMenu",
        "Bg_WeaponBg",
        "BG_ChipBg",
        "BG_Bag",
        "BG_Lab",
        "BG_Shop_BG",
        "Bg_PowerGuide",
        "Bg_Setting",
        "Bg_Bosschallenge",
        "Bg_Login_01",
        "Bg_GreenScreen",
    };

    private static void UpdateIntroViewerBtnVisibility()
    {
        if (Configuration.CharacterIntroViewer.Value)
        {
            if (viewBtn != null)
                viewBtn.gameObject.SetActive(true);
            else
                CreateViewButton();
        }
        else
        {
            if (viewBtn != null)
                viewBtn.gameObject.SetActive(false);

            if (isViewing)
                OnCloseShowCase();
        }
    }

    private static void CreateViewButton()
    {
        var basicUI = UIManager.Instance.GetUI<CharacterInfoBasic>("UI_CharacterInfo_Basic");
        if (basicUI == null)
            return;

        var mainTrans = basicUI.btnDeploy.GetComponentInParent<Transform>().GetParent();
        Transform[] componentsInChildren = mainTrans.transform.GetComponentsInChildren<Transform>(true);
        foreach (var component in componentsInChildren)
        {
            if (component.name == "Btn2DSwitch")
            {
                var temp = UIHelpers.CopyGameObject(component.gameObject, component.transform.GetSiblingIndex() - 1);
                temp.name = "btnShowCase";
                temp.transform.position = new Vector3(temp.transform.position.x - 10.0f, temp.transform.position.y, temp.transform.position.z);

                var text = temp.transform.GetChild(0).GetComponent<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = "FUNCTION_VIEW_DEBUT";
                text.text = LocalizationManager.Instance.GetStr("FUNCTION_VIEW_DEBUT");

                viewBtn = temp.transform.GetComponent<Button>();
                viewBtn.onClick = new Button.ButtonClickedEvent();
                viewBtn.onClick.AddListener(new Action(OnClickShowCase));
                break;
            }
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CharacterInfoBasic), nameof(CharacterInfoBasic.Setup), new[] { typeof(CharacterInfo) })]
    private static void fw_CharaInfo_Setup(CharacterInfoBasic __instance)
    {
        if (Configuration.CharacterIntroViewer.Value)
            CreateViewButton();
    }

    [HarmonyPrefix, HarmonyPatch(typeof(OrangeUIBase), nameof(OrangeUIBase.DoEscapeEvent))]
    private static bool OnEscapePressed()
    {
        if (isViewing)
        {
            OnCloseShowCase();

            // do not run original code
            return false;
        }
        // run original code
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(UIManager), nameof(UIManager.Update))]
    private static void Update()
    {
        if (isViewing)
        {
            OnDoubleClick();
            OnHotKeyPressed();
        }
    }

    private static void OnDoubleClick()
    {
        mousePos = Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            clickCount++;
            if (clickCount == 1)
            {
                clickTime = Time.time;
                mousePosStart = mousePos;
            }
        }
        if (clickCount > 1 && (Time.time - clickTime) < 0.4f && (mousePos - mousePosStart).magnitude < 30f)
        {
            OnCloseShowCase();
            clickCount = 0;
            clickTime = 0;
        }
        else if (clickCount > 2 || Time.time - clickTime > 1) { clickCount = 0; }
    }

    private static void OnHotKeyPressed()
    {
        if (Input.GetKeyDown(Configuration.NextBgKey.Value))
        {
            bgIndex++;
            if (bgIndex == bgGameObj.Count) bgIndex = 0;
            UpdateBackground();
            AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
        }
        else if (Input.GetKeyDown(Configuration.PreviousBgKey.Value))
        {
            bgIndex--;
            if (bgIndex < 0) bgIndex = bgGameObj.Count - 1;
            UpdateBackground();
            AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
        }

        if (Input.GetKeyDown(Configuration.RestartAnimKey.Value))
        {
            ui.textureObj.modelName = "p_Dummy";
            ui.textureObj.AssignNewRender(ui.characterTable, null, ui.m_skinTable, new Vector3(0f, -1.0f, 5.5f), ui.tModelImg, 0);
        }
    }

    private static void UpdateBackground()
    {
        if (bgGameObj[bgIndex] == "Bg_GreenScreen")
        {
            AssetsBundleManager.Instance.GetAssetAndAsyncLoad<Sprite>("ui/background/" + bgGameObj[bgIndex].ToLower(), bgGameObj[bgIndex], new System.Action<Sprite>((Sprite spr) =>
            {
                backgroundImg.sprite = spr;
            }), AssetKeepMode.KEEP_IN_SCENE);
        }
        else LoadBackground("ui/background/" + bgGameObj[bgIndex].ToLower(), bgGameObj[bgIndex]);
    }

    private static void LoadBackground(string uiBundle, string objName)
    {
        AssetsBundleManager.Instance.GetAssetAndAsyncLoad<GameObject>(uiBundle, objName, new Action<GameObject>((GameObject obj) =>
        {
            backgroundImg.sprite = obj.GetComponent<Image>().sprite;
        }), AssetKeepMode.KEEP_IN_SCENE);
    }

    private static void OnCloseShowCase()
    {
        // restore background + model position
        LoadBackground("ui/background/" + bgGameObj[0].ToLower(), bgGameObj[0]);
        ui.tModelImg.gameObject.GetComponent<RectTransform>().anchoredPosition3D = modelAnchorPosition;

        // close viewer
        EnableMenu();
        isViewing = false;
    }

    private static void OnClickShowCase()
    {
        ui = UIManager.Instance.GetUI<CharacterInfoUI>("UI_CharacterInfo_Main");
        if (ui.textureObj != null)
        {
            ui.textureObj.modelName = "p_Dummy";
            ui.textureObj.AssignNewRender(ui.characterTable, null, ui.m_skinTable, new Vector3(0f, -1.0f, 5.5f), ui.tModelImg, 0);
            var rect = ui.tModelImg.gameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                modelAnchorPosition = rect.anchoredPosition3D;
                rect.anchoredPosition3D = new Vector3(0f, 0f, 0f);
            }
            else
                Plugin.Log.LogError("Can't find rect");

            //ui.textureObj.renderCamera.fieldOfView = 40.0f;

            // get background component
            Transform[] componentsInChildren = ui.gameObject.transform.parent.GetComponentsInChildren<Transform>(true);
            var uiBG = OrangeBattleUtility.FindAllChildRecursive(componentsInChildren, "Bg_CharacterMenu(Clone)", true).Last();
            backgroundImg = uiBG.GetComponent<Image>();

            UpdateBackground();
            DisableMenu();
            isViewing = true;
        }
    }

    private static void DisableMenu()
    {
        var basicUI = UIManager.Instance.GetUI<CharacterInfoBasic>("UI_CharacterInfo_Basic");
        basicUI.gameObject.SetActive(false);

        var tfMainUI = ui.gameObject.transform;
        for (int i = 0; i < tfMainUI.GetChildCount(); i++)
            tfMainUI.GetChild(i).gameObject.SetActive(tfMainUI.GetChild(i).gameObject.name.Equals("ModelImage"));

        AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
    }

    private static void EnableMenu()
    {
        ui.RefreshMenu();

        var tfMainUI = ui.gameObject.transform;
        for (int i = 0; i < tfMainUI.GetChildCount(); i++)
        {
            if (!tfMainUI.GetChild(i).gameObject.name.Equals("BtnBack") && !tfMainUI.GetChild(i).gameObject.name.Equals("UI_lockFX(Clone)"))
                tfMainUI.GetChild(i).gameObject.SetActive(true);
        }
        AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
    }
}