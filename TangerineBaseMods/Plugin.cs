using System;
using System.IO;
using System.Text.Json.Nodes;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Tangerine.Manager.Mod;
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

    internal static readonly string ModsDir = Path.Combine(Paths.BepInExRootPath, "mods");
    internal static readonly string PluginModDir = Path.Combine(ModsDir, MyPluginInfo.PLUGIN_GUID);
    private const string JsonFile = "Settings.json";

    internal static readonly string _oldPluginDir = Path.Combine(Paths.BepInExRootPath, "plugins");

    public override void Load(TangerineMod tangerine)
    {
        _tangerine = tangerine;

        // Plugin startup logic
        Plugin.Log = base.Log;
        Log.LogInfo($"Tangerine plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        try
        {
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            var node = JsonNode.Parse(File.ReadAllText(Path.Combine(PluginModDir, JsonFile)));

            // plugins
            CharacterPassives.InitializeHarmony(_tangerine, _harmony, node);
            CardLoadout.InitializeHarmony(_tangerine, _harmony, node);
            DNA.InitializeHarmony(_tangerine, _harmony, node);
            EventSkip.InitializeHarmony(_tangerine, _harmony, node);
            StorySkip.InitializeHarmony(_tangerine, _harmony, node);
            SaveValidation.InitializeHarmony(_harmony, node);

            // Aoki plugins
            BorderExAddon.InitializeHarmony(_tangerine, _harmony, node);
            CharacterIntroViewer.InitializeHarmony(_tangerine, _harmony, node);

            // Django plugins
            DualGunFix.InitializeHarmony(_tangerine, _harmony, node);

            // hard patches
            HometopAnimationFix.InitializeHarmony(_harmony);
            CheatEngineFix.InitializeHarmony(_harmony);
            ChipIdRangeFix.InitializeHarmony(_harmony);
            ExpandedShopTabs.InitializeHarmony(_harmony);
            DiscordInvite.InitializeHarmony(_harmony);
            IntroSkip.InitializeHarmony(_harmony, node);
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to read {JsonFile} for mod \"{PluginModDir}\": {e}");
        }
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
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
            Log.LogError(msg);
        }
    }
}
