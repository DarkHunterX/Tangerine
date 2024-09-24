using System;
using System.Collections.Generic;
using System.Linq;
using OrangeConsoleService;

namespace TangerineBaseMods.Patches;

internal static class GalleryInfo
{
    private static List<NetGalleryInfo> gallery_mods = new();
    private static List<NetGalleryMainIdInfoSP> galleryCard_mods = new();

    private static Dictionary<string, NetGalleryMainIdInfoSP> _listGalleryMainId = new();

    private static int cacheExp;
    private static EXP_TABLE cacheExpTable;
    private static Dictionary<int, EXP_TABLE> _dictGalleryExp = new();

    internal static void Clear()
    {
        gallery_mods.Clear();
        galleryCard_mods.Clear();
    }

    internal static bool HasData()
    {
        if (gallery_mods.Count > 0)
            return true;
        if (galleryCard_mods.Count > 0)
            return true;
        return false;
    }

    private static void CardGalleryToDict()
    {
        _listGalleryMainId.Clear();
        foreach (var galleryCard in GalleryService.Instance._listGalleryMainId)
            _listGalleryMainId.Add($"{galleryCard.GalleryId}_{galleryCard.GalleryMainId}", galleryCard);
    }

    private static void CardGalleryToList()
    {
        GalleryService.Instance._listGalleryMainId.Clear();
        foreach (var galleryCard in _listGalleryMainId)
            GalleryService.Instance._listGalleryMainId.Add(galleryCard.Value);
    }

    internal static void Remove()
    {
        foreach (var gallery in gallery_mods)
            GalleryService.Instance._dicGallery.Remove(gallery.GalleryID);

        CardGalleryToDict();
        foreach (var galleryCard in galleryCard_mods)
            _listGalleryMainId.Remove($"{galleryCard.GalleryId}_{galleryCard.GalleryMainId}");
        CardGalleryToList();
    }

    internal static void Restore()
    {
        foreach (var gallery in gallery_mods)
            GalleryService.Instance._dicGallery.Add(gallery.GalleryID, gallery);
        foreach (var galleryCard in galleryCard_mods)
            GalleryService.Instance._listGalleryMainId.Add(galleryCard);
    }

    internal static void ValidateGalleryInfo()
    {
        var characterExp = 0;
        var weaponExp = 0;
        var cardExp = 0;

        foreach (var gallery in GalleryService.Instance._dicGallery)
        {
            if (!OrangeDataManager.Instance.GALLERY_TABLE_DICT.TryGetValue(gallery.Value.GalleryID, out GALLERY_TABLE gallery_TABLE))
                gallery_mods.Add(gallery.Value);
            else
            {
                if (gallery_TABLE.n_TYPE == 1)
                    characterExp += gallery_TABLE.n_EXP;
                else if (gallery_TABLE.n_TYPE == 2)
                    weaponExp += gallery_TABLE.n_EXP;
            }
        }

        foreach (var galleryCard in GalleryService.Instance._listGalleryMainId)
        {
            if (!OrangeDataManager.Instance.GALLERY_TABLE_DICT.TryGetValue(galleryCard.GalleryId, out GALLERY_TABLE gallery_TABLE))
                galleryCard_mods.Add(galleryCard);
            else if (gallery_TABLE.n_TYPE == 3)
                cardExp += gallery_TABLE.n_EXP;
        }

        foreach (var galleryExp in GalleryService.Instance._dicGalleryExp)
        {
            EXP_TABLE exp_TABLE;
            switch (galleryExp.Value.GalleryType)
            {
                case 1:
                    exp_TABLE = GetExpTable(characterExp);
                    if (galleryExp.Value.Exp >= exp_TABLE.n_TOTAL_DISCEXP)
                        galleryExp.Value.Exp = exp_TABLE.n_TOTAL_DISCEXP - 1;
                    break;
                case 2:
                    exp_TABLE = GetExpTable(weaponExp);
                    if (galleryExp.Value.Exp >= exp_TABLE.n_TOTAL_DISCEXP)
                        galleryExp.Value.Exp = exp_TABLE.n_TOTAL_DISCEXP - 1;
                    break;
                case 3:
                    exp_TABLE = GetExpTable(cardExp);
                    if (galleryExp.Value.Exp >= exp_TABLE.n_TOTAL_DISCEXP)
                        galleryExp.Value.Exp = exp_TABLE.n_TOTAL_DISCEXP - 1;
                    break;
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
            if (!_dictGalleryExp.TryGetValue(exp_TABLE.Key, out EXP_TABLE exp_TBL))
                _dictGalleryExp.Add(exp_TABLE.Key, exp_TABLE.Value);

            if (nExp < exp_TABLE.Value.n_TOTAL_GALLERYEXP)
            {
                cacheExpTable = exp_TABLE.Value;
                int num = exp_TABLE.Value.n_TOTAL_GALLERYEXP - nExp;
                if (num <= exp_TABLE.Value.n_GALLERYEXP)
                    break;
            }
        }

        if (cacheExpTable == null)
        {
            if (nExp > 0)
                cacheExpTable = _dictGalleryExp.Last().Value;
            else
                cacheExpTable = _dictGalleryExp.First().Value;
        }

        cacheExp = nExp;
        return cacheExpTable;
    }
}