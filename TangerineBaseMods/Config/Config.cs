using BepInEx.Configuration;
using UnityEngine;

namespace TangerineBaseMods.Config
{
    internal static class Configuration
    {
        // debut animation viewer
        internal static ConfigEntry<KeyCode> RestartAnimKey { get; set; }
        internal static ConfigEntry<KeyCode> NextBgKey { get; set; }
        internal static ConfigEntry<KeyCode> PreviousBgKey { get; set; }

        // mods
        internal static ConfigEntry<bool> IntroSkip { get; set; }
        internal static ConfigEntry<bool> StorySkip { get; set; }
        internal static ConfigEntry<int> StorySkipCount { get; set; }
        internal static ConfigEntry<bool> EventSkip { get; set; }
        internal static ConfigEntry<int> EventSkipCount { get; set; }

        internal static void Initialize()
        {
            // debut animation viewer
            RestartAnimKey = Plugin.Config.Bind("Debut Animation Viewer", "Restart Animation Key", KeyCode.R,
                new ConfigDescription($"Press this key to restart the character debut animation"));

            NextBgKey = Plugin.Config.Bind("Debut Animation Viewer", "Next BG Key", KeyCode.T,
                new ConfigDescription($"Press this key to switch to the next background image"));

            PreviousBgKey = Plugin.Config.Bind("Debut Animation Viewer", "Previous BG Key", KeyCode.Y,
                new ConfigDescription($"Press this key to switch to the previous background image"));

            // mods
            IntroSkip = Plugin.Config.Bind("Misc. Mods", "Intro Skip", false,
                new ConfigDescription($"Skip the CAPCOM screen and opening video"));

            StorySkip = Plugin.Config.Bind("Story Skip DiVE", "Enabled", true,
                new ConfigDescription($"Adds a button to skip replaying (Normal mode) story stages and receive the rewards"));

            StorySkipCount = Plugin.Config.Bind("Story Skip DiVE", "Amount", 10,
                new ConfigDescription($"The number of times to skip the stage", new AcceptableValueRange<int>(1, 10000)));

            EventSkip = Plugin.Config.Bind("Event Skip DiVE", "Enabled", true,
                new ConfigDescription($"Adds a button to skip replaying (Normal mode) event stages and receive the rewards"));

            EventSkipCount = Plugin.Config.Bind("Event Skip DiVE", "Amount", 10,
                new ConfigDescription($"The number of times to skip the stage", new AcceptableValueRange<int>(1, 10000)));
        }
    }
}