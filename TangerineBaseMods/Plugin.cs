using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ConfigManager.UI.InteractiveValues;
using HarmonyLib;
using System;
using System.IO;
using Tangerine.Manager.Mod;
using TangerineBaseMods.Config;
using TangerineBaseMods.Config.InteractiveValues;
using TangerineBaseMods.Config.TypeConverters;
using TangerineBaseMods.Config.Types;
using TangerineBaseMods.Patches;
using TangerineBaseMods.Patches.Toggle;

namespace TangerineBaseMods;

// Add dependency to Tangerine. This is required for the mod to show up in the mods menu.
[BepInDependency(Tangerine.Plugin.GUID, BepInDependency.DependencyFlags.HardDependency)]
// Do not modify this line. You can change AssemblyName, Product, and Version directly in the .csproj
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : TangerinePlugin
{
    private static TangerineMod _tangerine = null;
    private static Harmony _harmony;
    internal static new ManualLogSource Log;
    internal static new ConfigFile Config;

    internal static readonly string ModsDir = Path.Combine(Paths.BepInExRootPath, "mods");
    internal static readonly string PluginModDir = Path.Combine(ModsDir, MyPluginInfo.PLUGIN_GUID);

    internal static readonly string _oldPluginDir = Path.Combine(Paths.BepInExRootPath, "plugins");

    public override void Load(TangerineMod tangerine)
    {
        _tangerine = tangerine;

        // Plugin startup logic
        Plugin.Log = base.Log;
        Plugin.Config = base.Config;
        Log.LogInfo($"Tangerine plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Configuration.Initialize();
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        // plugins
        CharacterPassives.InitializeHarmony(_tangerine, _harmony);
        CardLoadout.InitializeHarmony(_harmony);
        DNA.InitializeHarmony(_tangerine, _harmony);
        EventSkip.InitializeHarmony(_harmony);
        StorySkip.InitializeHarmony(_harmony);
        SaveValidation.InitializeHarmony(_harmony);

        // Aoki plugins
        BorderExAddon.InitializeHarmony(_tangerine, _harmony);
        CharacterIntroViewer.InitializeHarmony(_harmony);
        SkinVoiceAddon.InitializeHarmony(_harmony);

        // Django plugins
        DualGunFix.InitializeHarmony(_tangerine, _harmony);

        // hard patches
        CheatEngineFix.InitializeHarmony(_harmony);
        ChipIdRangeFix.InitializeHarmony(_harmony);
        DiscordInvite.InitializeHarmony(_harmony);
        ExpandedShopTabs.InitializeHarmony(_harmony);
        GoUIMenuFixes.InitializeHarmony(_harmony);
        IntroSkip.InitializeHarmony(_harmony);
        LoadingImageTable.InitializeHarmony(_harmony);
        ResetWeapon.InitializeHarmony(_harmony);
        ResetCharacter.InitializeHarmony(_tangerine, _harmony);

        // add support for custom config types
        TomlTypeConverter.AddConverter(typeof(LoadingGacha), new LoadingGachaTypeConverter());
        InteractiveValue.RegisterIValueType<InteractiveLoadingGacha>();
    }

    public override bool Unload()
    {
        Log.LogInfo($"Tangerine plugin {MyPluginInfo.PLUGIN_GUID} was unloaded!");

        _harmony.UnpatchSelf();
        return true;
    }

    internal static void RemoveObsoleteMod_RestoredFunctions()
    {
        // remove old version of DNA restoration + story skip dive mods
        RemoveDir(Path.Combine(_oldPluginDir, "RestoredFunctions"), "Removed obsolete plugin \"RestoredFunctions\"");
        RemoveDir(Path.Combine(ModsDir, "RestoredFunctions"), "Removed obsolete mod folder \"RestoredFunctions\"");
    }

    internal static void RemoveObsoleteMod_SSBorder()
    {
        // remove old version of SS rank border addon
        RemoveDir(Path.Combine(_oldPluginDir, "SS_Border"), "Removed obsolete plugin \"SS Rank Border Add-on\"");
        RemoveDir(Path.Combine(ModsDir, "SS_Border"), "Removed obsolete mod folder \"SS Rank Border Add-on\"");
    }

    internal static void RemoveObsoleteMod_IntroViewer()
    {
        // remove old version of character intro viewer addon
        RemoveDir(Path.Combine(ModsDir, "CharacterIntroViewAddon"), "Removed obsolete mod folder \"Character Intro Viewer Add-on\"");
    }

    internal static void RemoveObsoleteMod_SkinVoiceAddon()
    {
        // remove old version of skin voice addon
        RemoveDir(Path.Combine(ModsDir, "SkinVoiceAddon"), "Removed obsolete mod folder \"Skin Voice Addon\"");
    }

    internal static void RemoveObsoleteMod_DualGunFix()
    {
        // remove old version of dual gun fix
        RemoveDir(Path.Combine(ModsDir, "DualGunFix"), "Removed obsolete mod folder \"Dual Gun Fix\"");
    }

    internal static void RemoveObsoleteMod_IntroSkip()
    {
        // remove old version of game intro skip
        RemoveDir(Path.Combine(ModsDir, "IntroSkip"), "Removed obsolete mod folder \"Intro Skip\"");
    }

    internal static void RemoveObsoleteMod_FixSavingCrash()
    {
        // remove old version of cheat engine fix
        RemoveDir(Path.Combine(_oldPluginDir, "FixSavingCrash"), "Removed obsolete plugin \"Fix Saving Crash\"");
    }

    internal static void RemoveDir(string folderPath, string msg)
    {
        try
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
                Log.LogError(msg);
            }
        }
        catch (Exception ex)
        {
            var modFolderName = new DirectoryInfo(folderPath).Name;
            Log.LogError($"Failed to remove obsolete mod \"{modFolderName}\" - {ex}");
        }
    }
}
