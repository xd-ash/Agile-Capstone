using UnityEngine;
using XNode;

namespace CardSystem
{
    [CreateAssetMenu(fileName = "CardSO", menuName = "Deckbuilding System/New Card SO")]
    public class CardSO : ScriptableObject
    {
        [SerializeField] private string _cardName;
        [TextArea(1, 3)]
        [SerializeField] private string _description;
        [SerializeField] private int _apCost;
        [SerializeField] private AbilityDefinition _cardAbility;
        [SerializeField] private GameObject _cardPrefab; //break into smaller art assets to
                                                         //add into a general card prefab?

        public string GetCardName { get { return _cardName; } }
        public string GetDescription { get { return _description; } }
        public int GetAPCost { get { return _apCost; } }
        public AbilityDefinition GetCardAbility { get { return _cardAbility; } }
        public GameObject GetCardPrefab { get { return _cardPrefab; } }
    }
}
