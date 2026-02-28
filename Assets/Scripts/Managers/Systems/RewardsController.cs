using CardSystem;
using UnityEngine;

public static class RewardsController
{
    public static void RewardCard(CardAbilityDefinition card)
    {
        if (PlayerDataManager.Instance == null) return;

        Debug.Log("Card Rewarded");
    }
    public static void RewardChips(int amount)
    {
        if (PlayerDataManager.Instance == null) return;

        Debug.Log("Chips Rewarded");
    }
    public static void RewardCash(int amount)
    {
        if (PlayerDataManager.Instance == null) return;

        Debug.Log("Cash Rewarded");
    }
    public static void RewardBadge(/* insert badge class/so */)
    {
        if (PlayerDataManager.Instance == null) return;

        Debug.Log("Badge Rewarded");
    }

    //generate rewards method for nodes
}
