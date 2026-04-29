using CardSystem;
using System.Collections.Generic;
using UnityEngine;

public enum RewardType
{
    Currency,
    NewCard,
    SwapCard,
}

public static class RewardsController
{
    private static int _maxCurrencyReward = 150;
    private static int _maxCardRewardPool = 3;
    private static int _maxBadgeReward = 3;
    private static int _minCurrencyReward = 10;
    private static int _minCardRewardPool = 2;

    public static int GetMaxCurrencyReward => _maxCurrencyReward;
    public static int GetMaxCardRewardPool => _maxCardRewardPool;
    public static int GetMaxBadgeReward => _maxBadgeReward;

    public static void RewardChips(int amount)
    {
        if (PlayerDataManager.Instance == null) return;

        PlayerDataManager.Instance.AddChips(amount);
    }
    public static void RewardCard(Card card)
    {
        if (DeckAndHandManager.Instance == null) return;

        DeckAndHandManager.Instance?.AddCardToRuntimeDeck(card);
    }

    //Randomly determine reward types
    private static RewardType DetermineRewardTypes(int randomSeed)
    {
        Random.InitState(randomSeed);
        int result = Random.Range(0, 100);

        switch (result)
        {
            case < 40:
                return RewardType.NewCard;
            case < 80:
                return RewardType.SwapCard;
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
        Card[] cardRewardPool = null;

        if (rewardTypes != RewardType.Currency)
            cardRewardPool = GetRewardPoolCards(mapCompleteRatio, randomSeed);

        return new Reward(rewardTypes, currencyReward, cardRewardPool);
    }
    private static int GetCurrencyReward(float mapCompleteRatio, int randomSeed)
    {
        Random.InitState(randomSeed);

        int maxCurrencyReward = (int)(_maxCurrencyReward * mapCompleteRatio);
        maxCurrencyReward = Mathf.Clamp(maxCurrencyReward, _minCurrencyReward, _maxCurrencyReward);

        return Random.Range(1, maxCurrencyReward);
    }
    private static Card[] GetRewardPoolCards(float mapCompleteRatio, int randomSeed)
    {
        var cardLibrary = Resources.Load<CardAndPackLibrary>("Libraries/CardAndPackLibrary");
        if (cardLibrary == null) return null;

        int rewardPoolSize = (int)(_maxCardRewardPool * mapCompleteRatio);
        rewardPoolSize = Mathf.Clamp(rewardPoolSize, _minCardRewardPool, _maxCardRewardPool);

        List<CardAbilityDefinition> temp = new();
        List<Card> cards = new();
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
            } while (randCard == null || temp.Contains(randCard));

            var rarity = RollRarity(randomSeed + i + c);
            temp.Add(randCard);
            cards.Add(new(randCard, rarity));
        }

        return cards.ToArray();
    }
    private static CardRarity RollRarity(int randomSeed)
    {
        Random.InitState(randomSeed);
        int randIndex = Random.Range(0, 100);

        switch (randIndex)
        {
            case < 65:
                return CardRarity.Common;
            case < 90:
                return CardRarity.Rare;
            case < 100:
            default:
                return CardRarity.Epic;
        }
    }
}
