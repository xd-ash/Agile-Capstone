using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;

public class PauseMenu : MonoBehaviour
{

    public Canvas pauseMenuCanvas;
    public static bool isPaused = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
            isPaused = !isPaused;
        }
    }

    private void TogglePause()
    {
        if (Time.timeScale == 1)
        {
            Time.timeScale = 0; // Pause the game
            pauseMenuCanvas.enabled = true; // Show pause menu UI
            
        }
        else
        {
            Time.timeScale = 1; // Resume the game
            pauseMenuCanvas.enabled = false; // Hide pause menu UI
        }
    }
}
