using UnityEngine;

namespace CardSystem {
    [CreateAssetMenu(fileName = "Deck", menuName = "Deckbuilding System/New Deck")]
    public class Deck : ScriptableObject
    {
        [SerializeField] private CardSO[] _deck;

        public CardSO[] GetDeck { get => _deck; }

        //sort function?
    }
}