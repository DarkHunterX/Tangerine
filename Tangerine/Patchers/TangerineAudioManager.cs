using CallbackDefs;
using HarmonyLib;
using System;
using System.Collections.Generic;
using Tangerine.Utils;

namespace Tangerine.Patchers
{
    internal static class TangerineAudioManager
    {
        private static readonly Dictionary<string, bool> _acbIsLoading = new();

        internal static void InitializeHarmony(Harmony harmony)
        {
            harmony.PatchAll(typeof(TangerineAudioManager));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AudioManager), nameof(AudioManager.GetAcb))]
        private static void GetAcbPrefix(string s_acb)
        {
            if (s_acb != null && s_acb != string.Empty && !AudioManager.Instance.orangePool.ContainsKey(s_acb))
            {
                LogMessage.LogWarning($"ACB is not loaded: {s_acb}", LogMessage.CriwareAudio);
                if (!_acbIsLoading.ContainsKey(s_acb))
                {
                    LogMessage.LogInfo($"Preloading ACB: {s_acb}", LogMessage.CriwareAudio);
                    
                    _acbIsLoading[s_acb] = true;
                    AudioManager.Instance.PreloadAtomSource(s_acb, (Callback)new Action(() =>
                    {
                        LogMessage.LogInfo($"Preloading finished for ACB: {s_acb}", LogMessage.CriwareAudio);

                        lock (_acbIsLoading)
                        {
                            _acbIsLoading[s_acb] = false;
                        }
                    }));
                }

                LogMessage.LogInfo($"Waiting for ACB {s_acb} to be loaded...", LogMessage.CriwareAudio);

                // TODO: lock?
                while (_acbIsLoading[s_acb])
                {
                    // Wait
                }

                LogMessage.LogMsg($"Finished waiting for ACB {s_acb}", LogMessage.CriwareAudio);
                _acbIsLoading.Remove(s_acb);
            }
        }
    }
}
