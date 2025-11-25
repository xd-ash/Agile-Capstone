using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    public class PlayerCollection : MonoBehaviour
    {
        public static PlayerCollection instance;
        public List<CardAbilityDefinition> ownedCards = new List<CardAbilityDefinition>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Add(CardAbilityDefinition def)
        {
            if (def == null) return;
            if (!ownedCards.Contains(def)) ownedCards.Add(def);
        }
    }
}