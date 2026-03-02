using CardSystem;
using UnityEngine;

public class Reward
{
    private int _currencyReward;
    private CardAbilityDefinition[] _cardReward;
    private BadgeSO[] _badgeReward;

    public int GetCurrencyReward => _currencyReward;
    public CardAbilityDefinition[] GetCardReward => _cardReward;
    public BadgeSO[] GetBadgeReward => _badgeReward;

    public Reward(int currencyReward, CardAbilityDefinition[] cardReward, BadgeSO[] badgeReward)
    {
        _currencyReward = currencyReward;
        _currencyReward = Mathf.Clamp(_currencyReward, 0, RewardsController.GetMaxCurrencyReward);

        _cardReward = cardReward ?? new CardAbilityDefinition[0];
        _badgeReward = badgeReward ?? new BadgeSO[0];
    }
}
