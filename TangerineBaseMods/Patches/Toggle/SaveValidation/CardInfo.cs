using System;
using System.Collections.Generic;
using System.Linq;
using OrangeConsoleService;
using static Il2CppSystem.Runtime.Remoting.RemotingServices;

namespace TangerineBaseMods.Patches;

internal static class CardInfo
{
    private static List<NetCardInfo> card_mods = new();
    private static List<NetCharacterCardSlotInfo> characterCard_mods = new();

    private static Dictionary<string, NetCharacterCardSlotInfo> _listCharacterCardSlot = new();

    private static int cacheExp;
    private static EXP_TABLE cacheExpTable;
    private static Dictionary<int, EXP_TABLE> _dictCardExp = new();

    internal static void Clear()
    {
        card_mods.Clear();
    }

    internal static bool HasData()
    {
        if (card_mods.Count > 0)
            return true;
        return false;
    }

    private static void CharacterCardSlotToDict()
    {
        _listCharacterCardSlot.Clear();
        foreach (var card in CardService.Instance._listCharacterCardSlot)
            _listCharacterCardSlot.Add($"{card.CharacterID}_{card.CharacterCardSlot}", card);
    }

    private static void CharacterCardSlotToList()
    {
        CardService.Instance._listCharacterCardSlot.Clear();
        foreach (var card in _listCharacterCardSlot)
            CardService.Instance._listCharacterCardSlot.Add(card.Value);
    }

    internal static void Remove()
    {
        foreach (var card in card_mods)
            CardService.Instance._dicCard.Remove(card.CardSeqID);

        CharacterCardSlotToDict();
        foreach (var card in characterCard_mods)
            _listCharacterCardSlot.Remove($"{card.CharacterID}_{card.CharacterCardSlot}");
        CharacterCardSlotToList();
    }

    internal static void Restore()
    {
        foreach (var card in card_mods)
            CardService.Instance._dicCard.Add(card.CardSeqID, card);
        foreach (var card in characterCard_mods)
            CardService.Instance._listCharacterCardSlot.Add(card);
    }

    internal static void ValidateCardInfo()
    {
        foreach (var card in CardService.Instance._dicCard)
        {
            if (card.Key > CardService.Instance._seqId)
                CardService.Instance._seqId = card.Key;
            
            if (!OrangeDataManager.Instance.CARD_TABLE_DICT.TryGetValue(card.Value.CardID, out CARD_TABLE card_TABLE))
                card_mods.Add(card.Value);
            else
            {
                var exp_TABLE = GetExpTable(card.Value.Exp);
                if (card.Value.Exp >= exp_TABLE.n_TOTAL_CARDEXP)
                    card.Value.Exp = exp_TABLE.n_TOTAL_CARDEXP - 1;
            }
        }

        foreach (var card in CardService.Instance._listCharacterCardSlot)
        {
            if (!OrangeDataManager.Instance.CHARACTER_TABLE_DICT.TryGetValue(card.CharacterID, out CHARACTER_TABLE character_TABLE))
                characterCard_mods.Add(card);
            else if (!CardService.Instance._dicCard.TryGetValue(card.CardSeqID, out NetCardInfo cardInfo))
                card.CardSeqID = 0;  // none
            else if (!OrangeDataManager.Instance.CARD_TABLE_DICT.TryGetValue(cardInfo.CardID, out CARD_TABLE card_TABLE))
                card.CardSeqID = 0;  // none
        }

        foreach (var card in CardService.Instance._listDeployCardSlot)
        {
            if (!CardService.Instance._dicCard.TryGetValue(card.CardSeqID, out NetCardInfo cardInfo))
                card.CardSeqID = 0;  // none
            else if (!OrangeDataManager.Instance.CARD_TABLE_DICT.TryGetValue(cardInfo.CardID, out CARD_TABLE card_TABLE))
                card.CardSeqID = 0;  // none
        }
    }

    private static EXP_TABLE GetExpTable(int nExp)
    {
        if (cacheExp > 0 && cacheExp == nExp)
            return cacheExpTable;

        cacheExpTable = null;
        foreach (var exp_TABLE in OrangeDataManager.Instance.EXP_TABLE_DICT)
        {
            if (!_dictCardExp.TryGetValue(exp_TABLE.Key, out EXP_TABLE exp_TBL))
                _dictCardExp.Add(exp_TABLE.Key, exp_TABLE.Value);
            
            if (nExp < exp_TABLE.Value.n_TOTAL_CARDEXP)
            {
                cacheExpTable = exp_TABLE.Value;
                int num = exp_TABLE.Value.n_TOTAL_CARDEXP - nExp;
                if (num <= exp_TABLE.Value.n_CARDEXP)
                    break;
            }
        }

        if (cacheExpTable == null)
        {
            if (nExp > 0)
                cacheExpTable = _dictCardExp.Last().Value;
            else
                cacheExpTable = _dictCardExp.First().Value;
        }

        cacheExp = nExp;
        return cacheExpTable;
    }
}