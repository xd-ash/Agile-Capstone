using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [Tooltip("Starting currency for new players")]
    [SerializeField] private int _startingBalance = 100;
    [Tooltip("Max currency (0 = unlimited)")]
    [SerializeField] private int _maxBalance = 0;

    // Fired when balance changes; argument is new balance
    public event Action<int> OnBalanceChanged;

    public static CurrencyManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        PlayerDataManager.Instance.UpdateCurrencyData(_startingBalance);//potentially needs changed if multiple shops
    }

    // Try to spend amount. Returns true if successful and deducts amount.
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        int balance = PlayerDataManager.Instance.GetBalance;

        if (balance >= amount)
        {
            balance -= amount;
            balance = ClampAndReturn(balance);
            UpdateCurrency(balance);
            return true;
        }
        return false;
    }
    // Add currency (can be used for rewards, refunds, admin)
    public void Add(int amount)
    {
        if (amount <= 0) return;
        int balance = PlayerDataManager.Instance.GetBalance;

        balance += amount;
        balance = ClampAndReturn(balance);
        UpdateCurrency(balance);
    }

    // Set absolute balance (useful for debug)
    public void SetBalance(int value)
    {
        int balance = ClampAndReturn(value);
        UpdateCurrency(balance);
    }
    // Update player data & ui
    public void UpdateCurrency(int balance)
    {
        PlayerDataManager.Instance.UpdateCurrencyData(balance);
        OnBalanceChanged?.Invoke(balance);
    }
    private int ClampAndReturn(int balance)
    {
        if (_maxBalance > 0) balance = Mathf.Min(balance, _maxBalance);
        if (balance < 0) balance = 0;
        return balance;
    }
}