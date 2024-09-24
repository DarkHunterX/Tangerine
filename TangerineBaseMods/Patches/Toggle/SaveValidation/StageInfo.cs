using System;
using System.Collections.Generic;
using System.Linq;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class StageInfo
{
    private static List<NetStageInfoSave> stage_mods = new();
    private static List<NetTAStageInfoData> TAStage_mods = new();
    private static List<NetStageSecretInfoDB> stageSecret_mods = new();
    private static List<NetTowerBossInfo> towerBoss_mods = new();

    private static Dictionary<int, int> dailyEventStage_mods = new();
    private static Dictionary<int, bool> eventStage_mods = new();

    private static Dictionary<string, NetStageSecretInfoDB> _listStageSecret = new();
    private static Dictionary<string, NetTowerBossInfo> _listTowerBoss = new();

    internal static void Clear()
    {
        stage_mods.Clear();
        TAStage_mods.Clear();
        stageSecret_mods.Clear();
        towerBoss_mods.Clear();
        dailyEventStage_mods.Clear();
        eventStage_mods.Clear();
    }

    internal static bool HasData()
    {
        if (stage_mods.Count > 0)
            return true;
        if (TAStage_mods.Count > 0)
            return true;
        if (stageSecret_mods.Count > 0)
            return true;
        if (towerBoss_mods.Count > 0)
            return true;
        if (dailyEventStage_mods.Count > 0)
            return true;
        if (eventStage_mods.Count > 0)
            return true;
        return false;
    }

    private static void StageSecretToDict()
    {
        _listStageSecret.Clear();
        foreach (var stageSecret in StageService.Instance._listStageSecret)
            _listStageSecret.Add($"{stageSecret.StageID}_{stageSecret.SecretID}", stageSecret);
    }

    private static void StageSecretToList()
    {
        StageService.Instance._listStageSecret.Clear();
        foreach (var stageSecret in _listStageSecret)
            StageService.Instance._listStageSecret.Add(stageSecret.Value);
    }

    private static void TowerBossToDict()
    {
        _listTowerBoss.Clear();
        foreach (var towerBoss in StageService.Instance._listTowerBoss)
            _listTowerBoss.Add($"{towerBoss.TowerStageID}_{towerBoss.TowerBossID}", towerBoss);
    }

    private static void TowerBossToList()
    {
        StageService.Instance._listTowerBoss.Clear();
        foreach (var towerBoss in _listTowerBoss)
            StageService.Instance._listTowerBoss.Add(towerBoss.Value);
    }

    internal static void Remove()
    {
        foreach (var stage in stage_mods)
            StageService.Instance._dicStage.Remove(stage.StageID);
        foreach (var TAStage in TAStage_mods)
            StageService.Instance._dicTaStageInfo.Remove(TAStage.StageId);

        StageSecretToDict();
        foreach (var stageSecret in stageSecret_mods)
            _listStageSecret.Remove($"{stageSecret.StageID}_{stageSecret.SecretID}");
        StageSecretToList();

        TowerBossToDict();
        foreach (var towerBoss in towerBoss_mods)
            _listTowerBoss.Remove($"{towerBoss.TowerStageID}_{towerBoss.TowerBossID}");
        TowerBossToList();

        foreach (var dailyEventStage in dailyEventStage_mods)
            StandaloneConsoleStorage.ClientInGameData.DicEventStageDifficulties.Remove(dailyEventStage.Key);
        foreach (var eventStage in eventStage_mods)
            StandaloneConsoleStorage.ClientInGameData.DicEventOpenedByTool.Remove(eventStage.Key);
    }

    internal static void Restore()
    {
        foreach (var stage in stage_mods)
            StageService.Instance._dicStage.Add(stage.StageID, stage);
        foreach (var TAStage in TAStage_mods)
            StageService.Instance._dicTaStageInfo.Add(TAStage.StageId, TAStage);
        foreach (var stageSecret in stageSecret_mods)
            StageService.Instance._listStageSecret.Add(stageSecret);
        foreach (var towerBoss in towerBoss_mods)
            StageService.Instance._listTowerBoss.Add(towerBoss);

        foreach (var dailyEventStage in dailyEventStage_mods)
            StandaloneConsoleStorage.ClientInGameData.DicEventStageDifficulties.Add(dailyEventStage.Key, dailyEventStage.Value);
        foreach (var eventStage in eventStage_mods)
            StandaloneConsoleStorage.ClientInGameData.DicEventOpenedByTool.Add(eventStage.Key, eventStage.Value);
    }

    internal static void ValidateStageInfo()
    {
        foreach (var stage in StageService.Instance._dicStage)
        {
            if (!OrangeDataManager.Instance.STAGE_TABLE_DICT.TryGetValue(stage.Value.StageID, out STAGE_TABLE stage_TABLE))
                stage_mods.Add(stage.Value);
        }
        foreach (var TAStage in StageService.Instance._dicTaStageInfo)
        {
            if (!OrangeDataManager.Instance.STAGE_TABLE_DICT.TryGetValue(TAStage.Value.StageId, out STAGE_TABLE stage_TABLE))
                TAStage_mods.Add(TAStage.Value);
        }
        foreach (var secret in StageService.Instance._listStageSecret)
        {
            if (!OrangeDataManager.Instance.STAGE_TABLE_DICT.TryGetValue(secret.StageID, out STAGE_TABLE stage_TABLE))
                stageSecret_mods.Add(secret);
        }
        foreach (var boss in StageService.Instance._listTowerBoss)
        {
            if (!OrangeDataManager.Instance.STAGE_TABLE_DICT.TryGetValue(boss.TowerStageID, out STAGE_TABLE stage_TABLE))
                towerBoss_mods.Add(boss);
            else if (!OrangeDataManager.Instance.MOB_TABLE_DICT.TryGetValue(boss.TowerBossID, out MOB_TABLE mob_TABLE))
                towerBoss_mods.Add(boss);
        }
    }

    internal static void ValidateEventStageInfo(EXP_TABLE exp_TABLE)
    {
        var dailyEventStage = new Dictionary<int, Dictionary<int, STAGE_TABLE>>();
        var stageIDs = new[] { 15001, 15011, 15021, 15031, 15041 };
        foreach (var stage in OrangeDataManager.Instance.STAGE_TABLE_DICT.Values)
        {
            if (stage.n_TYPE == 2 && stageIDs.Contains(stage.n_MAIN))
                if (!dailyEventStage.TryGetValue(stage.n_MAIN, out Dictionary<int, STAGE_TABLE> eventStages))
                    dailyEventStage.Add(stage.n_MAIN, new Dictionary<int, STAGE_TABLE> {{ stage.n_DIFFICULTY, stage }});
                else
                    eventStages.Add(stage.n_DIFFICULTY, stage);
        }

        var badRank = new List<int>();
        foreach (var eventStage in StandaloneConsoleStorage.ClientInGameData.DicEventStageDifficulties)
        {
            if (!dailyEventStage.TryGetValue(eventStage.Key, out Dictionary<int, STAGE_TABLE> eventStages))
                dailyEventStage_mods.Add(eventStage.Key, eventStage.Value);
            else
            {
                if (!eventStages.TryGetValue(eventStage.Value, out STAGE_TABLE stage_TABLE))
                    badRank.Add(eventStage.Key);
                else if (stage_TABLE.n_RANK > exp_TABLE.n_ID)
                    badRank.Add(eventStage.Key);
            }
        }
        foreach (var eventStage in badRank)
            StandaloneConsoleStorage.ClientInGameData.DicEventStageDifficulties[eventStage] = 1;

        foreach (var eventStage in StandaloneConsoleStorage.ClientInGameData.DicEventOpenedByTool)
        {
            if (!OrangeDataManager.Instance.EVENT_TABLE_DICT.TryGetValue(eventStage.Key, out EVENT_TABLE event_TABLE))
                eventStage_mods.Add(eventStage.Key, eventStage.Value);
        }
    }
}