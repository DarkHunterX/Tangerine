using System;
using System.Collections.Generic;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class GachaInfo
{
    /*
    _dicBoxGachaPlayerStatus = Dictionary<int, NetBoxGachaStatus>
        NetBoxGachaStatus = int EventID, int CurrentBoxGachaID, int CycleCounts

    _listBoxGachaRecord = List<NetBoxGachaRecord>
        NetBoxGachaRecord = int EventID, int BoxGachaID, int GachaID, int Count
    
    _dicGachaRecord = Dictionary<int, NetGachaRecordDB>
        NetGachaRecordDB = int EventID, int Count, int TotalCount, int Group, int SetupDrawCount, int LastDrawTime

    _dicGachaGuaranteeRecord = Dictionary<int, NetGachaGuaranteeRecordDB>
        NetGachaGuaranteeRecordDB = int GroupID, int GuaranteeValue, int ExchangeTimes, int ResetTimes, int TotalDraws
    */

    internal static void Clear()
    {
        
    }

    internal static bool HasData()
    {      
        return false;
    }

    internal static void Remove()
    {
        
    }

    internal static void Restore()
    {
        
    }

    internal static void ValidateGachaInfo()
    {
        
    }
}