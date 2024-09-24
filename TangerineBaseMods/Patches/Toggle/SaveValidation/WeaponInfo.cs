using System;
using System.Linq;
using System.Collections.Generic;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class WeaponInfo
{
    private static List<NetWeaponInfo> weapon_mods = new();
    private static List<NetWeaponSkillInfo> weaponSkill_mods = new();
    private static List<NetWeaponDiVESkillInfo> weaponHSkill_mods = new();
    private static List<NetWeaponExpertInfo> weaponExpert_mods = new();

    private static Dictionary<string, NetWeaponSkillInfo> _listWeaponSkill = new();
    private static Dictionary<string, NetWeaponExpertInfo> _listWeaponExpert = new();

    private static int cacheExp;
    private static EXP_TABLE cacheExpTable;
    private static Dictionary<int, EXP_TABLE> _dictWeaponExp = new();

    internal static void Clear()
    {
        weapon_mods.Clear();
        weaponSkill_mods.Clear();
        weaponHSkill_mods.Clear();
        weaponExpert_mods.Clear();
    }

    internal static bool HasData()
    {
        if (weapon_mods.Count > 0)
            return true;
        if (weaponSkill_mods.Count > 0)
            return true;
        if (weaponHSkill_mods.Count > 0)
            return true;
        if (weaponExpert_mods.Count > 0)
            return true;
        return false;
    }

    private static void WeaponSkillToDict()
    {
        _listWeaponSkill.Clear();
        foreach (var skill in WeaponService.Instance._listWeaponSkill)
            _listWeaponSkill.Add($"{skill.WeaponID}_{skill.Slot}", skill);
    }

    private static void WeaponSkillToList()
    {
        WeaponService.Instance._listWeaponSkill.Clear();
        foreach (var skill in _listWeaponSkill)
            WeaponService.Instance._listWeaponSkill.Add(skill.Value);
    }

    private static void WeaponExpertToDict()
    {
        _listWeaponExpert.Clear();
        foreach (var expert in WeaponService.Instance._listWeaponExpert)
            _listWeaponExpert.Add($"{expert.WeaponID}_{expert.ExpertType}", expert);
    }

    private static void WeaponExpertToList()
    {
        WeaponService.Instance._listWeaponExpert.Clear();
        foreach (var expert in _listWeaponExpert)
            WeaponService.Instance._listWeaponExpert.Add(expert.Value);
    }

    internal static void Remove()
    {
        foreach (var weapon in weapon_mods)
            WeaponService.Instance._dicWeapon.Remove(weapon.WeaponID);

        WeaponSkillToDict();
        foreach (var skill in weaponSkill_mods)
            _listWeaponSkill.Remove($"{skill.WeaponID}_{skill.Slot}");
        WeaponSkillToList();

        foreach (var HSkill in weaponHSkill_mods)
            WeaponService.Instance._dicWeaponDiVESkill.Remove(HSkill.WeaponID);

        WeaponExpertToDict();
        foreach (var expert in weaponExpert_mods)
            _listWeaponExpert.Remove($"{expert.WeaponID}_{expert.ExpertType}");
        WeaponExpertToList();
    }

    internal static void Restore()
    {
        foreach (var weapon in weapon_mods)
            WeaponService.Instance._dicWeapon.Add(weapon.WeaponID, weapon);
        foreach (var skill in weaponSkill_mods)
            WeaponService.Instance._listWeaponSkill.Add(skill);
        foreach (var HSkill in weaponHSkill_mods)
            WeaponService.Instance._dicWeaponDiVESkill.Add(HSkill.WeaponID, HSkill);
        foreach (var expert in weaponExpert_mods)
            WeaponService.Instance._listWeaponExpert.Add(expert);
    }

    private static bool CheckWeaponPassive(NetWeaponSkillInfo netSkill, EXP_TABLE exp_TABLE, int skillID)
    {
        if (OrangeDataManager.Instance.SKILL_TABLE_DICT.TryGetValue(skillID, out SKILL_TABLE skill_TABLE))
        {
            if (netSkill.Level > exp_TABLE.n_ID)
                netSkill.Level = (byte)exp_TABLE.n_ID;

            if (netSkill.Level > skill_TABLE.n_LVMAX)
                netSkill.Level = (byte)skill_TABLE.n_LVMAX;

            return true;
        }
        return false;
    }

    internal static void ValidateWeaponInfo()
    {
        foreach (var weapon in WeaponService.Instance._dicWeapon)
        {
            if (!OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(weapon.Value.WeaponID, out WEAPON_TABLE weapon_TABLE))
                weapon_mods.Add(weapon.Value);
            else if (weapon_TABLE.n_UNLOCK_ID == 0)  // Perfect Save fix - remove invalid weapons
                weapon_mods.Add(weapon.Value);
            else if (!OrangeDataManager.Instance.DISC_TABLE_DICT.TryGetValue(weapon.Value.Chip, out DISC_TABLE chip_TABLE))
                weapon.Value.Chip = 0; // none
        }

        foreach (var skill in WeaponService.Instance._listWeaponSkill)
        {
            if (!OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(skill.WeaponID, out WEAPON_TABLE weapon_TABLE))
                weaponSkill_mods.Add(skill);
            else if (weapon_TABLE.n_UNLOCK_ID == 0)  // Perfect Save fix - remove invalid weapons
                weaponSkill_mods.Add(skill);
            else
            {
                var exp_TABLE = GetExpTable(WeaponService.Instance._dicWeapon[skill.WeaponID].Exp);
                if (WeaponService.Instance._dicWeapon[skill.WeaponID].Exp >= exp_TABLE.n_TOTAL_WEAPONEXP)
                    WeaponService.Instance._dicWeapon[skill.WeaponID].Exp = exp_TABLE.n_TOTAL_WEAPONEXP - 1;

                if (skill.Slot == 1 && !CheckWeaponPassive(skill, exp_TABLE, weapon_TABLE.n_PASSIVE_1))
                    weaponSkill_mods.Add(skill);
                if (skill.Slot == 2 && !CheckWeaponPassive(skill, exp_TABLE, weapon_TABLE.n_PASSIVE_2))
                    weaponSkill_mods.Add(skill);
                if (skill.Slot == 3 && !CheckWeaponPassive(skill, exp_TABLE, weapon_TABLE.n_PASSIVE_3))
                    weaponSkill_mods.Add(skill);
                if (skill.Slot == 4 && !CheckWeaponPassive(skill, exp_TABLE, weapon_TABLE.n_PASSIVE_4))
                    weaponSkill_mods.Add(skill);
                if (skill.Slot == 5 && !CheckWeaponPassive(skill, exp_TABLE, weapon_TABLE.n_PASSIVE_5))
                    weaponSkill_mods.Add(skill);
                if (skill.Slot == 6 && !CheckWeaponPassive(skill, exp_TABLE, weapon_TABLE.n_PASSIVE_6))
                    weaponSkill_mods.Add(skill);
            }
        }

        // build hidden skill table
        Dictionary<int, List<int>> _dicHSkillInfo = new();
        Dictionary<int, List<int>> _dicHSkillGroup = new();
        foreach (var HSkill in OrangeDataManager.Instance.RANDOMSKILL_TABLE_DICT.Values)
        {
            if (!_dicHSkillGroup.TryGetValue(HSkill.n_GROUP, out List<int> HSkills))
                _dicHSkillGroup[HSkill.n_GROUP] = new List<int> { HSkill.n_SKILL };
            else
                HSkills.Add(HSkill.n_SKILL);
        }
        foreach (var weapon_TABLE in OrangeDataManager.Instance.WEAPON_TABLE_DICT.Values)
        {
            if (_dicHSkillGroup.TryGetValue(weapon_TABLE.n_DIVE, out List<int> HSkills))
            {
                if (!_dicHSkillInfo.TryGetValue(weapon_TABLE.n_ID, out List<int> HSkillInfo))
                    _dicHSkillInfo[weapon_TABLE.n_ID] = HSkills;
            }
        }

        foreach (var HSkill in WeaponService.Instance._dicWeaponDiVESkill)
        {
            if (!OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(HSkill.Value.WeaponID, out WEAPON_TABLE weapon_TABLE))
                weaponHSkill_mods.Add(HSkill.Value);
            else if (weapon_TABLE.n_UNLOCK_ID == 0)  // Perfect Save fix - remove invalid weapons
                weaponHSkill_mods.Add(HSkill.Value);
            else if (!OrangeDataManager.Instance.SKILL_TABLE_DICT.TryGetValue(HSkill.Value.SkillID, out SKILL_TABLE skill_TABLE))
                weaponHSkill_mods.Add(HSkill.Value);
            else
            {
                if (!_dicHSkillInfo.TryGetValue(HSkill.Value.WeaponID, out List<int> HSkillInfo))
                    weaponHSkill_mods.Add(HSkill.Value);
                else if (!HSkillInfo.Contains(HSkill.Value.SkillID))
                    weaponHSkill_mods.Add(HSkill.Value);
            }
        }

        var dicUpgrade = new Dictionary<int, UPGRADE_TABLE>();
        foreach (var upgrade_Table in OrangeDataManager.Instance.UPGRADE_TABLE_DICT.Values)
            dicUpgrade.Add(upgrade_Table.n_LV, upgrade_Table);

        foreach (var expert in WeaponService.Instance._listWeaponExpert)
        {
            if (!OrangeDataManager.Instance.WEAPON_TABLE_DICT.TryGetValue(expert.WeaponID, out WEAPON_TABLE weapon_TABLE))
                weaponExpert_mods.Add(expert);
            else if (weapon_TABLE.n_UNLOCK_ID == 0)  // Perfect Save fix - remove invalid weapons
                weaponExpert_mods.Add(expert);
            else
            {
                var weaponLv = GetExpTable(WeaponService.Instance._dicWeapon[expert.WeaponID].Exp).n_ID;
                var upgrade = dicUpgrade.Values.Where(x => x.n_WEAPON_LV <= weaponLv).ToList();

                if (upgrade.Count == 0)
                    expert.ExpertLevel = 0; // reset to zero
                else if (!dicUpgrade.TryGetValue(expert.ExpertLevel, out UPGRADE_TABLE upgrade_TBLE))
                    expert.ExpertLevel = (byte)upgrade.Last().n_LV;
            }
        }
    }

    private static EXP_TABLE GetExpTable(int nExp)
    {
        if (cacheExp > 0 && cacheExp == nExp)
            return cacheExpTable;

        cacheExpTable = null;
        foreach (var exp_TABLE in OrangeDataManager.Instance.EXP_TABLE_DICT)
        {
            if (!_dictWeaponExp.TryGetValue(exp_TABLE.Key, out EXP_TABLE exp_TBL))
                _dictWeaponExp.Add(exp_TABLE.Key, exp_TABLE.Value);

            if (nExp < exp_TABLE.Value.n_TOTAL_WEAPONEXP)
            {
                cacheExpTable = exp_TABLE.Value;
                int num = exp_TABLE.Value.n_TOTAL_WEAPONEXP - nExp;
                if (num <= exp_TABLE.Value.n_WEAPONEXP)
                    break;
            }
        }

        if (cacheExpTable == null)
        {
            if (nExp > 0)
                cacheExpTable = _dictWeaponExp.Last().Value;
            else
                cacheExpTable = _dictWeaponExp.First().Value;
        }

        cacheExp = nExp;
        return cacheExpTable;
    }
}