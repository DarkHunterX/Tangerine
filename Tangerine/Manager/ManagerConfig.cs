using BepInEx.Configuration;
using UnityEngine;

namespace Tangerine.Manager
{
    internal static class ManagerConfig
    {
        internal enum ReloadMode
        {
            None,
            BackToHome,
            BackToTitle,
            Both,
        }

        internal const string ModReloadFile = "ModReload.txt";

        // general
        internal static ConfigEntry<KeyCode> ReloadKey { get; set; }
        internal static ConfigEntry<KeyCode> BackToTitleKey { get; set; }
        internal static ConfigEntry<KeyCode> BackToHometopKey { get; set; }
        internal static ConfigEntry<ReloadMode> BackToSceneReloadMode { get; set; }

        // debug logging
        internal static ConfigEntry<bool> DebugLogJsonText { get; set; }
        internal static ConfigEntry<bool> DebugLogJsonData { get; set; }
        internal static ConfigEntry<bool> DebugLogJsonParam { get; set; }
        internal static ConfigEntry<bool> DebugLogAssetbundle { get; set; }
        internal static ConfigEntry<bool> DebugLogAssetRemap { get; set; }
        internal static ConfigEntry<bool> DebugLogFileRemap { get; set; }
        internal static ConfigEntry<bool> DebugLogCriwareAudio { get; set; }
        internal static ConfigEntry<bool> DebugLogPluginDll { get; set; }
        internal static ConfigEntry<bool> DebugLogChargeFx { get; set; }
        internal static ConfigEntry<bool> DebugLogDebutEasterEgg { get; set; }
        internal static ConfigEntry<bool> DebugLogTextureCache { get; set; }

        public static void Initialize()
        {
            // general
            ReloadKey = Plugin.Config.Bind("General", "Reload Key", KeyCode.F4,
                new ConfigDescription($"Press this key to reload all mods that have the \"{ModReloadFile}\" file in their folder"));

            BackToTitleKey = Plugin.Config.Bind("General", "Back to Title Key", KeyCode.None,
                new ConfigDescription($"Press this key to go back to the title screen"));

            BackToHometopKey = Plugin.Config.Bind("General", "Back to Hometop Key", KeyCode.None,
                new ConfigDescription($"Press this key to go back to the home screen"));

            BackToSceneReloadMode = Plugin.Config.Bind("General", "Reload when going back to Title/Hometop", ReloadMode.BackToTitle,
                new ConfigDescription($"Specify whether to reload all asset bundles when going back to title, home, or both"));

            // debug logging
            DebugLogJsonText = Plugin.Config.Bind("Debug Logging", "JSON Text", false,
                new ConfigDescription($"Print 'Json Text' related debug messages to the console window"));

            DebugLogJsonData = Plugin.Config.Bind("Debug Logging", "JSON Data", false,
                new ConfigDescription($"Print 'Json Data' related debug messages to the console window"));

            DebugLogJsonParam = Plugin.Config.Bind("Debug Logging", "JSON Parameters", false,
                new ConfigDescription($"Print 'Json Parameter' related debug messages to the console window"));

            DebugLogAssetbundle = Plugin.Config.Bind("Debug Logging", "Assetbundle", false,
                new ConfigDescription($"Print 'Assetbundle' related debug messages to the console window"));

            DebugLogAssetRemap = Plugin.Config.Bind("Debug Logging", "Asset Remap", false,
                new ConfigDescription($"Print 'Asset Remap' related debug messages to the console window"));

            DebugLogFileRemap = Plugin.Config.Bind("Debug Logging", "File Remap", false,
                new ConfigDescription($"Print 'File Remap' related debug messages to the console window"));

            DebugLogCriwareAudio = Plugin.Config.Bind("Debug Logging", "CRIWARE Audio", false,
                new ConfigDescription($"Print 'Criware Audio' related debug messages to the console window"));

            DebugLogPluginDll = Plugin.Config.Bind("Debug Logging", "Plugin DLL", false,
                new ConfigDescription($"Print 'Plugin DLL' related debug messages to the console window"));

            DebugLogChargeFx = Plugin.Config.Bind("Debug Logging", "Charge FX", false,
                new ConfigDescription($"Print 'Charge FX' related debug messages to the console window"));

            DebugLogDebutEasterEgg = Plugin.Config.Bind("Debug Logging", "Debut Easter Egg", false,
                new ConfigDescription($"Print 'Debut Easter Egg' related debug messages to the console window"));

            DebugLogTextureCache = Plugin.Config.Bind("Debug Logging", "Texture Cache", false,
                new ConfigDescription($"Print 'Texture Cache' related debug messages to the console window"));
        }
    }
}
