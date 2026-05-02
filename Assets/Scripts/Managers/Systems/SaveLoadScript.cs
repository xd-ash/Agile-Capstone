using CardSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveLoadScript
{
    public static Action SaveGame => () => SaveGameData();
    public static Action CreateNewGame => () => { SaveGameData(true); LoadGameData(); }; //create new save sata and immidiately load data to apply
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
        //Debug.Log($"{_gameDataFilePath}");
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
        AudioManager.Instance.LoadVolumeSettings(settingsData.GetAudioSettings);
        OptionsSettings.UpdateOptionsData(settingsData.GetOptionsSettings);
        sr.Close();
    }
}

[System.Serializable]
public class GameData
{
    [SerializeField] private int _playerHealth;
    [SerializeField] private MapNodeDataToken _mapNodeData;
    [SerializeField] private CurrencyManagerDataToken _currencyData;
    [SerializeField] private CardDataToken _cardData;
    [SerializeField] private SpecialMechanicsDataToken _specialMechanicData;

    public int GetPlayerHealth => _playerHealth;
    public MapNodeDataToken GetMapNodeData => _mapNodeData;
    public CurrencyManagerDataToken GetCurrencyData => _currencyData;
    public CardDataToken GetCardData => _cardData;
    public SpecialMechanicsDataToken GetSpecialMechanicData => _specialMechanicData;

    public GameData(bool newGameData = false)
    {
        var pdm = PlayerDataManager.Instance;

        if (newGameData)
        {
            _playerHealth = pdm.GetMaxHealth;
            _mapNodeData = new(null, new(0, 0), -1, -1);
            _currencyData = new(100);
            _cardData = new(null);
            _specialMechanicData = new(new int[3], new bool[0], new int[0]);
        }
        else
        {
            _playerHealth = pdm.GetCurrentHealth;
            _mapNodeData = new(pdm.GetCompletedNodes, pdm.GetCurrentNodeIndex, pdm.GetGeneralSeed, pdm.GetNodeMapSeed);
            _currencyData = new(pdm.GetBalance);
            _cardData = new(pdm.GetPlayerDeck.GetCardsInDeck);
            _specialMechanicData = new(pdm.GetAllBuffs, pdm.GetAllCoinFlipsThisRun, pdm.GetAllDiceRollsThisRun);
        }
    }

    // node map vars
    [System.Serializable]
    public class MapNodeDataToken
    {
        [SerializeField] private Vector2IntToken[] _completedNodes;
        [SerializeField] private Vector2IntToken _curNodeIndex;
        [SerializeField] private int _generalSeed;
        [SerializeField] private int _nodeMapSeed;

        public Vector2Int[] GetCompletedNodes => _completedNodes.Select((x) => x.GetVector2Int).ToArray();
        public Vector2Int GetCurrentNodeIndex => _curNodeIndex.GetVector2Int;
        public int GetGeneralSeed => _generalSeed;
        public int GetNodeMapSeed => _nodeMapSeed;

        public MapNodeDataToken(Vector2Int[] completedNodes, Vector2Int curNodeIndex, int generalSeed, int nodeMapSeed)
        {
            List<Vector2IntToken> temp = new();
            if (completedNodes != null)
                foreach (var nodePos in completedNodes)
                    temp.Add(new(nodePos.x, nodePos.y));
            _completedNodes = temp.ToArray();

            _curNodeIndex = new(curNodeIndex.x, curNodeIndex.y);
            _generalSeed = generalSeed;
            _nodeMapSeed = nodeMapSeed;
        }
    }
    [System.Serializable]
    public class Vector2IntToken
    {
        [SerializeField] private int _x;
        [SerializeField] private int _y;

        public Vector2Int GetVector2Int => new(_x, _y);

        public Vector2IntToken(int x, int y)
        {
            _x = x;
            _y = y;
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

    // deck, packs, and card info
    [System.Serializable]
    public class CardDataToken
    {
        [SerializeField] private string[] _deck;

        public string[] GetDeck => _deck;

        public CardDataToken(List<Card> cardsInDeck)
        {
            if (cardsInDeck != null)
            {
                _deck = new string[cardsInDeck.Count];
                for (int i = 0; i < cardsInDeck.Count; i++)
                    _deck[i] = Card.CreateNamingConventionString(cardsInDeck[i]);
            }
            else
                _deck = new string[0];
        }
    }

    [System.Serializable]
    public struct PackToken
    {
        public string packName;
        public string[] cardNames;
    }

    [System.Serializable]
    public class SpecialMechanicsDataToken
    {
        [SerializeField] private int[] _buffsCurrentRun;
        [SerializeField] private bool[] _coinFlipsCurrentRun;
        [SerializeField] private int[] _diceRollsCurrentRun;

        public int[] GetBuffsCurrentRun => _buffsCurrentRun;
        public bool[] GetCoinFlipsCurrentRun => _coinFlipsCurrentRun ?? new bool[0];
        public int[] GetDiceRollsCurrentRun => _diceRollsCurrentRun ?? new int[0];

        public SpecialMechanicsDataToken(int[] buffs, bool[] coinflips, int[] diceRolls)
        {
            _buffsCurrentRun = buffs;
            _coinFlipsCurrentRun = coinflips;
            _diceRollsCurrentRun = diceRolls;
        }
    }
}

[System.Serializable]
public class SettingsData
{
    [SerializeField] private AudioSettingsToken _audioSettings;
    [SerializeField] private OptionsSettingsToken _optionsSettings;

    public SettingsData()
    {
        _audioSettings = new();
        _optionsSettings = new();
    }

    public AudioSettingsToken GetAudioSettings => _audioSettings;
    public OptionsSettingsToken GetOptionsSettings => _optionsSettings;

    [System.Serializable]
    public class OptionsSettingsToken
    {
        [SerializeField] private bool _isCardSelectOnClick;
        [SerializeField] private int _targetFrameRate;
        [SerializeField] private bool _autoEndTurn;
        [SerializeField] private int _resolutionWidth;
        [SerializeField] private int _resolutionHeight;
        [SerializeField] private int _fullscreenModeIndex;

        public bool IsCardSelectOnClick => _isCardSelectOnClick;
        public int TargetFrameRate => _targetFrameRate;
        public bool AutoEndTurn => _autoEndTurn;
        public int ResolutionWidth => _resolutionWidth;
        public int ResolutionHeight => _resolutionHeight;
        public int FullscreenModeIndex => _fullscreenModeIndex;

        public OptionsSettingsToken()
        {
            _isCardSelectOnClick = OptionsSettings.IsCardSelectOnClick;
            _targetFrameRate = OptionsSettings.TargetFrameRate;
            _autoEndTurn = OptionsSettings.AutoEndTurn;
            _resolutionWidth = OptionsSettings.ResolutionWidth;
            _resolutionHeight = OptionsSettings.ResolutionHeight;
            _fullscreenModeIndex = OptionsSettings.FullscreenModeIndex;
        }
    }

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
            _masterVolume = Mathf.Min(AudioManager.Instance.GetMasterVolume, 1);
            _sfxVolume = Mathf.Min(AudioManager.Instance.GetSFXVolume, 1);
            _musicVolume = Mathf.Min(AudioManager.Instance.GetMusicVolume, 1);
        }
    }
}
