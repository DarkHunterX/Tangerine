using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.IO;
using Tangerine.Manager;
using Tangerine.Patchers;
using Tangerine.Patchers.DataProvider;
using Tangerine.Utils;

namespace Tangerine;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[BepInPlugin(GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static string Location;
    internal static new ManualLogSource Log;
    internal static new ConfigFile Config;

    private static Harmony _harmony;

    public const string GUID = "0Tangerine";

    public override void Load()
    {
        // Plugin startup logic
        Plugin.Log = base.Log;
        Plugin.Config = base.Config;

        Log.LogInfo($"Tangerine is loaded!");

        // Get folder
        Location = Path.GetDirectoryName(IL2CPPChainloader.Instance.Plugins[GUID].Location);
        DebugLogging.LoadSettings();

        _harmony = new Harmony(GUID);
        TangerineConst.InitializeHarmony(_harmony);
        TangerineDataManager.InitializeHarmony(_harmony);
        TangerineTextDataManager.InitializeHarmony(_harmony);
        TangerineLoader.InitializeHarmony(_harmony);

        TangerineCharacter.InitializeHarmony(_harmony);
        TangerineDebutEgg.InitializeHarmony(_harmony);
        TangerineChargeFx.InitializeHarmony(_harmony);
        TangerineAudioManager.InitializeHarmony(_harmony);

        // Start loading mods
        ModManager.Initialize(this);
    }

    public override bool Unload()
    {
        _harmony.UnpatchSelf();
        return true;
    }
}
