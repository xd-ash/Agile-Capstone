using UnityEngine;

namespace CardSystem {
    [CreateAssetMenu(fileName = "Deck", menuName = "Deckbuilding System/New Deck")]
    public class Deck : ScriptableObject
    {
        [SerializeField] private AbilityDefinition[] deck; 

        //sort function?
    }
}