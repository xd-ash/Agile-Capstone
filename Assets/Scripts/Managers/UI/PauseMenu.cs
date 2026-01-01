using UnityEngine;
using UnityEngine.UI;
using static AbilityEvents;
using CardSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    public static bool isPaused = false;

    [SerializeField] private GameObject settingsPanel;
    
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    private void Awake()
    {
        isPaused = false;
        Time.timeScale = 1f; // IMPORTANT: reset global timeScale on scene load
    }
    
    private void Start()
    {
        if (AudioManager.instance != null)
        {
            if (masterSlider != null)
                masterSlider.value = AudioManager.instance.masterVolume;

            if (sfxSlider != null)
                sfxSlider.value = AudioManager.instance.sfxVolume;

            if (musicSlider != null)
                musicSlider.value = AudioManager.instance.musicVolume;
        }

        // Hook up listeners
        masterSlider?.onValueChanged.AddListener(OnMasterChanged);

        sfxSlider?.onValueChanged.AddListener(OnSfxChanged);

        musicSlider?.onValueChanged.AddListener(OnMusicChanged);
    }

    private void Update()
    {
        // Esc will toggle pause & back out of any settings menu instantly to unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // No real pause menu on main menu, so just exit out of settings when Esc is pressed
            if (TransitionScene.instance.GetCurrentScene == "MainMenu")
            {
                settingsPanel.SetActive(false);
                return;
            }

            TogglePause();
        }
    }
    private void TogglePause()
    {
        isPaused = !isPaused;
        
        if (IsTargeting && !isPaused)
        {
            if (CardManager.instance.SelectedCard != null &&
                CardManager.instance.SelectedCard.CardTransform.TryGetComponent(out CardSelect card))
            {
                AbilityEvents.TargetingStopped();
                card.ReturnCardToHand();
                CardManager.instance.OnCardAblityCancel?.Invoke();
            }
        }
        
        if (isPaused)
            Time.timeScale = 0f; // Pause the game
        else
            Time.timeScale = 1f; // Resume the game

        pauseMenuPanel?.SetActive(isPaused);
        settingsPanel?.SetActive(false); // close settings menu 
    }

    private void OnDestroy()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(OnMasterChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
    }
    
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (TransitionScene.instance.GetCurrentScene != "MainMenu")
            pauseMenuPanel.SetActive(true);
    }


    private void OnMasterChanged(float value)
    {
        AudioManager.instance?.SetMasterVolume(value);
    }

    private void OnSfxChanged(float value)
    {
        AudioManager.instance?.SetSfxVolume(value);
    }

    private void OnMusicChanged(float value)
    {
        AudioManager.instance?.SetMusicVolume(value);
    }
}
