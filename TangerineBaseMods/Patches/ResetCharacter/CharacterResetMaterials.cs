using Il2CppInterop.Runtime;
using OrangeConsoleService;
using System;
using System.Collections.Generic;

namespace TangerineBaseMods.Patches;
public class CharacterResetMaterials
{
    public Dictionary<int, int> ActiveMaterials = new();
    public Dictionary<int, int> PassiveMaterials = new();
    public Dictionary<int, int> DnaMaterials = new();
    public Dictionary<int, int> SkinMaterials = new();
    public Dictionary<int, int> RankMaterials = new();
    public Dictionary<int, int> SellMaterials = new();

    public void AddMaterials(ref Dictionary<int, int> materialDict, MATERIAL_TABLE material_TABLE)
    {
        var materialId = new int[] {
            material_TABLE.n_MATERIAL_1,
            material_TABLE.n_MATERIAL_2,
            material_TABLE.n_MATERIAL_3,
            material_TABLE.n_MATERIAL_4,
            material_TABLE.n_MATERIAL_5
        };
        var materialCount = new int[] {
            material_TABLE.n_MATERIAL_MOUNT1,
            material_TABLE.n_MATERIAL_MOUNT2,
            material_TABLE.n_MATERIAL_MOUNT3,
            material_TABLE.n_MATERIAL_MOUNT4,
            material_TABLE.n_MATERIAL_MOUNT5
        };
        for (int i = 0; i < materialId.Length; i++)
            AddMaterial(ref materialDict, materialId[i], materialCount[i]);
    }

    public void AddMaterial(ref Dictionary<int, int> materialDict, int materialId, int amount)
    {
        if (materialId != 0)
        {
            if (materialDict.TryGetValue(materialId, out int mat))
                materialDict[materialId] += amount;
            else
                materialDict.Add(materialId, amount);
        }
    }

    public void ComposeRewards(ref NetRewardsEntity netRewardsEntity)
    {
        foreach (var mat in ActiveMaterials)
            AddReward(ref netRewardsEntity, mat.Key, mat.Value);
        foreach (var mat in PassiveMaterials)
            AddReward(ref netRewardsEntity, mat.Key, mat.Value);
        foreach (var mat in DnaMaterials)
            AddReward(ref netRewardsEntity, mat.Key, mat.Value);
        foreach (var mat in SkinMaterials)
            AddReward(ref netRewardsEntity, mat.Key, mat.Value);
        foreach (var mat in RankMaterials)
            AddReward(ref netRewardsEntity, mat.Key, mat.Value);
        foreach (var mat in SellMaterials)
            AddReward(ref netRewardsEntity, mat.Key, mat.Value);
    }

    private void AddReward(ref NetRewardsEntity netRewardsEntity, int rewardId, int amount)
    {
        var item = ItemService.Instance.AddItem(rewardId, amount);
        var itemPredicate = DelegateSupport.ConvertDelegate<Il2CppSystem.Predicate<NetItemInfo>>((NetItemInfo x) => x.ItemID == rewardId);
        var itemIdx = netRewardsEntity.ItemList.FindIndex(itemPredicate);
        if (itemIdx >= 0)
            netRewardsEntity.ItemList[itemIdx].Stack = item.Stack;
        else
            netRewardsEntity.ItemList.Add(item);

        var reward = ItemService.Instance.MakeRewardItem(1, rewardId, amount);
        var rewardPredicate = DelegateSupport.ConvertDelegate<Il2CppSystem.Predicate<NetRewardInfo>>((NetRewardInfo x) => x.RewardID == rewardId);
        var rewardIdx = netRewardsEntity.RewardList.FindIndex(rewardPredicate);
        if (rewardIdx >= 0)
            netRewardsEntity.RewardList[rewardIdx].Amount += reward.Amount;
        else
            netRewardsEntity.RewardList.Add(reward);
    }
}