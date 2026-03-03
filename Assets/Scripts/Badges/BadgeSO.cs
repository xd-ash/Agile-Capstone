using CardSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "BadgeSO", menuName = "Rewards/New Badge")]
public class BadgeSO : ScriptableObject
{
    [SerializeField] private string _description;

    //rarity enum?
    [SerializeField] private CardAbilityDefinition _badgeAbility;

    public string GetDescription => _description;
    public CardAbilityDefinition GetBadgeAbility => _badgeAbility;
}
