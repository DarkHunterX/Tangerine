using System;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using Tangerine.Manager;
using Tangerine.Utils;

namespace Tangerine.Patchers
{
    /// <summary>
    /// Contains methods for adding and updating character charge FX
    /// </summary>
    public class TangerineChargeFx
    {
        private static Harmony _harmony;

        private static bool _chargeDataScriptObjUnpatched = false;
        private static PlayerBuilder.PlayerBuildParam _SetPBP = new();

        // convert ChargeData list to dict for fast lookup
        private static Dictionary<string, ChargeData> _dictChargeDatas = new();

        private static readonly Dictionary<string, ChargeData> _originalChargeFxDatas = new();
        internal static readonly ModDictionary<string, ChargeData> ChargeFxDatas = new();
        internal static readonly Dictionary<string, string[]> ChargeFxDatasEX = new();

        private readonly string _modGuid;

        static TangerineChargeFx()
        {
            ChargeFxDatas.BaseChangedEvent += ApplyChargeFxPatch;
            ChargeFxDatas.BaseResetEvent += ResetChargeFxPatch;
        }

        internal TangerineChargeFx(string modGuid)
        {
            _modGuid = modGuid;
        }

        internal static void InitializeHarmony(Harmony harmony)
        {
            _harmony = harmony;
            _harmony.PatchAll(typeof(TangerineChargeFx));
        }

        /// <summary>
        /// Adds a Charge FX to the game's dictionary to allow it to be loaded
        /// </summary>
        /// <param name="chargeFx">Charge FX entry to add</param>
        /// <param name="chargeListEX">Charge FX entry names for each skill chip</param>
        public void AddChargeFxData(ChargeData chargeFx, string[] chargeListEX)
        {
            ChargeFxDatas.Set(_modGuid, chargeFx.sCharacterStr, chargeFx);
            ChargeFxDatasEX.Add(chargeFx.sCharacterStr, chargeListEX);
        }

        /// <summary>
        /// Removes a Charge FX entry that was added before
        /// </summary>
        /// <param name="name">Name of the Charge FX entry to remove</param>
        public bool RemoveChargeFxData(string name)
        {
            if (ChargeFxDatas.TryGetValue(_modGuid, name, out var fx))
            {
                ChargeFxDatas.Remove(_modGuid, name);
                ChargeFxDatasEX.Remove(name);

                // No need to apply anything to the game here, as the event in the Base dictionary will do it
                return true;
            }

            return false;
        }

        private static void ApplyChargeFxPatch(string name, BaseChangeType changeType)
        {
            if (!_chargeDataScriptObjUnpatched)
                return;

            ChargeData fx;
            switch (changeType)
            {
                case BaseChangeType.Add:
                    if (_dictChargeDatas.TryGetValue(name, out fx))
                    {
                        // Store original for restoring later
                        _originalChargeFxDatas[name] = fx;
                    }
                    goto case BaseChangeType.Update;
                case BaseChangeType.Update:
                    // Update the game's dictionary
                    _dictChargeDatas[name] = ChargeFxDatas.Base[name];
                    break;
                case BaseChangeType.Remove:
                    if (_originalChargeFxDatas.TryGetValue(name, out fx))
                    {
                        // Patch original value back
                        _dictChargeDatas[name] = fx;
                    }
                    else
                        _dictChargeDatas.Remove(name);
                    break;
            }
        }

        private static void ResetChargeFxPatch(IEnumerable<string> names)
        {
            if (!_chargeDataScriptObjUnpatched)
                return;

            // Unpatch existing Base
            foreach (string name in names)
                _dictChargeDatas.Remove(name);

            // Patch original values back
            foreach (var pair in _originalChargeFxDatas)
                _dictChargeDatas[pair.Key] = pair.Value;

            // Reset original dict and fill it again based on the new Base
            _originalChargeFxDatas.Clear();

            // Patch new Base
            foreach (var pair in ChargeFxDatas.Base)
            {
                if (_dictChargeDatas.TryGetValue(pair.Key, out var value))
                    _originalChargeFxDatas[pair.Key] = value;

                _dictChargeDatas[pair.Key] = pair.Value;
            }
            OrangeBattleServerManager.Instance.tChargeDataScriptObj.listChargeDatas = ChargeDataToList(_dictChargeDatas);
        }

        private static Dictionary<string, ChargeData> ChargeDataToDict(Il2CppSystem.Collections.Generic.List<ChargeData> listChargeDatas)
        {
            Dictionary<string, ChargeData> dictChargeDatas = new();
            foreach (var fx in listChargeDatas)
                dictChargeDatas.Add(fx.sCharacterStr, fx);
            return dictChargeDatas;
        }

        private static Il2CppSystem.Collections.Generic.List<ChargeData> ChargeDataToList(Dictionary<string, ChargeData> dictChargeDatas)
        {
            Il2CppSystem.Collections.Generic.List<ChargeData> listChargeDatas = new();
            foreach (var fx in dictChargeDatas)
                listChargeDatas.Add(fx.Value);
            return listChargeDatas;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlayerBuilder), nameof(PlayerBuilder.CreatePlayer))]
        private static void test(PlayerBuilder __instance)
        {
            _SetPBP = __instance.SetPBP;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ChargeDataScriptObj), nameof(ChargeDataScriptObj.GetChargeData))]
        private static bool GetChargeData_Prefix(ChargeDataScriptObj __instance, MethodBase __originalMethod, ref string sCharacterID)
        {
            if (!_chargeDataScriptObjUnpatched)
            {
                _dictChargeDatas = ChargeDataToDict(__instance.listChargeDatas);
                var dictChargeDatas = new Dictionary<string, ChargeData>(_dictChargeDatas);

                // This will override existing charge effects in the ChargeData list
                foreach (var fx in ChargeFxDatas.Base.Values)
                {
                    if (dictChargeDatas.TryGetValue(fx.sCharacterStr, out var orgFx))
                    {
                        // Store original for restoring later
                        _originalChargeFxDatas.Add(fx.sCharacterStr, orgFx);
                        LogMessage.LogWarning($"Updating ChargeFx: {fx.sCharacterStr}", LogMessage.ChargeFx);
                    }
                    else
                        LogMessage.LogWarning($"Adding ChargeFx: {fx.sCharacterStr}", LogMessage.ChargeFx);

                    dictChargeDatas[fx.sCharacterStr] = fx;
                }
                __instance.listChargeDatas = ChargeDataToList(dictChargeDatas);
                _chargeDataScriptObjUnpatched = true;
            }

            if (ChargeFxDatasEX.TryGetValue(sCharacterID, out string[] chargeEX))
                sCharacterID = chargeEX[_SetPBP.EnhanceEXIndex[0] - 1];

            // run original code
            return true;
        }
    }
}