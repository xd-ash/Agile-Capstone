using CardSystem;
using UnityEngine;

[System.Serializable]
public class Reward
{
    [SerializeField] private int _currencyReward;
    [SerializeField] private Card[] _cardReward;
    [SerializeField] private Card[] _cardReward2;
    [SerializeField] private RewardType _rewardType;

    public int GetCurrencyReward => _currencyReward;
    public Card[] GetCardReward1 => _cardReward;
    public Card[] GetCardReward2 => _cardReward2;
    public RewardType GetRewardType => _rewardType;

    public Reward(RewardType rewardType, int currencyReward, Card[] cardReward, Card[] cardReward2)
    {
        _rewardType = rewardType;

        _currencyReward = currencyReward;
        _currencyReward = Mathf.Clamp(_currencyReward, 0, RewardsController.GetMaxCurrencyReward);

        _cardReward = cardReward ?? new Card[0];
        _cardReward2 = cardReward2 ?? new Card[0];
    }
}
