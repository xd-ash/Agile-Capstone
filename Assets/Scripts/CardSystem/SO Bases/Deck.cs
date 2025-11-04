using UnityEngine;

namespace CardSystem {
    [CreateAssetMenu(fileName = "Deck", menuName = "Deckbuilding System/New Deck")]
    public class Deck : ScriptableObject
    {
        [SerializeField] private CardAbilityDefinition[] _deck;

        public CardAbilityDefinition[] GetDeck { get => _deck; }

        //sort function?
    }
}