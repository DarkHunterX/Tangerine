using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tangerine.Utils
{
    /// <summary>
    /// Provides helper methods for loading UI menus and working with UI elements
    /// </summary>
    public class UIHelpers
    {
        /// <summary>
        /// Get list of child GameObjects while ignoring their children
        /// </summary>
        /// <param name="parentObj">the parent GameObject to search for children</param>
        public static List<GameObject> GetTopLevelChildren(GameObject parentObj)
        {
            var children = new List<GameObject>();
            foreach (var child in parentObj.GetComponentsInChildren<Transform>(true))
            {
                if (child.parent.name == parentObj.name)
                    children.Add(child.gameObject);
            }
            return children;
        }

        /// <summary>
        /// Copy a GameObject and place it into the current scene hierarchy
        /// </summary>
        /// <param name="source">the GameObject to copy</param>
        /// <param name="childPosition">hierarchy position to place the GameObject copy relative to the original</param>
        public static GameObject CopyGameObject(GameObject source, int childPosition = -1)
        {
            var NewObj = GameObject.Instantiate(source);
            NewObj.transform.SetParent(source.transform.parent);
            if (childPosition != -1) NewObj.transform.SetSiblingIndex(childPosition);
            NewObj.transform.position = source.transform.position;
            NewObj.transform.rotation = source.transform.rotation;
            NewObj.transform.localScale = source.transform.localScale;
            return NewObj;
        }

        /// <summary>
        /// Alternative implementation of <see cref="UIManager.LoadUI{T}(string, UIManager.LoadUIComplete{T})"/>
        /// </summary>
        /// <param name="p_name">name of UI assetbundle to load</param>
        /// <param name="p_cb1">code to run before activating the UI. used to setup the fields of <see cref="OrangeUIBase"/></param>
        /// <param name="p_cb2">code to run after the UI has been loaded</param>
        public static void LoadUI<T>(string p_name, Action<OrangeUIBase> p_cb1 = null, UIManager.LoadUIComplete<T> p_cb2 = null) where T : OrangeUIBase
        {
            var __instance = UIManager.Instance;
            if (__instance.tempActiveUiName == p_name)
                return;

            if (__instance.isUiLoading)
            {
                // Plugin.Log.LogInfo($"Load UI Queue: {p_name}, count: {__instance.queueUiWaitLoading.Count}, Last Loading: {__instance.tempActiveUiName}");
                __instance.queueUiWaitLoading.Enqueue(new Action(() =>
                {
                    LoadUI<T>(p_name, p_cb1, p_cb2);
                }));
                return;
            }

            __instance.isUiLoading = true;
            __instance.UpdateBlockState(0.15f);
            __instance.bLockTurtorialLoad = true;
            TurtorialUI.CheckTurtorialTriggerName(p_name, null);
            __instance.tempActiveUiName = p_name;

            //Plugin.Log.LogInfo($"Loading UI: {p_name}");
            AssetsBundleManager.Instance.GetAssetAndAsyncLoad<GameObject>($"{__instance.uiPath}{p_name}", p_name, new Action<GameObject>((GameObject asset) =>
            {
                //Plugin.Log.LogInfo($"Load UI End: {p_name}");
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(asset, __instance.transform, false);
                gameObject.name = p_name;
                gameObject.transform.SetParent(__instance.UiParent, false);

                var component = gameObject.GetComponent<T>();
                if (component == null)
                    component = gameObject.AddComponent<T>();

                if (p_cb1 != null)
                    p_cb1(component);

                __instance.tempActiveUiName = string.Empty;
                __instance.ActiveUI<T>(component, p_cb2, false);
            }), AssetKeepMode.KEEP_IN_SCENE);
        }
    }
}
