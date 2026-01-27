using CardSystem;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    private PlayerData _playerData;
    public PlayerData GetPlayerData => _playerData;

    public static PlayerDataManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _playerData = new PlayerData();
    }
    public void UpdateNodeData(bool[] nodesCompleted, bool[] nodesUnlocked)
    {

    }
    public void UpdateCurrencyData(int currentCurrency)
    {

    }
    public void UpdateCardData(List<CardAbilityDefinition> ownedCards, Deck deck)
    {

    }
}
public struct PlayerData
{
    public int _currency;

    public bool[] _nodeCompleted;
    public bool[] _nodeUnlocked;

    public string[] _ownedCardNames;
    public string _deckName;
}
