using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using TangerineBaseMods.Config.Types;
using TangerineBaseMods.Patches.Toggle;

namespace TangerineBaseMods.Config
{
    // for help, see: https://github.com/sinai-dev/BepInExConfigManager/tree/main
    internal static class Configuration
    {
        // debut animation viewer
        internal static ConfigEntry<bool> CharacterIntroViewer { get; set; }
        internal static ConfigEntry<KeyCode> RestartAnimKey { get; set; }
        internal static ConfigEntry<KeyCode> NextBgKey { get; set; }
        internal static ConfigEntry<KeyCode> PreviousBgKey { get; set; }

        // skip dive
        internal static ConfigEntry<bool> StorySkip { get; set; }
        internal static ConfigEntry<int> StorySkipCount { get; set; }
        internal static ConfigEntry<bool> EventSkip { get; set; }
        internal static ConfigEntry<int> EventSkipCount { get; set; }

        // character & weapon reset
        internal static ConfigEntry<int> ResetCost { get; set; }

        // character dna
        internal static ConfigEntry<bool> SaveValidation { get; set; }
        internal static ConfigEntry<bool> EraseDisabledDNA { get; set; }

        // misc. mods
        internal static ConfigEntry<bool> IntroSkip { get; set; }
        internal static ConfigEntry<bool> CardLoadout { get; set; }
        internal static ConfigEntry<bool> ExpandedDNA { get; set; }
        internal static ConfigEntry<bool> ExpandedCharacterPassives { get; set; }
        internal static ConfigEntry<bool> BorderExAddon { get; set; }
        internal static ConfigEntry<bool> SkinVoiceAddon { get; set; }
        internal static ConfigEntry<bool> DualGunFix { get; set; }

        // loading screen image
        internal static ConfigEntry<string> LockImage { get; set; }
        internal static ConfigEntry<LoadingGacha> LoadingImgGacha { get; set; }


        internal static void Initialize()
        {
            // debut animation viewer
            CharacterIntroViewer = Plugin.Config.Bind("Debut Animation Viewer", "Enabled", true,
                new ConfigDescription($"Adds a button to view the character debut animations unobstructed by the UI"));

            RestartAnimKey = Plugin.Config.Bind("Debut Animation Viewer", "Restart Animation Key", KeyCode.R,
                new ConfigDescription($"Press this key to restart the character debut animation"));

            NextBgKey = Plugin.Config.Bind("Debut Animation Viewer", "Next BG Key", KeyCode.T,
                new ConfigDescription($"Press this key to switch to the next background image"));

            PreviousBgKey = Plugin.Config.Bind("Debut Animation Viewer", "Previous BG Key", KeyCode.Y,
                new ConfigDescription($"Press this key to switch to the previous background image"));

            // skip dive
            StorySkip = Plugin.Config.Bind("Story Skip DiVE", "Enabled", true,
                new ConfigDescription($"Adds a button to skip replaying (Normal mode) story stages and receive the rewards"));

            StorySkipCount = Plugin.Config.Bind("Story Skip DiVE", "Amount", 10,
                new ConfigDescription($"The number of times to skip the stage", new AcceptableValueRange<int>(1, 10000)));

            EventSkip = Plugin.Config.Bind("Event Skip DiVE", "Enabled", true,
                new ConfigDescription($"Adds a button to skip replaying (Normal mode) event stages and receive the rewards"));

            EventSkipCount = Plugin.Config.Bind("Event Skip DiVE", "Amount", 10,
                new ConfigDescription($"The number of times to skip the stage", new AcceptableValueRange<int>(1, 10000)));

            // character & weapon reset
            ResetCost = Plugin.Config.Bind("Character & Weapon Reset", "Cost", 30,
                new ConfigDescription($"The amount of Elemental Metals each reset option costs", new AcceptableValueRange<int>(0, 50)));

            // save validation
            SaveValidation = Plugin.Config.Bind("Save Validation", "Enabled", true,
                new ConfigDescription($"Removes all invalid data from your save file when loading. This keeps your save file from corrupting when uninstalling mods [REQUIRES GAME RESTART]"));

            EraseDisabledDNA = Plugin.Config.Bind("Save Validation", "Erase Disabled DNA", false,
                new ConfigDescription($"When saving the game, delete all your Recombined and Inherited DNA if you have the \"Expanded DNA\" feature disabled, otherwise it will just be hidden from the game"));

            // misc. mods
            IntroSkip = Plugin.Config.Bind("Misc. Mods", "Intro Skip", false,
                new ConfigDescription($"Skip the CAPCOM screen and opening video"));

            CardLoadout = Plugin.Config.Bind("Misc. Mods", "Card Loadout Naming", true,
                new ConfigDescription($"Restored feature from the online version, allows naming your card loadouts (up to 10 characters) [REQUIRES GAME RESTART]"));

            ExpandedDNA = Plugin.Config.Bind("Misc. Mods", "Exanded Character DNA", true,
                new ConfigDescription($"Restored feature from the online version, expands character DNA passives and allows linking them to other characters [REQUIRES GAME RESTART]"));

            ExpandedCharacterPassives = Plugin.Config.Bind("Misc. Mods", "Expanded Character Passives", true,
                new ConfigDescription($"Updates the character passive menu UI to allow use of a 6th passive [REQUIRES GAME RESTART]"));

            BorderExAddon = Plugin.Config.Bind("Misc. Mods", "Expanded Rank Borders", true,
                new ConfigDescription($"Adds colored borders for D, C, and SS rank characters and cards [REQUIRES GAME RESTART]"));

            SkinVoiceAddon = Plugin.Config.Bind("Misc. Mods", "Skin Voice Addon", true,
                new ConfigDescription($"Allows custom character skins to have their own voice instead of using the voice from the original character [REQUIRES GAME RESTART]"));

            DualGunFix = Plugin.Config.Bind("Misc. Mods", "Dual Gun Fix", true,
                new ConfigDescription($"Fixes several issues with the Dual Gun weapon type [REQUIRES GAME RESTART]"));
        }

        internal static void AddLoadingImgConfigEntry(List<string> lockImgList)
        {
            // loading screen image
            LockImage = Plugin.Config.Bind("Loading Screen Image", "Lock Image", string.Empty,
                new ConfigDescription($"Choose an image to always display", new AcceptableValueList<string>(lockImgList.ToArray())));

            LoadingImgGacha = Plugin.Config.Bind("Loading Screen Image", "Image Rates", new LoadingGacha(),
                new ConfigDescription($"The probabilities of each image type being displayed"));
        }
    }
}