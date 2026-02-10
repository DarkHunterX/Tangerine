using BepInEx;
using CallbackDefs;
using HarmonyLib;
using OrangeAudio;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using TangerineBaseMods.Config;

namespace TangerineBaseMods.Patches.Toggle;
public class SkinVoiceAddon
{
    internal static void InitializeHarmony(Harmony harmony)
    {
        if (Configuration.SkinVoiceAddon.Value)
        {
            harmony.PatchAll(typeof(SkinVoiceAddon));
            Plugin.RemoveObsoleteMod_SkinVoiceAddon();
            LoadDict();
        }
    }

    #region Variable
    private static List<SKIN_VOICE_TABLE> _SKIN_VOICE_TABLE_DICT = new();
    private static List<cSkinVoiceData> _SKIN_VOICE_ORIGIN_DICT = new();
    private static cSkinVoiceData CurVoice;
    private static int[] curSelect = { 0, 0 };
    #endregion
    #region Classes
    class SKIN_VOICE_TABLE : CapTableBase
    {
        public string s_NAME { get; set; } = "Dummy";
        public int n_CHARAID { get; set; } = -1;
        public int n_SKINID { get; set; } = -1;
        public string s_VOICE { get; set; } = "Dummy";
        public string s_SE_CHARA { get; set; } = "Dummy";
        public string s_SE_SKILL { get; set; } = "Dummy";
        public string s_VOICE_SKILL1 { get; set; } = "";
        public string s_VOICE_SKILL2 { get; set; } = "";
        public string s_VOICE_VICTORY { get; set; } = "";

        public SKIN_VOICE_TABLE() { }
    }

    struct cSkinVoiceData
    {
        public int CharaID { get; set; }
        public string Voice { get; set; }
        public string SkillSE { get; set; }
        public string CharaSE { get; set; }
        public string PreVoice { get; set; } = "";
        public string Victory { get; set; } = "";
        public string NewVictory { get; set; } = "";
        public string Skill1 { get; set; } = "";
        public string NewSkill1 { get; set; } = "";
        public string Skill2 { get; set; } = "";
        public string NewSkill2 { get; set; } = "";
        public bool isCustomVoice { get; set; } = false;

        public cSkinVoiceData() { }
    }
    #endregion

