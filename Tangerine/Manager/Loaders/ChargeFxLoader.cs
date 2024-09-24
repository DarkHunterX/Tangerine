using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tangerine.Patchers;


namespace Tangerine.Manager.Loaders
{
    internal static class ChargeFxLoader
    {
        private const string JsonFile = "ChargeFxConfig.json";

        public static bool Load(string modPath, TangerineChargeFx loader)
        {
            try
            {
                var node = JsonNode.Parse(File.ReadAllText(Path.Combine(modPath, JsonFile)));
                var list = node["ListChargeFx"]?.AsArray();

                if (list == null)
                {
                    Plugin.Log.LogError($"Failed to read {JsonFile} for mod \"{modPath}\"");
                    return false;
                }

                foreach (var chargeFx in list.Select(DeserializeChargeFx))
                {
                    loader.AddChargeFxData(chargeFx.chargeData, chargeFx.chargeListEX);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Failed to read {JsonFile} for mod \"{modPath}\": {e}");
                return false;
            }

            return true;
        }

        public static void Unload(string modId)
        {
            TangerineLoader.FilePaths.OnModDisabled(modId);
        }

        public static bool HasContentToLoad(string modPath)
        {
            return File.Exists(Path.Combine(modPath, JsonFile));
        }

        private static (ChargeData chargeData, string[] chargeListEX) DeserializeChargeFx(JsonNode node)
        {
            var chargeData = new ChargeData
            (
                node["sCharacterStr"].Deserialize<string>(),
                node["sABDlPath"].Deserialize<string>(),
                node["FXRoot"].Deserialize<ChargeData.ChargeFXRoot>(),
                node["sChargeLV1FX"].Deserialize<string>(),
                node["sChargeLV2FX"].Deserialize<string>(),
                node["sChargeLV3FX"].Deserialize<string>(),
                node["sChargeStartFX"].Deserialize<string>(),
                node["sChargeLV1FX2"].Deserialize<string>(),
                node["sChargeLV2FX2"].Deserialize<string>(),
                node["sChargeLV3FX2"].Deserialize<string>(),
                node["sChargeStartFX2"].Deserialize<string>()
            )
            {
                nUpdateInAdvanceLV1FX = node["nUpdateInAdvanceLV1FX"].Deserialize<int>(),
                nUpdateInAdvanceLV2FX = node["nUpdateInAdvanceLV2FX"].Deserialize<int>(),
                nUpdateInAdvanceLV3FX = node["nUpdateInAdvanceLV3FX"].Deserialize<int>(),
                nUpdateInAdvanceLV1FX2 = node["nUpdateInAdvanceLV1FX2"].Deserialize<int>(),
                nUpdateInAdvanceLV2FX2 = node["nUpdateInAdvanceLV2FX2"].Deserialize<int>(),
                nUpdateInAdvanceLV3FX2 = node["nUpdateInAdvanceLV3FX2"].Deserialize<int>()
            };

            var chargeEX = new string[3];
            var chargeList = node["sSkillEX"]?.Deserialize<string>().Split(',');
            if (chargeList != null)
            {
                for (int i = 0; i < chargeList.Length && i < chargeEX.Length; i++)
                    chargeEX[i] = chargeList[i];
                return (chargeData, chargeEX);
            }
            else
                return (chargeData, null);
        }
    }
}