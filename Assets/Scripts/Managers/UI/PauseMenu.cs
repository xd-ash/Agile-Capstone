using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static AbilityEvents;
using CardSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuCanvas;
    public static bool isPaused = false;

    private void Awake()
    {
        pauseMenuCanvas = TransitionScene.instance?.transform.Find("PauseMenu").gameObject;

        ///Adam - moved this to the TransitionScene script when updating MenuCanvas to be DontDestroyOnLoad
            // Ensure the menu is hidden and the game is unpaused when the scene loads
            //if (pauseMenuCanvas != null)
                //pauseMenuCanvas.enabled = false;
        ///

        isPaused = false;
        Time.timeScale = 1f; // IMPORTANT: reset global timeScale on scene load
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsTargeting)
            {
                if (CardSystem.CardManager.instance.selectedCard.CardTransform.TryGetComponent<CardSelect>(out CardSelect card))
                {
                    card.ReturnCardToHand();
                    AbilityEvents.TargetingStopped();
                    CardManager.instance.OnCardAblityCancel?.Invoke();
                }
            }
            else
                TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Pause the game
            if (pauseMenuCanvas != null)
                pauseMenuCanvas.SetActive(true);
                //pauseMenuCanvas.enabled = true;
        }
        else
        {
            Time.timeScale = 1f; // Resume the game
            if (pauseMenuCanvas != null)
                pauseMenuCanvas.SetActive(false);
                //pauseMenuCanvas.enabled = false;
        }
    }
}
