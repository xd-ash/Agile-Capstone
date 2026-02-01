using CardSystem;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveLoadScript
{
    public static Action SaveGame => () => SaveGameData();
    public static Action CreateNewGame => () => SaveGameData(true);
    public static Action LoadGame => () => LoadGameData();

    public static Action SaveSettings => () => SaveSettingsData();
    public static Action LoadSettings => () => LoadSettingsData();

    private static string _gameDataFilePath = Application.persistentDataPath + "-GameSave.json";
    private static string _settingsDataFilePath = Application.persistentDataPath + "-SettingsData.json";
    public static bool CheckForSaveGame => File.Exists(_gameDataFilePath);

    // Save/laod for general game data
    private static void SaveGameData(bool isNewGame = false)
    {
        string json = JsonUtility.ToJson(new GameData(isNewGame), true); // turn off prettyPrint once encryption is implemented?
        StreamWriter sw = new StreamWriter(_gameDataFilePath);
        sw.Write(json);
        sw.Close();
        //Debug.Log("Game Saved");
    }
    private static void LoadGameData()
    {
        string json = string.Empty;
        StreamReader sr = new StreamReader(_gameDataFilePath);
        json = sr.ReadToEnd();

        GameData _gameData = JsonUtility.FromJson<GameData>(json);
        PlayerDataManager.Instance?.OnGameLoad(_gameData);
        sr.Close();
    }

    // Save/load for settings such as audio
    private static void SaveSettingsData()
    {
        string json = JsonUtility.ToJson(new SettingsData(), true);
        StreamWriter sw = new StreamWriter(_settingsDataFilePath);
        sw.Write(json);
        sw.Close();
    }
    private static void LoadSettingsData()
    {
        // Create new settings file if no file exists
        if (!File.Exists(_settingsDataFilePath)) SaveSettingsData();

        string json = string.Empty;
        StreamReader sr = new StreamReader(_settingsDataFilePath);
        json = sr.ReadToEnd();

        SettingsData settingsData = JsonUtility.FromJson<SettingsData>(json);
        AudioManager.instance.LoadVolumeSettings(settingsData.GetAudioSettings);
        sr.Close();
    }
}

[System.Serializable]
public class GameData
{
    [SerializeField] private MapNodeDataToken _mapNodeData;
    [SerializeField] private CurrencyManagerDataToken _currencyData;
    [SerializeField] private CardDataToken _cardData;

    public MapNodeDataToken GetMapNodeData => _mapNodeData;
    public CurrencyManagerDataToken GetCurrencyData => _currencyData;
    public CardDataToken GetCardData => _cardData;

    public GameData(bool newGameData = false)
    {
        var pdm = PlayerDataManager.Instance;

        if (newGameData)
        {
            _mapNodeData = new(null, null, 0, -1);
            _currencyData = new(0);
            _cardData = new(null, pdm.GetDeck);
        }
        else
        {
            _mapNodeData = new(pdm.GetNodeCompleted, pdm.GetNodeUnlocked, pdm.GetCurrentNodeIndex, pdm.GetSeed);
            _currencyData = new(pdm.GetBalance);
            _cardData = new(pdm.GetOwnedCards, pdm.GetDeck);
        }
    }

    // node map vars
    [System.Serializable]
    public class MapNodeDataToken
    {
        [SerializeField] private bool[] _nodesCompleted;
        [SerializeField] private bool[] _nodesUnlocked;
        [SerializeField] private int _currentNodeIndex;
        [SerializeField] private int _randomSeed;

        public bool[] GetNodesCompleted => _nodesCompleted;
        public bool[] GetNodesUnlocked => _nodesUnlocked;
        public int GetCurrentNodeIndex => _currentNodeIndex;
        public int GetSeed => _randomSeed;

        public MapNodeDataToken(bool[] nodesCompleted, bool[] nodesUnlocked, int currentNodeIndex, int seed)
        {
            _nodesCompleted = nodesCompleted;
            _nodesUnlocked = nodesUnlocked;
            _currentNodeIndex = currentNodeIndex;
            _randomSeed = seed;
        }
    }

    // currency
    [System.Serializable]
    public class CurrencyManagerDataToken
    {
        [SerializeField] private int _balance;
        public int GetBalance => _balance;

        public CurrencyManagerDataToken(int balance)
        {
            _balance = balance;
        }
    }

    // deck and card info
    [System.Serializable]
    public class CardDataToken
    {
        [SerializeField] private string[] _ownedCardNames;
        [SerializeField] private string _deckName;

        public string[] GetOwnedCardNames => _ownedCardNames;
        public string GetDeckName => _deckName;

        public CardDataToken(List<CardAbilityDefinition> ownedCards, Deck deck)
        {
            if (ownedCards != null)
            {
                _ownedCardNames = new string[ownedCards.Count];
                for (int i = 0; i < ownedCards.Count; i++)
                    _ownedCardNames[i] = ownedCards[i].name;
            }
            else
                _ownedCardNames = new string[0];

            _deckName = deck.name;
        }
    }
}

[System.Serializable]
public class SettingsData
{
    [SerializeField] private AudioSettingsToken _audioSettings;

    public AudioSettingsToken GetAudioSettings => _audioSettings;

    [System.Serializable]
    public class AudioSettingsToken
    {
        [SerializeField] private float _masterVolume;
        [SerializeField] private float _sfxVolume;
        [SerializeField] private float _musicVolume;

        public float GetMasterVolume => _masterVolume;
        public float GetSFXVolume => _sfxVolume;
        public float GetMusicVolume => _musicVolume;

        public AudioSettingsToken()
        {
            _masterVolume = Mathf.Min(AudioManager.instance.GetMasterVolume, 1);
            _sfxVolume = Mathf.Min(AudioManager.instance.GetSFXVolume, 1);
            _musicVolume = Mathf.Min(AudioManager.instance.GetMusicVolume, 1);
        }
    }
}
