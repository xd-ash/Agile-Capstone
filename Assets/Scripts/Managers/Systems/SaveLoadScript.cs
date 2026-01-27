using CardSystem;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SaveLoadScript
{
    public static Action SaveGame => () => SaveData();
    public static Action LoadGame => () => LoadData();

    private static void SaveData()
    {
        string json = JsonUtility.ToJson(new GameData());
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + "DC_GameSave.json");
        sw.Write(json);
        sw.Close();
        Debug.Log("Game Saved");
    }

    private static void LoadData()
    {
        string json = string.Empty;
        StreamReader sr = new StreamReader(Application.persistentDataPath + "DC_GameSave.json");
        json = sr.ReadToEnd();

        GameData _gameData = JsonUtility.FromJson<GameData>(json);
        _gameData.LoadGameData();
        sr.Close();
        Debug.Log("Game Loaded");
    }
}

[System.Serializable]
public class GameData
{
    private MapNodeDataToken mapNodeData;
    private CurrencyManagerDataToken currencyData;
    private DeckDataToken deckData;

    public GameData()
    {
        mapNodeData = new();
        currencyData = new();
        deckData = new();
    }

    public void LoadGameData()
    {
        mapNodeData.LoadData();
        currencyData.LoadData();
        deckData.LoadData();
    }

    // node map vars
    [System.Serializable]
    public class MapNodeDataToken
    {
        private bool[] _nodeCompleted;
        private bool[] _nodeUnlocked;
        private int _currentNodeIndex;

        public MapNodeDataToken()
        {
            SceneProgressManager.Instance.GrabNodeData( ref _nodeCompleted, 
                ref _nodeUnlocked, ref _currentNodeIndex);
        }
        public void LoadData()
        {
            SceneProgressManager.Instance.LoadNodeData(_nodeCompleted, 
                _nodeUnlocked, _currentNodeIndex);
        }
    }

    // currency
    [System.Serializable]
    public class CurrencyManagerDataToken
    {
        private int _balance;

        public CurrencyManagerDataToken()
        {
            if (CurrencyManager.instance != null)
                _balance = CurrencyManager.instance.Balance;
        }
        public void LoadData()
        {
            CurrencyManager.instance?.LoadGameData(_balance);
        }
    }

    // deck and card info
    [System.Serializable]
    public class DeckDataToken
    {
        private string[] _ownedCardNames;
        private string _deckName;

        public DeckDataToken()
        {
            var ownedCards = PlayerCardCollection.instance.GetOwnedCards;
            _ownedCardNames = new string[ownedCards.Count];
            for (int i = 0; i < ownedCards.Count; i++)
                _ownedCardNames[i] = ownedCards[i].name;

            _deckName = DeckAndHandManager.instance.GetDeck.name;
        }
        public void LoadData()
        {
            List<CardAbilityDefinition> ownedCards = new();
            foreach (var name in _ownedCardNames)
                ownedCards.Add(GetCardDefinitionFromName(name));
            PlayerCardCollection.instance.LoadGameData(ownedCards);

            DeckAndHandManager.instance.LoadGameData(GetDeckFromName(_deckName));
        }

        //Make these generic at some point & combine
        private CardAbilityDefinition GetCardDefinitionFromName(string cardName)
        {
            string[] cardGUID = AssetDatabase.FindAssets(cardName, new[] { "Assets/ScriptableObjects/CardAbilities" });
            if (cardGUID.Length != 1)
            {
                Debug.LogError($"{cardGUID.Length} Card GUID matches for cardName on data load. ({cardName})");
                return null;
            }
            string cardPath = AssetDatabase.GUIDToAssetPath(cardGUID[0]);
            return AssetDatabase.LoadAssetAtPath<CardAbilityDefinition>(cardPath);
        }
        private Deck GetDeckFromName(string deckName)
        {
            string[] deckGUID = AssetDatabase.FindAssets(deckName, new[] { "Assets/ScriptableObjects/CardAbilities" });
            if (deckGUID.Length != 1)
            {
                Debug.LogError($"{deckGUID.Length} Card GUID matches for cardName on data load. ({deckName})");
                return null;
            }
            string deckPath = AssetDatabase.GUIDToAssetPath(deckGUID[0]);
            return AssetDatabase.LoadAssetAtPath<Deck>(deckPath);
        }
    }
}
