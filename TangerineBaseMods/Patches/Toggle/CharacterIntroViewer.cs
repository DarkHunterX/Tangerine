using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tangerine.Manager.Mod;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TangerineBaseMods;

public class CharacterIntroViewer
{
    private static bool isViewing = false;
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

    internal static void InitializeHarmony(TangerineMod tangerine, Harmony harmony, JsonNode node)
    {
        if (node["CharacterIntroViewer"]["enabled"].Deserialize<bool>())
        {
            harmony.PatchAll(typeof(CharacterIntroViewer));
            Plugin.RemoveObsoleteMod_IntroViewer();
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CharacterInfoBasic), nameof(CharacterInfoBasic.Setup), new[] { typeof(CharacterInfo) })]
    private static void fw_CharaInfo_Setup(CharacterInfoBasic __instance)
    {
        var mainTrans = __instance.btnDeploy.GetComponentInParent<Transform>().GetParent();
        Transform[] componentsInChildren = mainTrans.transform.GetComponentsInChildren<Transform>(true);
        foreach (var component in componentsInChildren)
        {
            if (component.name == "Btn2DSwitch")
            {
                var temp = CopyTransform(component.gameObject);
                temp.name = "btnShowCase";
                temp.transform.position = new Vector3(temp.transform.position.x - 10.0f, temp.transform.position.y, temp.transform.position.z);

                var text = temp.transform.GetChild(0).GetComponent<OrangeText>();
                text.IsLocalizationText = true;
                text.LocalizationKey = "FUNCTION_VIEW_DEBUT";
                text.text = LocalizationManager.Instance.GetStr("FUNCTION_VIEW_DEBUT");

                var btn = temp.transform.GetComponent<Button>();
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(new Action(OnClickShowCase));
            }
        }
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
        if (Input.GetKeyDown(KeyCode.T))
        {
            bgIndex++;
            if (bgIndex == bgGameObj.Count) bgIndex = 0;
            UpdateBackground();
            AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            bgIndex--;
            if (bgIndex < 0) bgIndex = bgGameObj.Count - 1;
            UpdateBackground();
            AudioManager.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR01);
        }

        if (Input.GetKeyDown(KeyCode.R))
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
            var uiBG = OrangeBattleUtility.FindChildRecursive(componentsInChildren, "Bg_CharacterMenu(Clone)", true);
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


    private static GameObject CopyTransform(GameObject source, int childPosition = -1)
    {
        var NewObj = GameObject.Instantiate(source);
        NewObj.transform.SetParent(source.transform.parent);
        if (childPosition != -1) NewObj.transform.SetSiblingIndex(childPosition);
        NewObj.transform.position = source.transform.position;
        NewObj.transform.rotation = source.transform.rotation;
        NewObj.transform.localScale = source.transform.localScale;
        return NewObj;
    }
}