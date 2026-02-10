using System;

namespace Tangerine.Utils
{
    internal class LogMessage
    {
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
