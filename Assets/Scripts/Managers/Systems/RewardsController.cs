using CardSystem;
using UnityEngine;
using System.Collections.Generic;

public enum RewardType
{
    Currency,
    CardAndCurrency,
    BadgeAndCurrency,
    All
}
public static class RewardsController
{
    private static int _maxCurrencyReward = 150;
    private static int _maxCardRewardPool = 5;
    private static int _maxBadgeReward = 3;
    private static int _minCurrencyReward = 10;
    private static int _minCardRewardPool = 2;
    //private static int _minBadgeReward = 1;

    public static int GetMaxCurrencyReward => _maxCurrencyReward;
    public static int GetMaxCardRewardPool => _maxCardRewardPool;
    public static int GetMaxBadgeReward => _maxBadgeReward;

    public static void RewardChips(int amount)
    {
        if (PlayerDataManager.Instance == null) return;

        PlayerDataManager.Instance.AddChips(amount);
    }
    public static void RewardCash(int amount)
    {
        if (PlayerDataManager.Instance == null) return;

        Debug.Log("Cash Rewarded");
    }
    public static void RewardCard(CardAbilityDefinition card)
    {
        if (DeckAndHandManager.Instance == null) return;

        DeckAndHandManager.Instance?.AddDefinitionToRuntimeDeck(card);
    }
    public static void RewardBadge(BadgeSO badge)
    {
        if (PlayerDataManager.Instance == null) return;

        Debug.Log("Badge Rewarded");
    }

    //Randomly determine reward types
    private static RewardType DetermineRewardTypes(int randomSeed)
    {
        Random.InitState(randomSeed);
        int result = Random.Range(0, 100);

        switch (result)
        {
            case < 10:
                return RewardType.Currency;
            case < 70:
                return RewardType.CardAndCurrency;
            case < 90:
                return RewardType.BadgeAndCurrency;
            case < 100:
                return RewardType.All;
            default:
                return RewardType.Currency;
        }
    }
    public static Reward DetermineRewards(Vector2Int nodeIndex)
    {
        var pdm = PlayerDataManager.Instance;
        if (pdm == null) return null;

        int randomSeed = pdm.GetNodeMapSeed + int.Parse($"{nodeIndex.x}{nodeIndex.y}");

        int totalNodeTiers = NodeMapCreator.Instance.GetNumberOfTiers;
        float mapCompleteRatio = (float)nodeIndex.x / (float)totalNodeTiers;
        mapCompleteRatio =  Mathf.Clamp(mapCompleteRatio, 0f, 1f);

        var rewardTypes = DetermineRewardTypes(randomSeed);

        int currencyReward = GetCurrencyReward(mapCompleteRatio, randomSeed);
        CardAbilityDefinition[] cardRewardPool = null;
        BadgeSO[] badgeRewardPool = null;

        if (rewardTypes == RewardType.CardAndCurrency || rewardTypes == RewardType.All)
            cardRewardPool = GetRewardPoolCards(mapCompleteRatio, randomSeed);

        if (rewardTypes == RewardType.BadgeAndCurrency || rewardTypes == RewardType.All)
            badgeRewardPool = GetRewardPoolBadges(mapCompleteRatio, randomSeed);

        return new Reward(rewardTypes, currencyReward, cardRewardPool, badgeRewardPool);
    }
    private static int GetCurrencyReward(float mapCompleteRatio, int randomSeed)
    {
        Random.InitState(randomSeed);

        int maxCurrencyReward = (int)(_maxCurrencyReward * mapCompleteRatio);
        maxCurrencyReward = Mathf.Clamp(maxCurrencyReward, _minCurrencyReward, _maxCurrencyReward);

        return Random.Range(1, maxCurrencyReward);
    }
    private static CardAbilityDefinition[] GetRewardPoolCards(float mapCompleteRatio, int randomSeed)
    {
        var cardLibrary = Resources.Load<CardAndDeckLibrary>("Libraries/CardAndDeckLibrary");
        if (cardLibrary == null) return null;

        int rewardPoolSize = (int)(_maxCardRewardPool * mapCompleteRatio);
        rewardPoolSize = Mathf.Clamp(rewardPoolSize, _minCardRewardPool, _maxCardRewardPool);

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
    private static BadgeSO[] GetRewardPoolBadges(float mapCompleteRatio, int randomSeed)
    {
        //Debug.LogError("Badge SO reward method not implemented");
        return null;
    }
}
