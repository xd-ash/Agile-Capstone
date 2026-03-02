using CardSystem;
using UnityEngine;
using System.Collections.Generic;

public static class RewardsController
{
    private static int _maxCurrencyReward = 150;
    private static int _maxCardRewardPool = 5;
    private static int _maxBadgeReward = 2;

    public static int GetMaxCurrencyReward => _maxCurrencyReward;
    public static int GetMaxCardRewardPool => _maxCardRewardPool;
    public static int GetMaxBadgeReward => _maxBadgeReward;

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

    public static Reward DetermineRewards(Vector2Int nodeIndex)
    {
        var pdm = PlayerDataManager.Instance;
        if (pdm == null) return null;

        int randomSeed = pdm.GetNodeMapSeed + int.Parse($"{nodeIndex.x}{nodeIndex.y}");
    }
    private static CardAbilityDefinition[] GetRewardPoolCards(int rewardPoolSize, int randomSeed)
    {
        var cardLibrary = Resources.Load<CardAndDeckLibrary>("Libraries/CardAndDeckLibrary");
        if (cardLibrary == null) return null;

        List<CardAbilityDefinition> cards = new();

        for (int i = 0; i < rewardPoolSize; i++)
        {
            CardAbilityDefinition randCard = null;
            int c = 0;
            do
            {
                Random.InitState(randomSeed + i + c);
                int randIndex = Random.Range(0, cardLibrary.GetCardsInProject.Count);
                randCard = cardLibrary.GetCardsInProject[randIndex];
                c++;
            } while (randCard == null || cards.Contains(randCard));

            cards.Add(randCard);
        }

        return cards.ToArray();
    }
    private static BadgeSO[] GetRewardPoolBadges(int rewardPoolSize, int randomSeed)
    {

    }
}
