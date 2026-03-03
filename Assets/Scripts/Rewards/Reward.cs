using CardSystem;
using UnityEngine;

[System.Serializable]
public class Reward
{
    [SerializeField] private int _currencyReward;
    [SerializeField] private CardAbilityDefinition[] _cardReward;
    [SerializeField] private BadgeSO[] _badgeReward;
    [SerializeField] private RewardType _rewardType;

    public int GetCurrencyReward => _currencyReward;
    public CardAbilityDefinition[] GetCardReward => _cardReward;
    public BadgeSO[] GetBadgeReward => _badgeReward;
    public RewardType RewardType => _rewardType;

    public Reward(RewardType rewardType, int currencyReward, CardAbilityDefinition[] cardReward, BadgeSO[] badgeReward)
    {
        _rewardType = rewardType;

        _currencyReward = currencyReward;
        _currencyReward = Mathf.Clamp(_currencyReward, 0, RewardsController.GetMaxCurrencyReward);

        _cardReward = cardReward ?? new CardAbilityDefinition[0];
        _badgeReward = badgeReward ?? new BadgeSO[0];
    }
}
