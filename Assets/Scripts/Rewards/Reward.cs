using CardSystem;
using UnityEngine;

[System.Serializable]
public class Reward
{
    [SerializeField] private int _currencyReward;
    [SerializeField] private Card[] _cardReward;
    [SerializeField] private RewardType _rewardType;

    public int GetCurrencyReward => _currencyReward;
    public Card[] GetCardReward => _cardReward;
    public RewardType GetRewardType => _rewardType;

    public Reward(RewardType rewardType, int currencyReward, Card[] cardReward/*, BadgeSO[] badgeReward*/)
    {
        _rewardType = rewardType;

        _currencyReward = currencyReward;
        _currencyReward = Mathf.Clamp(_currencyReward, 0, RewardsController.GetMaxCurrencyReward);

        _cardReward = cardReward ?? new Card[0];
    }
}
