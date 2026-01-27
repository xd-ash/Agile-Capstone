using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [Tooltip("Starting currency for new players")]
    [SerializeField] private int _startingBalance = 100;
    [Tooltip("Max currency (0 = unlimited)")]
    [SerializeField] private int _maxBalance = 0;

    private const string PREFS_KEY = "PlayerCurrency_Balance";
    private int _balance;

    public int Balance => _balance;

    // Fired when balance changes; argument is new balance
    public event Action<int> OnBalanceChanged;

    public static CurrencyManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        LoadBalance();
    }

    private void LoadBalance()
    {
        _balance = _startingBalance;
        SaveBalance();
    }

    private void SaveBalance()
    {
        PlayerPrefs.SetInt(PREFS_KEY, _balance);
        PlayerPrefs.Save();
    }

    //new save/load stuff
    public void LoadGameData(int balance)
    {
        _balance = balance;
    }

    // Try to spend amount. Returns true if successful and deducts amount.
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;

        if (_balance >= amount)
        {
            _balance -= amount;
            ClampAndSave();
            OnBalanceChanged?.Invoke(_balance);
            return true;
        }
        return false;
    }

    // Add currency (can be used for rewards, refunds, admin)
    public void Add(int amount)
    {
        if (amount <= 0) return;
        _balance += amount;
        ClampAndSave();
        OnBalanceChanged?.Invoke(_balance);
    }

    // Set absolute balance (useful for debug)
    public void SetBalance(int value)
    {
        _balance = value;
        ClampAndSave();
        OnBalanceChanged?.Invoke(_balance);
    }

    private void ClampAndSave()
    {
        if (_maxBalance > 0) _balance = Mathf.Min(_balance, _maxBalance);
        if (_balance < 0) _balance = 0;
        SaveBalance();
    }
}