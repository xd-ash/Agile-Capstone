using System;
using UnityEngine;

public class TransitionScene : MonoBehaviour
{
    public static TransitionScene instance { get; private set; }

    private GameObject mainMenu, pauseMenu;
    private string _currScene = "MainMenu";

    public static Action<string> SceneSwap;

    public string GetCurrentScene => _currScene;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        var mainMenuTransform = transform.Find("MainMenu");
        if (mainMenuTransform != null)
            mainMenu = mainMenuTransform.gameObject;
        else
            Debug.LogWarning("TransitionScene: 'MainMenu' child not found under " + name);

        var pauseMenuTransform = transform.Find("PauseMenu");
        if (pauseMenuTransform != null)
            pauseMenu = pauseMenuTransform?.gameObject;
        else
            Debug.LogWarning("TransitionScene: 'PauseMenu' child not found under " + name);
    }

    public void StartTransition(string targetScene = "MainMenu")
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        _currScene = targetScene;

        PauseMenu.isPaused = false;
        AbilityEvents.TargetingStopped();

        if (targetScene == "MainMenu")
            pauseMenu?.SetActive(false);

        mainMenu?.SetActive(targetScene == "MainMenu");

        SceneSwap?.Invoke(targetScene);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}