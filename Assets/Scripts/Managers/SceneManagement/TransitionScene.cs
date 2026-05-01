using System;
using UnityEngine;

public class TransitionScene : MonoBehaviour
{
    private GameObject mainMenu;//, pauseMenu, rewardsMenu, deckViewMenu;
    private string _currScene = "MainMenu";

    public static Action<string> SceneSwap;

    public string GetCurrentScene => _currScene;
    public static bool IsTutorial { get; private set; }

    public static TransitionScene Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var mainMenuTransform = transform.Find("MainMenu");
        if (mainMenuTransform != null)
            mainMenu = mainMenuTransform.gameObject;
        else
            Debug.LogWarning("TransitionScene: 'MainMenu' child not found under " + name);
        /*
        var pauseMenuTransform = transform.Find("PauseMenu");
        if (pauseMenuTransform != null)
            pauseMenu = pauseMenuTransform?.gameObject;
        else
            Debug.LogWarning("TransitionScene: 'PauseMenu' child not found under " + name);

        var deckViewMenuTransform = transform.Find("DeckViewMenu");
        if (deckViewMenuTransform != null)
            deckViewMenu = deckViewMenuTransform?.gameObject;
        else
            Debug.LogWarning("TransitionScene: 'PauseMenu' child not found under " + name);

        var rewardsMenuTransform = transform.Find("RewardsMenu");
        if (rewardsMenuTransform != null)
            rewardsMenu = rewardsMenuTransform?.gameObject;
        else
            Debug.LogWarning("TransitionScene: 'RewardsMenu' child not found under " + name);
        */
    }
    private void ResetMenusOnMainMenu()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i)?.gameObject;
            if (child == null || child.name == "MainMenu") continue;
            child.SetActive(false);
        }
    }
    public void StartTutorial()
    {
        // Load tutorial map
        var library = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTileMapSOLibrary");
        var tutorialMaps = library.GetTileMapSOsFromType(CombatMapType.Tutorial);

        CustomTileMapSO tutorialMap = null;
        if (tutorialMaps != null && tutorialMaps.Length > 0)
            tutorialMap = tutorialMaps[0];
        else
            Debug.LogError("TransitionScene: No tutorial maps found in library.");

        // Load tutorial deck
        var deckConfig = Resources.Load<TutorialDeckConfig>("TutorialDeckConfig");
        if (deckConfig != null && deckConfig.GetTutorialCards.Count > 0)
        {
            var tutorialDeck = new Deck(deckConfig.GetTutorialCards);
            PlayerDataManager.Instance.UpdateCardData(tutorialDeck);
        }
        else
            Debug.LogError("TransitionScene: TutorialDeckConfig not found or empty.");

        var rngEnemies = UnitLibrary.GetRandomEnemies(1, DateTime.Now.Millisecond, true, new UnitType[] {UnitType.TankEnemy, UnitType.MedicEnemy});

        PlayerDataManager.Instance.SetCurrMapNodeData(new CombatMapData
        {
            maxEnemiesAllowed = 1,
            selectedEnemies = rngEnemies,
            selectedMap = tutorialMap
        });

        IsTutorial = true;

        UnityEngine.SceneManagement.SceneManager.LoadScene("Combat");
        _currScene = "Combat";

        mainMenu?.SetActive(false);
        SceneSwap?.Invoke("Combat");
        SaveLoadScript.SaveGame?.Invoke();

        //StartTransition("Combat");
    }

    public void StartTransition(string targetScene = "MainMenu")
    {
        IsTutorial = false;

        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        _currScene = targetScene;
        
        PauseMenu.isPaused = false;
        AbilityEvents.TargetingStopped();

        if (targetScene == "MainMenu")
        {
            ResetMenusOnMainMenu();
            SaveLoadScript.LoadGame?.Invoke();

            //pauseMenu?.SetActive(false);
            //rewardsMenu?.SetActive(false);
        }

        mainMenu?.SetActive(targetScene == "MainMenu");

        SceneSwap?.Invoke(targetScene);
        SaveLoadScript.SaveGame?.Invoke();
    }

    public void QuitApplication()
    {
        SaveLoadScript.SaveGame?.Invoke();
        Application.Quit();
    }
}