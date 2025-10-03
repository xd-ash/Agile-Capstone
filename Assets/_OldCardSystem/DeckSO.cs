using CardSystem;
using Unity.VisualScripting;
using UnityEngine;

namespace OldCardSystem
{
    //[CreateAssetMenu(fileName = "DeckSO", menuName = "Deckbuilding System/New Deck")]
    public class DeckSO : ScriptableObject
    {
        public CardSO[] _deck;
    }
}
