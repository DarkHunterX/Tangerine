using System;
using System.Collections.Generic;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class MissionInfo
{
    private static List<NetMissionInfoEX> mission_mods = new();
    private static List<NetMissionProgressInfo> missionProgress_mods = new();
    private static List<MissionProgressRestRule> missionResetRule_mods = new();

    internal static void Clear()
    {
        mission_mods.Clear();
        missionProgress_mods.Clear();
        missionResetRule_mods.Clear();
    }

    internal static bool HasData()
    {
        if (mission_mods.Count > 0)
            return true;
        if (missionProgress_mods.Count > 0)
            return true;
        if (missionResetRule_mods.Count > 0)
            return true;
        return false;
    }

    internal static void Remove()
    {
        foreach (var mission in mission_mods)
            MissionService.Instance._dicMission.Remove(mission.MissionID);
        foreach (var missionProgress in missionProgress_mods)
            MissionService.Instance._dicMissionProgress.Remove(missionProgress.CounterID);
        foreach (var missionRule in missionResetRule_mods)
            MissionService.Instance._dicMissionProgressRestRule.Remove(missionRule.CounterID);
    }

    internal static void Restore()
    {
        foreach (var mission in mission_mods)
            MissionService.Instance._dicMission.Add(mission.MissionID, mission);
        foreach (var missionProgress in missionProgress_mods)
            MissionService.Instance._dicMissionProgress.Add(missionProgress.CounterID, missionProgress);
        foreach (var missionRule in missionResetRule_mods)
            MissionService.Instance._dicMissionProgressRestRule.Add(missionRule.CounterID, missionRule);
    }

    internal static void ValidateMissionInfo()
    {
        foreach (var mission in MissionService.Instance._dicMission)
        {
            if (!OrangeDataManager.Instance.MISSION_TABLE_DICT.TryGetValue(mission.Value.MissionID, out MISSION_TABLE mission_TABLE))
                mission_mods.Add(mission.Value);
        }

        foreach (var missionProgress in MissionService.Instance._dicMissionProgress)
        {
            if (!OrangeDataManager.Instance.MISSION_TABLE_DICT.TryGetValue(missionProgress.Value.CounterID, out MISSION_TABLE mission_TABLE))
                missionProgress_mods.Add(missionProgress.Value);
        }

        foreach (var missionRule in MissionService.Instance._dicMissionProgressRestRule)
        {
            if (!OrangeDataManager.Instance.MISSION_TABLE_DICT.TryGetValue(missionRule.Value.CounterID, out MISSION_TABLE mission_TABLE))
                missionResetRule_mods.Add(missionRule.Value);
        }
    }
}