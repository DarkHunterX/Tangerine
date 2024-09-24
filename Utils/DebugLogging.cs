using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tangerine.Utils
{
    internal class DebugLogging
    {
        private static readonly string DebugSettings = Path.Combine(Plugin.Location, "DebugLogging.json");

        internal static void LoadSettings()
        {
            try
            {
                var node = JsonNode.Parse(File.ReadAllText(DebugSettings));

                LogMessage.JsonText = node["JSON Text"].Deserialize<bool>();
                LogMessage.JsonData = node["JSON Data"].Deserialize<bool>();
                LogMessage.JsonConst = node["JSON Parameters"].Deserialize<bool>();
                LogMessage.AssetBundle = node["AssetBundle"].Deserialize<bool>();
                LogMessage.AssetRemap = node["Asset Remap"].Deserialize<bool>();
                LogMessage.FileRemap = node["File Remap"].Deserialize<bool>();
                LogMessage.CriwareAudio = node["Criware Audio"].Deserialize<bool>();
                LogMessage.PluginDLL = node["Plugin DLL"].Deserialize<bool>();
                LogMessage.ChargeFx = node["ChargeFx"].Deserialize<bool>();
                LogMessage.DebutEgg = node["Debut Easter Egg"].Deserialize<bool>();
                LogMessage.TextureCache = node["Texture Cache"].Deserialize<bool>();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to read {DebugSettings}: {e}");
            }
        }
    }
}
