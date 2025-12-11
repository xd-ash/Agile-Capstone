using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static AbilityEvents;
using CardSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public static bool isPaused = false;

    [SerializeField] private GameObject settingsPanel;
    
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    private void Awake()
    {
        ///Adam - moved this to the TransitionScene script when updating MenuCanvas to be DontDestroyOnLoad
            // Ensure the menu is hidden and the game is unpaused when the scene loads
            //if (pauseMenuCanvas != null)
                //pauseMenuCanvas.enabled = false;
        ///

        isPaused = false;
        Time.timeScale = 1f; // IMPORTANT: reset global timeScale on scene load
    }
    
    private void Start()
    {
        if (AudioManager.instance != null)
        {
            if (masterSlider != null)
            {
                masterSlider.value = AudioManager.instance.masterVolume;
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.instance.sfxVolume;
            }

            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.instance.musicVolume;
            }
        }

        // Hook up listeners
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(OnMasterChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
            CloseSettings();
        }
    }
    private void TogglePause()
    {
        isPaused = !isPaused;

        if (IsTargeting && !PauseMenu.isPaused)
        {
            if (CardSystem.CardManager.instance.selectedCard != null &&
                CardSystem.CardManager.instance.selectedCard.CardTransform.TryGetComponent<CardSelect>(out CardSelect card))
            {
                AbilityEvents.TargetingStopped();
                card.ReturnCardToHand();
                CardManager.instance.OnCardAblityCancel?.Invoke();
            }
        }
        
        if (isPaused)
        {
            Time.timeScale = 0f; // Pause the game
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
                //pauseMenuCanvas.enabled = true;
        }
        else
        {
            Time.timeScale = 1f; // Resume the game
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
                //pauseMenuCanvas.enabled = false;
        }
    }
    
    private void OnDestroy()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        }
    }
    
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        pauseMenuPanel.SetActive(false);
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
    

    private void OnMasterChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMasterVolume(value);
        }
    }

    private void OnSfxChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSfxVolume(value);
        }
    }

    private void OnMusicChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(value);
        }
    }
}
