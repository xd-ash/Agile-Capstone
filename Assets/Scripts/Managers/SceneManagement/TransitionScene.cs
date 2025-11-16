using CardSystem;
using System;
using UnityEngine;
using static AbilityEvents;

public class TransitionScene : MonoBehaviour
{
    public static TransitionScene instance { get; private set; }
    private GameObject mainMenu, pauseMenu;
    //public string sceneToLoad; Adam removed in favor of adding param to StartTransition method
                                //(did this b/c I combined pause and main menu canvases. Buttons now set the scene string)
    public static Action<string> SceneSwap;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        mainMenu = transform.Find("MainMenu").gameObject;
        pauseMenu = transform.Find("PauseMenu").gameObject;
    }

    //delete me
    private void Update()
    {
        Debug.Log("istargeting: " + AbilityEvents.IsTargeting);
    }

    public void StartTransition(string targetScene = "MainMenu")
    {
        Debug.Log("Scene transition started.");
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);

        //Handle anything else needed between scenes here
        PauseMenu.isPaused = false;
        AbilityEvents.TargetingStopped();

        if (targetScene == "MainMenu")
            pauseMenu.SetActive(false);
        mainMenu.SetActive(targetScene == "MainMenu" ? true : false);

        //if (targetScene == "LevelOne")
            //GetComponent<Canvas>().enabled = false;

        SceneSwap?.Invoke(targetScene);
    }
     
    // quit button functionality
    public void QuitApplication()
    {
        Application.Quit();
    }
}