    #region Main Patching
    [HarmonyPrefix, HarmonyPatch(typeof(GoCheckUI), nameof(GoCheckUI.CharacterSetByNet))]
    static void fw_CharacterSetByNet(GoCheckUI __instance)
    {
        try
        {
            NetCharacterInfo netCharacterInfo = __instance.refSelectCharacter.tNetCharacterInfo.Cast<NetCharacterInfo>();
            UpdateCurVoice(netCharacterInfo.CharacterID, netCharacterInfo.Skin);
        }
        catch (Exception e)
        {
            Plugin.Log.LogError(e.Message);
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterInfoUI), nameof(CharacterInfoUI.RefreshModelHelper))]
    static void fw_RefreshModelHelper(CharacterInfoUI __instance, SKIN_TABLE skinTable)
    {
        UpdateCurVoice(__instance.characterTable.n_ID, skinTable == null ? 0 : skinTable.n_ID);

        __instance.characterTable.s_VOICE = CurVoice.Voice;
        __instance.characterTable.s_SE_CHARA = CurVoice.CharaSE;
        __instance.characterTable.s_SE_SKILL = CurVoice.SkillSE;
    }

    /* Note: This one will hook all the sound on the character debut screen
    [HarmonyPrefix, HarmonyPatch(typeof(CharacterAnimatonEvent), nameof(CharacterAnimatonEvent.PlaySE))]*/

    /*This one is played for the victory one. Any animation sound that played by animation will use this*/
    [HarmonyPrefix, HarmonyPatch(typeof(AnimatorSoundHelper), nameof(AnimatorSoundHelper.PlaySE))]
    public static bool fw_AnimationHelper_PlaySE(AnimatorSoundHelper __instance, string param)
    {
        return PatchingVoice(param, __instance);
    }

    /* Note: This one used for the older character */
    [HarmonyPrefix, HarmonyPatch(typeof(AnimatorSoundHelper), nameof(AnimatorSoundHelper.PlayVoice))]
    public static bool fw_AnimationHelper_PlayVoice(AnimatorSoundHelper __instance, string param)
    {
        string cue = param.Split(',')[1].Trim();
        string exParam = $"{CurVoice.PreVoice},{cue}";
        return PatchingVoice(param, __instance);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(CharacterControlBase), nameof(CharacterControlBase.PlayVoiceSE), new[] { typeof(string) })]
    public static void fw_CharacterControlBase_PlayVoiceSE(CharacterControlBase __instance, ref string cue)
    {
        if (__instance._refEntity.VoiceID != $"VOICE_{CurVoice.Voice}" || !CurVoice.isCustomVoice)
            return;

        if (cue.Equals(CurVoice.Skill1))
            cue = CurVoice.NewSkill1;

        if (cue.Equals(CurVoice.Skill2))
            cue = CurVoice.NewSkill2;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(OrangeCriSource), nameof(OrangeCriSource.PlaySE), new[] { typeof(string), typeof(string), typeof(float) })]
    public static void fw_OrangeCriSource_PlaySE(OrangeCriSource __instance, string s_acb, ref string cuename, float delay = 0f)
    {
        if (s_acb != $"VOICE_{CurVoice.Voice}" || !CurVoice.isCustomVoice || !s_acb.StartsWith("VOICE_"))
            return;

        if (cuename.Equals(CurVoice.Skill1))
            cuename = CurVoice.NewSkill1;

        if (cuename.Equals(CurVoice.Skill2))
            cuename = CurVoice.NewSkill2;

        /*CriAtomExAcb acbPre = AudioManager.Instance.GetAcb($"VOICE_{CurVoice.PreVoice}", "NULL");
        if (acbPre == null)
            return;

        CriAtomEx.CueInfo cueInfo = default(CriAtomEx.CueInfo);
        if (GetCueInfoById(acbPre, cueid, out cueInfo))
        {
            CriAtomEx.CueInfo cueInfoNew = default(CriAtomEx.CueInfo);
            CriAtomExAcb acbCur = AudioManager.Instance.GetAcb(s_acb, "NULL");
            if (acbPre == null)
                return;

            if (cueInfo.name.Equals(CurVoice.Skill1))
            {
                if (GetCueInfo(acbCur, CurVoice.NewSkill1, out cueInfoNew))
                    cueid = cueInfoNew.id;
            }
            else if (cueInfo.name.Equals(CurVoice.Skill2))
            {
                if (GetCueInfo(acbCur, CurVoice.NewSkill2, out cueInfoNew))
                    cueid = cueInfoNew.id;
            }
        }*/
    }

    #region Patch Loading
    [HarmonyPrefix, HarmonyPatch(typeof(AudioLib), nameof(AudioLib.LoadVoice))]
    static void fw_LoadVoice(AudioLib __instance, ref CHARACTER_TABLE character, CallbackDefs.Callback p_cb)
    {
        if (character.n_ID == CurVoice.CharaID)
            character.s_VOICE = CurVoice.Voice;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AudioLib), nameof(AudioLib.GetVoice))]
    static void fw_GetVoice(AudioLib __instance, ref CHARACTER_TABLE character)
    {
        if (character.n_ID == CurVoice.CharaID)
            character.s_VOICE = CurVoice.Voice;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AudioLib), nameof(AudioLib.LoadSkillSE))]
    static void fw_LoadLoadSkillSE(AudioLib __instance, ref CHARACTER_TABLE character, Callback p_cb)
    {
        if (character.n_ID == CurVoice.CharaID)
            character.s_SE_SKILL = CurVoice.SkillSE;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AudioLib), nameof(AudioLib.GetSkillSE))]
    static void fw_GetSkillSE(AudioLib __instance, ref CHARACTER_TABLE character)
    {
        if (character.n_ID == CurVoice.CharaID)
            character.s_SE_SKILL = CurVoice.SkillSE;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AudioLib), nameof(AudioLib.LoadCharaSE))]
    static void fw_LoadCharaSE(AudioLib __instance, ref CHARACTER_TABLE character, Callback p_cb)
    {
        if (character.n_ID == CurVoice.CharaID)
            character.s_SE_CHARA = CurVoice.CharaSE;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AudioLib), nameof(AudioLib.GetCharaSE))]
    static void fw_GetLoadSkillSE(AudioLib __instance, ref CHARACTER_TABLE character)
    {
        if (character.n_ID == CurVoice.CharaID)
            character.s_SE_CHARA = CurVoice.CharaSE;
    }
    #endregion
    #endregion

    #region Function
    static void LoadDict()
    {
        string[] modMainMenu = Directory.GetDirectories(Plugin.ModsDir);
        foreach (string mod in modMainMenu)
        {
            string tableDir = Path.Combine(mod, "Tables");
            tableDir = Path.Combine(tableDir, "SKIN_VOICE_TABLE.json");
            if (!File.Exists(tableDir))
                continue;

            var Tablenode = JsonNode.Parse(File.ReadAllText(tableDir));
            var list = Tablenode["SKIN_VOICE_TABLE"]?.AsArray();

            //Getting Entry from tables
            foreach (var node in list)
            {
                var table = new SKIN_VOICE_TABLE();
                try
                {
                    int skinID = node["n_SKINID"].Deserialize<int>();
                    SKIN_VOICE_TABLE tableFind = _SKIN_VOICE_TABLE_DICT.FirstOrDefault(x => x.n_SKINID == skinID);
                    if (_SKIN_VOICE_TABLE_DICT.Any(x => x.n_SKINID == skinID))
                    {
                        LogInfoDebug($"Found dupe entry for skin with id {skinID}\nRemoving the previous entry");
                        _SKIN_VOICE_TABLE_DICT.Remove(tableFind);
                    }

                    table.s_NAME = node["s_NAME"].Deserialize<string>();
                    table.n_CHARAID = node["n_CHARAID"].Deserialize<int>();
                    table.n_SKINID = skinID;
                    table.s_VOICE = node["s_VOICE"].Deserialize<string>();
                    table.s_SE_CHARA = node["s_SE_CHARA"].Deserialize<string>();
                    table.s_SE_SKILL = node["s_SE_SKILL"].Deserialize<string>();
                    table.s_VOICE_VICTORY = node["s_VOICE_VICTORY"].Deserialize<string>();
                    table.s_VOICE_SKILL1 = node["s_VOICE_SKILL1"].Deserialize<string>();
                    table.s_VOICE_SKILL2 = node["s_VOICE_SKILL2"].Deserialize<string>();
                }
                catch (Exception)
                {
                    Plugin.Log.LogError($"There is an error when loading data. Entry's Name ({table.s_NAME})");
                    continue;
                }

                _SKIN_VOICE_TABLE_DICT.Add(table);
            }
        }
    }

    static List<SKIN_VOICE_TABLE> GetListSkinByCharacterID(int id)
    {
        var list = new List<SKIN_VOICE_TABLE>();
        foreach (var table in _SKIN_VOICE_TABLE_DICT)
        {
            if (table.n_CHARAID == id)
                list.Add(table);
        }

        return list;
    }

    static bool UpdateCurVoice(int CharaID, int SkinID)
    {
        try
        {
            if (CharaID == curSelect[0] && SkinID == curSelect[1])
                return true;

            curSelect = new int[] { CharaID, SkinID };

            LogInfoDebug($"[UpdateVoice] Get skin List by CharaId ({CharaID})");
            var skinList = GetListSkinByCharacterID(CharaID);

            LogInfoDebug($"[UpdateVoice] Get Original Voice by CharaId ({CharaID})");
            var origin = GetOriginalVoice(CharaID);

            CurVoice = new cSkinVoiceData
            {
                CharaID = CharaID,
                Voice = origin.Voice,
                SkillSE = origin.SkillSE,
                CharaSE = origin.CharaSE
            };

            if (skinList.Count == 0 || SkinID == 0)
            {
                LogInfoDebug($"[UpdateVoice] Skin Voice List: {skinList.Count} - Current Skin: {SkinID}");
                return false;
            }
            else
            {
                LogInfoDebug($"[UpdateVoice] All Skin Voice List (Total: {skinList.Count})");
                for (int i = 0; i < skinList.Count; i++)
                    LogInfoDebug($"- {skinList[i].n_SKINID}");

                SKIN_VOICE_TABLE table = skinList.FirstOrDefault(element => element.n_SKINID == SkinID);
                if (table == null || table.n_CHARAID == 0)
                {
                    //if(table == null)
                    //    Plugin.Log.LogError($"Fail to get Skin Voice Table of the skin with ID: {SkinID}");

                    //if(table.n_CHARAID == 0)
                    //    Plugin.Log.LogError($"Table Skin have n_CHARAID = 0: {SkinID}");
                    return false;
                }

                LogInfoDebug($"[UpdateVoice] Got the Skin Voice List");
                LogInfoDebug($"CharaID:{table.n_CHARAID} - Name:{table.s_NAME} - Voice:{table.s_VOICE} - Skill:{table.s_SE_SKILL} - Victory:{table.s_VOICE_VICTORY}");

                CurVoice = new cSkinVoiceData
                {
                    CharaID = CharaID,
                    Voice = table.s_VOICE,
                    SkillSE = table.s_SE_SKILL,
                    CharaSE = table.s_SE_CHARA,
                    PreVoice = origin.Voice,
                    isCustomVoice = true
                };

                //LogInfoDebug($"[UpdateVOice] Current Voice: {CurVoice.Voice}");

                if (!table.s_VOICE_VICTORY.IsNullOrWhiteSpace())
                {
                    string[] sVictory = table.s_VOICE_VICTORY.Split(',');
                    if (sVictory.Count() != 2)
                        Plugin.Log.LogError($"There is an error with param 's_VOICE_VICTORY' of '{table.s_NAME}'");
                    else
                    {
                        CurVoice.Victory = sVictory[0].Trim();
                        CurVoice.NewVictory = sVictory[1].Trim();
                    }
                }
                else
                    Plugin.Log.LogError($"Param 's_VOICE_VICTORY' of '{table.s_NAME}' is missing or empty");

                if (!table.s_VOICE_SKILL1.IsNullOrWhiteSpace())
                {
                    string[] sSkill1 = table.s_VOICE_SKILL1.Split(',');
                    if (sSkill1.Count() != 2)
                        Plugin.Log.LogError($"There is an error with param 's_VOICE_SKILL1' of '{table.s_NAME}'");
                    else
                    {
                        CurVoice.Skill1 = sSkill1[0].Trim();
                        CurVoice.NewSkill1 = sSkill1[1].Trim();
                    }
                }
                else
                    Plugin.Log.LogError($"Param 's_VOICE_SKILL1' of '{table.s_NAME}' is missing");

                if (!table.s_VOICE_SKILL2.IsNullOrWhiteSpace())
                {
                    string[] sSkill2 = table.s_VOICE_SKILL2.Split(',');
                    if (sSkill2.Count() != 2)
                        Plugin.Log.LogError($"There is an error with param 's_VOICE_SKILL2' of '{table.s_NAME}'");
                    else
                    {
                        CurVoice.Skill2 = sSkill2[0].Trim();
                        CurVoice.NewSkill2 = sSkill2[1].Trim();
                    }
                }
                else
                    Plugin.Log.LogError($"Param 's_VOICE_SKILL2' of '{table.s_NAME}' is missing");
            }

            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.LogError(e.Message);
            return false;
        }
    }

    static cSkinVoiceData GetOriginalVoice(int CharaID)
    {
        var result = new cSkinVoiceData();
        try
        {
            LogInfoDebug($"[GetOriginalVoice] Check if character voice in _SKIN_VOICE_ORIGIN_DICT ({CharaID})");
            if (!_SKIN_VOICE_ORIGIN_DICT.Any(x => x.CharaID == CharaID))
            {
                LogInfoDebug($"[GetOriginalVoice] Get Character Table ({CharaID})");
                CHARACTER_TABLE charaTable = OrangeDataManager.Instance.CHARACTER_TABLE_DICT[CharaID];

                LogInfoDebug($"[GetOriginalVoice] Create cSkinVoiceData");
                LogInfoDebug($"[GetOriginalVoice][{CharaID} - {charaTable.s_VOICE} - {charaTable.s_SE_CHARA} - {charaTable.s_SE_SKILL}]");
                result = new cSkinVoiceData
                {
                    CharaID = CharaID,
                    Voice = charaTable.s_VOICE,
                    CharaSE = charaTable.s_SE_CHARA,
                    SkillSE = charaTable.s_SE_SKILL,
                    isCustomVoice = false
                };

                LogInfoDebug($"[GetOriginalVoice] Count List Skin of the Character");
                if (GetListSkinByCharacterID(CharaID).Count > 0)
                {
                    LogInfoDebug($"[GetOriginVoice] Adding '{result.Voice}' to List");
                    _SKIN_VOICE_ORIGIN_DICT.Add(result);
                }
            }
            else
            {
                result = _SKIN_VOICE_ORIGIN_DICT.Find(x => x.CharaID == CharaID);
                LogInfoDebug($"[GetOriginVoice] Found Character with ID '{CharaID}' and with '{result.Voice}'");
            }
        }
        catch (Exception e)
        {
            Plugin.Log.LogError($"[GetOriginalVoice] {e.Message}");
        }
        return result;
    }

    static bool PatchingVoice(string param, AnimatorSoundHelper helper)
    {
        if (!CurVoice.isCustomVoice || !param.StartsWith("VOICE_"))
            return true;

        //string checkParam;
        string preVoice = $"VOICE_{CurVoice.PreVoice}";
        string curVoice = $"VOICE_{CurVoice.Voice}";

        //checkParam = $"{preVoice},{CurVoice.Victory}";
        //Plugin.Log.LogInfo($"[Patching] System Param {param}");
        //Plugin.Log.LogInfo($"[Patching] Check Param {checkParam}");

        if (param.StartsWith(preVoice))
        {
            var splitParam = param.Split(',');

            AudioManager.Instance.PreloadAtomSource(curVoice, 3, (Callback)new Action(() =>
            {
                if (splitParam[1].Equals(CurVoice.Victory))
                    helper.SoundSource.PlaySE(curVoice, CurVoice.NewVictory, 0f); //Play the new Victory sound
                else
                    helper.SoundSource.PlaySE(curVoice, splitParam[1], 0f); //Play the same voice but in the CurVoice Bundle
            }));
            return false;
        }
        return true;
    }

    static bool GetCueInfoById(CriAtomExAcb acb, int index, out CriAtomEx.CueInfo info)
    {
        CriStructMemory<CriAtomEx.CueInfo> criStructMemory = new CriStructMemory<CriAtomEx.CueInfo>();

        bool flag = CriAtomExAcb.criAtomExAcb_GetCueInfoById(acb.handle, index, criStructMemory.ptr);
        info = new CriAtomEx.CueInfo(criStructMemory.bytes, 0);
        criStructMemory.Dispose();
        return flag;
    }

    static bool GetCueInfo(CriAtomExAcb acb, string cueName, out CriAtomEx.CueInfo info)
    {
        CriStructMemory<CriAtomEx.CueInfo> criStructMemory = new CriStructMemory<CriAtomEx.CueInfo>();

        bool flag = CriAtomExAcb.criAtomExAcb_GetCueInfoByName(acb.handle, cueName, criStructMemory.ptr);
        info = new CriAtomEx.CueInfo(criStructMemory.bytes, 0);
        criStructMemory.Dispose();
        return flag;
    }
    #endregion

    #region DEBUG
    static void LogInfoDebug(string text)
    {
        if (isDebugMode)
            Plugin.Log.LogInfo(text);
    }

    private static readonly bool isDebugMode = false;
    #endregion
}
