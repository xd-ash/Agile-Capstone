using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    public class PlayerCardCollection : MonoBehaviour
    {
        public static PlayerCardCollection instance;

        // store all persistent player cards
        public List<CardAbilityDefinition> OwnedCards { get; private set; } = new();

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
                OwnedCards.Add(def);
        }
    }
}