using System;
using UnityEngine;

namespace CardSystem
{
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager instance;
        public int startingCurrency = 100;
        public int currency;

        public event Action<int> OnCurrencyChanged;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                currency = startingCurrency;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (currency >= amount)
            {
                currency -= amount;
                OnCurrencyChanged?.Invoke(currency);
                return true;
            }
            return false;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            currency += amount;
            OnCurrencyChanged?.Invoke(currency);
        }
    }
}