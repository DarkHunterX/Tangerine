using System;
using System.Collections.Generic;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class CharacterInfo
{
    internal static bool dnaEnabled = false;

    private static List<NetCharacterInfo> character_mods = new();
    private static List<NetCharacterSkinInfo> characterSkin_mods = new();
    private static List<NetCharacterSkillInfo> characterSkill_mods = new();
    private static List<NetCharacterDNAInfo> characterDNA_mods = new();
    private static List<NetCharacterDNAInfo> characterRDNA_mods = new();
    private static List<NetCharacterDNAInfo> characterRDNA_invalidRange = new();
    private static List<NetCharacterDNAInfo> characterRDNA_disabled = new();
    private static List<NetCharacterDNALinkInfo> characterIDNA_mods = new();
    private static List<NetCharacterDNALinkInfo> characterIDNA_disabled = new();

    private static Dictionary<string, NetCharacterSkinInfo> _listCharacterSkin = new();
    private static Dictionary<string, NetCharacterSkillInfo> _listCharacterSkill = new();
    private static Dictionary<string, NetCharacterDNAInfo> _listCharacterDNA = new();

    internal static void Clear(bool clearDNA = true)
    {
        character_mods.Clear();
        characterSkin_mods.Clear();
        characterSkill_mods.Clear();
        characterDNA_mods.Clear();
        if (clearDNA)
        {
            characterRDNA_mods.Clear();
            characterRDNA_disabled.Clear();
            characterIDNA_mods.Clear();
            characterIDNA_disabled.Clear();
        }
    }

    internal static bool HasData()
    {
        if (character_mods.Count > 0)
            return true;
        if (characterSkin_mods.Count > 0)
            return true;
        if (characterSkill_mods.Count > 0)
            return true;
        if (characterDNA_mods.Count > 0)
            return true;
        if (characterRDNA_mods.Count > 0)
            return true;
        if (characterIDNA_mods.Count > 0)
            return true;
        return false;
    }

    internal static bool HasRemovedDNA()
    {
        if (characterRDNA_disabled.Count > 0)
            return true;
        if (characterIDNA_disabled.Count > 0)
            return true;
        return false;
    }

    private static void CharacterSkinToDict()
    {
        _listCharacterSkin.Clear();
        foreach (var skin in CharacterService.Instance._listCharacterSkin)
            _listCharacterSkin.Add($"{skin.CharacterID}_{skin.SkinId}", skin);
    }

    private static void CharacterSkinToList()
    {
        CharacterService.Instance._listCharacterSkin.Clear();
        foreach (var skin in _listCharacterSkin)
            CharacterService.Instance._listCharacterSkin.Add(skin.Value);
    }

    private static void CharacterSkillToDict()
    {
        _listCharacterSkill.Clear();
        foreach (var skill in CharacterService.Instance._listCharacterSkill)
            _listCharacterSkill.Add($"{skill.CharacterID}_{skill.Slot}", skill);
    }

    private static void CharacterSkillToList()
    {
        CharacterService.Instance._listCharacterSkill.Clear();
        foreach (var skill in _listCharacterSkill)
            CharacterService.Instance._listCharacterSkill.Add(skill.Value);
    }

    private static void CharacterDnaToDict()
    {
        _listCharacterDNA.Clear();
        foreach (var dna in CharacterService.Instance._listCharacterDNA)
            _listCharacterDNA.Add($"{dna.CharacterID}_{dna.SlotID}_{dna.SkillID}", dna);
    }

    private static void CharacterDnaToList()
    {
        CharacterService.Instance._listCharacterDNA.Clear();
        foreach (var dna in _listCharacterDNA)
            CharacterService.Instance._listCharacterDNA.Add(dna.Value);
    }

    internal static void Remove()
    {
        foreach (var character in character_mods)
            CharacterService.Instance._dicCharacter.Remove(character.CharacterID);

        CharacterSkinToDict();
        foreach (var skin in characterSkin_mods)
            _listCharacterSkin.Remove($"{skin.CharacterID}_{skin.SkinId}");
        CharacterSkinToList();

        CharacterSkillToDict();
        foreach (var skill in characterSkill_mods)
            _listCharacterSkill.Remove($"{skill.CharacterID}_{skill.Slot}");
        CharacterSkillToList();

        CharacterDnaToDict();
        foreach (var dna in characterDNA_mods)
            _listCharacterDNA.Remove($"{dna.CharacterID}_{dna.SlotID}_{dna.SkillID}");
        CharacterDnaToList();

        RemoveDnaData();
    }

    internal static void Restore()
    {
        foreach (var character in character_mods)
            CharacterService.Instance._dicCharacter.Add(character.CharacterID, character);
        foreach (var skin in characterSkin_mods)
            CharacterService.Instance._listCharacterSkin.Add(skin);
        foreach (var skill in characterSkill_mods)
            CharacterService.Instance._listCharacterSkill.Add(skill);
        foreach (var dna in characterDNA_mods)
            CharacterService.Instance._listCharacterDNA.Add(dna);
        RestoreDnaData();
    }

    internal static void RemoveDnaData()
    {
        if (!dnaEnabled)
        {
            CharacterDnaToDict();
            foreach (var rDNA in characterRDNA_disabled)
                _listCharacterDNA.Remove($"{rDNA.CharacterID}_{rDNA.SlotID}_{rDNA.SkillID}");
            CharacterDnaToList();

            foreach (var iDNA in characterIDNA_disabled)
                CharacterService.Instance._dicCharacterDNALink.Remove(iDNA.CharacterID);
        }
        else
        {
            CharacterDnaToDict();
            foreach (var rDNA in characterRDNA_mods)
                _listCharacterDNA.Remove($"{rDNA.CharacterID}_{rDNA.SlotID}_{rDNA.SkillID}");
            foreach (var rDNA in characterRDNA_invalidRange)
                _listCharacterDNA.Remove($"{rDNA.CharacterID}_{rDNA.SlotID}_{rDNA.SkillID}");
            CharacterDnaToList();

            foreach (var iDNA in characterIDNA_mods)
                CharacterService.Instance._dicCharacterDNALink.Remove(iDNA.CharacterID);
        }
    }

    internal static void RestoreDnaData()
    {
        if (!dnaEnabled)
        {
            foreach (var rDNA in characterRDNA_disabled)
                CharacterService.Instance._listCharacterDNA.Add(rDNA);
            foreach (var iDNA in characterIDNA_disabled)
                CharacterService.Instance._dicCharacterDNALink.Add(iDNA.CharacterID, iDNA);
        }
        else
        {
            foreach (var rDNA in characterRDNA_mods)
                CharacterService.Instance._listCharacterDNA.Add(rDNA);
            foreach (var iDNA in characterIDNA_disabled)
                CharacterService.Instance._dicCharacterDNALink.Add(iDNA.CharacterID, iDNA);
        }
    }

    private static void CheckCharacterSkill(NetCharacterSkillInfo netSkill, int skillLV)
    {
        if (netSkill.Level > skillLV)
            netSkill.Level = (short)skillLV;
    }

    private static bool CheckCharacterPassive(NetCharacterSkillInfo netSkill, EXP_TABLE exp_TABLE, int skillID)
    {
        if (OrangeDataManager.Instance.SKILL_TABLE_DICT.TryGetValue(skillID, out SKILL_TABLE skill_TABLE))
        {
            if (netSkill.Level > skill_TABLE.n_LVMAX)
                netSkill.Level = (byte)skill_TABLE.n_LVMAX;

            return true;
        }
        return false;
    }

    internal static void ValidateCharacterInfo()
    {
        foreach (var character in CharacterService.Instance._dicCharacter)
        {
            if (!OrangeDataManager.Instance.CHARACTER_TABLE_DICT.TryGetValue(character.Value.CharacterID, out CHARACTER_TABLE character_TABLE))
                character_mods.Add(character.Value);
            else if (!OrangeDataManager.Instance.SKIN_TABLE_DICT.TryGetValue(character.Value.Skin, out SKIN_TABLE skin_TABLE))
                character.Value.Skin = 0; // None
        }

        foreach (var skin in CharacterService.Instance._listCharacterSkin)
        {
            if (!OrangeDataManager.Instance.CHARACTER_TABLE_DICT.TryGetValue(skin.CharacterID, out CHARACTER_TABLE character_TABLE))
                characterSkin_mods.Add(skin);
            else if (!OrangeDataManager.Instance.SKIN_TABLE_DICT.TryGetValue(skin.SkinId, out SKIN_TABLE skin_TABLE))
                characterSkin_mods.Add(skin);
        }

        var exp_TABLE = OrangeTableHelper.Instance.GetExpTable(PlayerService.Instance._playerInfo.Exp);
        foreach (var skill in CharacterService.Instance._listCharacterSkill)
        {
            if (!OrangeDataManager.Instance.CHARACTER_TABLE_DICT.TryGetValue(skill.CharacterID, out CHARACTER_TABLE character_TABLE))
                characterSkill_mods.Add(skill);
            else
            {
                if (skill.Slot == 1)
                    CheckCharacterSkill(skill, exp_TABLE.n_SKILL1_LV);
                else if (skill.Slot == 2)
                    CheckCharacterSkill(skill, exp_TABLE.n_SKILL2_LV);

                else if (skill.Slot == 3 && !CheckCharacterPassive(skill, exp_TABLE, character_TABLE.n_PASSIVE_1))
                    characterSkill_mods.Add(skill);
                else if (skill.Slot == 4 && !CheckCharacterPassive(skill, exp_TABLE, character_TABLE.n_PASSIVE_2))
                    characterSkill_mods.Add(skill);
                else if (skill.Slot == 5 && !CheckCharacterPassive(skill, exp_TABLE, character_TABLE.n_PASSIVE_3))
                    characterSkill_mods.Add(skill);
                else if (skill.Slot == 6 && !CheckCharacterPassive(skill, exp_TABLE, character_TABLE.n_PASSIVE_4))
                    characterSkill_mods.Add(skill);
                else if (skill.Slot == 7 && !CheckCharacterPassive(skill, exp_TABLE, character_TABLE.n_PASSIVE_5))
                    characterSkill_mods.Add(skill);
                else if (skill.Slot == 8 && !CheckCharacterPassive(skill, exp_TABLE, character_TABLE.n_PASSIVE_6))
                    characterSkill_mods.Add(skill);
            }
        }

        // build DNA table
        Dictionary<int, Dictionary<int, List<int>>> _dicDnaInfo = new();
        Dictionary<int, List<int>> _dicDnaGroup = new();
        foreach (var dnaSkill in OrangeDataManager.Instance.RANDOMSKILL_TABLE_DICT.Values)
        {
            if (!_dicDnaGroup.TryGetValue(dnaSkill.n_GROUP, out List<int> dnaSkills))
                _dicDnaGroup[dnaSkill.n_GROUP] = new List<int> { dnaSkill.n_SKILL };
            else
                dnaSkills.Add(dnaSkill.n_SKILL);
        }
        foreach (var dna_TABLE in OrangeDataManager.Instance.DNA_TABLE_DICT.Values)
        {
            if (_dicDnaGroup.TryGetValue(dna_TABLE.n_GROUP, out List<int> dnaSkills))
            {
                if (!_dicDnaInfo.TryGetValue(dna_TABLE.n_CHARACTER, out Dictionary<int, List<int>> dnaInfo))
                    _dicDnaInfo[dna_TABLE.n_CHARACTER] = new Dictionary<int, List<int>> {{ dna_TABLE.n_SLOT, dnaSkills }};
                else
                    dnaInfo.Add(dna_TABLE.n_SLOT, dnaSkills);
            }
        }

        _listCharacterDNA.Clear();
        foreach (var dna in CharacterService.Instance._listCharacterDNA)
        {
            if (dna.SlotID < 4)
            {
                if (!OrangeDataManager.Instance.CHARACTER_TABLE_DICT.TryGetValue(dna.CharacterID, out CHARACTER_TABLE character_TABLE))
                    characterDNA_mods.Add(dna);
                else
                {
                    if (!_dicDnaInfo.TryGetValue(character_TABLE.n_ID, out Dictionary<int, List<int>> dnaInfo))
                        characterDNA_mods.Add(dna);
                    else if (dnaInfo.TryGetValue(dna.SlotID, out List<int> dnaSkills) && !dnaSkills.Contains(dna.SkillID))
                        characterDNA_mods.Add(dna);
                }
            }
            else if (dna.SlotID >= 4 && dna.SlotID <= 8)
            {
                if (!dnaEnabled)
                    characterRDNA_disabled.Add(dna);
                else if (!OrangeDataManager.Instance.CHARACTER_TABLE_DICT.TryGetValue(dna.CharacterID, out CHARACTER_TABLE character_TABLE))
                    characterRDNA_mods.Add(dna);
                else
                {
                    if (!_dicDnaInfo.TryGetValue(character_TABLE.n_ID, out Dictionary<int, List<int>> dnaInfo))
                        characterRDNA_mods.Add(dna);
                    else if (dnaInfo.TryGetValue(dna.SlotID, out List<int> dnaSkills) && !dnaSkills.Contains(dna.SkillID))
                        characterRDNA_mods.Add(dna);
                    else
                        _listCharacterDNA.TryAdd($"{dna.CharacterID}_{dna.SlotID}", dna);
                }
            }
            else
                characterRDNA_invalidRange.Add(dna);
        }

        var characterDNALink = new Dictionary<int, int>();
        foreach (var dnaLink in CharacterService.Instance._dicCharacterDNALink)
        {
            if (!dnaEnabled)
                characterIDNA_disabled.Add(dnaLink.Value);
            else
            {
                if (!OrangeDataManager.Instance.CHARACTER_TABLE_DICT.TryGetValue(dnaLink.Value.CharacterID, out CHARACTER_TABLE character_TABLE))
                    characterIDNA_mods.Add(dnaLink.Value);
                else if (!OrangeDataManager.Instance.CHARACTER_TABLE_DICT.TryGetValue(dnaLink.Value.LinkedCharacterID, out CHARACTER_TABLE characterLink_TABLE))
                    characterIDNA_mods.Add(dnaLink.Value);
                else if (characterDNALink.ContainsKey(dnaLink.Value.LinkedCharacterID))
                    characterIDNA_mods.Add(dnaLink.Value);
                else
                {
                    characterDNALink.Add(dnaLink.Value.LinkedCharacterID, dnaLink.Value.CharacterID);

                    var invalidSlot = new List<sbyte>();
                    foreach (var slot in dnaLink.Value.LinkedSlotID)
                    {
                        if (slot < 4 || slot > 8)
                            invalidSlot.Add(slot);
                        else if (!_listCharacterDNA.ContainsKey($"{dnaLink.Value.LinkedCharacterID}_{slot}"))
                            invalidSlot.Add(slot);
                    }

                    foreach (var slot in invalidSlot)
                        dnaLink.Value.LinkedSlotID.Remove(slot);
                    if (dnaLink.Value.LinkedSlotID.Count == 0)
                        characterIDNA_mods.Add(dnaLink.Value);
                }
            }
        }
    }
}