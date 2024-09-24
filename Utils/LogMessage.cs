using System;

namespace Tangerine.Utils
{
    internal class LogMessage
    {
        internal static bool JsonText = true;
        internal static bool JsonData = true;
        internal static bool JsonConst = true;
        internal static bool AssetBundle = true;
        internal static bool AssetRemap = true;
        internal static bool FileRemap = true;
        internal static bool CriwareAudio = true;
        internal static bool PluginDLL = true;
        internal static bool ChargeFx = true;
        internal static bool DebutEgg = true;
        internal static bool TextureCache = true;

        internal static void LogMsg(string msg, bool msgType = true)
        {
            if (msgType)
            {
                Plugin.Log.LogMessage(msg);
            }
        }

        internal static void LogInfo(string msg, bool msgType = true)
        {
            if (msgType)
            {
                Plugin.Log.LogInfo(msg);
            }
        }

        internal static void LogWarning(string msg, bool msgType = true)
        {
            if (msgType)
            {
                Plugin.Log.LogWarning(msg);
            }
        }

        internal static void LogError(string msg, bool msgType = true)
        {
            if (msgType)
            {
                Plugin.Log.LogError(msg);
            }
        }
    }
}
