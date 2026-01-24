using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    public class PlayerCardCollection : MonoBehaviour
    {
        // store all persistent player cards
        [SerializeField] private List<CardAbilityDefinition> _ownedCards = new();
        public List<CardAbilityDefinition> GetOwnedCards => _ownedCards;

        public static PlayerCardCollection instance;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        public void Add(CardAbilityDefinition def)
        {
            if (def != null)
                _ownedCards.Add(def);
        }
        public void LoadGameData(List<CardAbilityDefinition> ownedCards)
        {
            _ownedCards = ownedCards;
        }
    }
}