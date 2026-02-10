using CallbackDefs;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Manager;
using Tangerine.Patchers.LogicUpdate;
using Tangerine.Utils;

namespace Tangerine.Patchers
{
    /// <summary>
    /// Contains methods for adding UI menu controller classes that inherit from <see cref="OrangeUIBase"/>
    /// </summary>
    public class TangerineUI
    {
        internal static readonly Dictionary<string, Callback> preloadUIDict = new();
        private static readonly List<(Type, Type[])> _initialControllerList = new();
        private static bool _orangeConstInitialized = false;

        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(TangerineUI));
        }

        private readonly string _modGuid;
        internal TangerineUI(string modGuid)
        {
            _modGuid = modGuid;
        }

        private static void RegisterController(Type controllerType, Type[] interfaces = null)
        {
            if (!_orangeConstInitialized)
            {
                // Delay registration until OrangeConst is initialized
                _initialControllerList.Add((controllerType, interfaces));
            }
            else if (!ClassInjector.IsTypeRegisteredInIl2Cpp(controllerType))
            {
                LogMessage.LogWarning($"Registering UI menu controller: {controllerType.FullName}", ManagerConfig.DebugLogPluginDll.Value);
                
                interfaces ??= Array.Empty<Type>();
                if (typeof(ITangerineLogicUpdate).IsAssignableFrom(controllerType) && !interfaces.Contains(typeof(ILogicUpdate)))
                {
                    // Add ILogicUpdate to list of interfaces
                    interfaces = interfaces.AddToArray(typeof(ILogicUpdate));
                }

                var options = new RegisterTypeOptions()
                {
                    Interfaces = new Il2CppInterfaceCollection(interfaces),
                };

                ClassInjector.RegisterTypeInIl2Cpp(controllerType, options);
            }
        }

        /// <summary>
        /// Adds a UI menu controller class by injecting it into the game's runtime
        /// </summary>
        /// <param name="uiName">name of UI assetbundle</param>
        /// <param name="controllerType"><see langword="typeof"/> the controller class</param>
        /// <param name="interfaces">Il2Cpp interfaces the class should implement, if any (e.g. <see cref="ILogicUpdate"/>)</param>
        /// <param name="preloadAtHomeTop">if UI assetbundle should be preloaded when loading the HomeTop</param>
        /// <param name="preload_cb">code to run after preload is complete</param>
        public void AddController(string uiName, Type controllerType, Type[] interfaces = null, bool preloadAtHomeTop = false, Callback preload_cb = null)
        {
            if (preloadAtHomeTop)
                preloadUIDict.Add(uiName, preload_cb);
            RegisterController(controllerType, interfaces);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(OrangeConst), nameof(OrangeConst.ConstInit))]
        private static void OrangeConstInitPostfix()
        {
            if (!_orangeConstInitialized)
            {
                _orangeConstInitialized = true;
                foreach (var args in _initialControllerList)
                    RegisterController(args.Item1, args.Item2);
                _initialControllerList.Clear();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(OrangeSceneManager), nameof(OrangeSceneManager.OnStartChangeScene))]
        private static void OnStartChangeScene_prefix(OrangeSceneManager __instance)
        {
            if (__instance.NowScene == "hometop")
            {
                foreach (var ui in preloadUIDict)
                    UIManager.Instance.PreloadUI(ui.Key, ui.Value);
            }
        }
    }
}
