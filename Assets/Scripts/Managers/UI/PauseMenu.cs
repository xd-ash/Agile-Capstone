using UnityEngine;
using UnityEngine.UI;
using static AbilityEvents;
using CardSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenuPanel;
    public static bool isPaused = false;

    [SerializeField] private GameObject _settingsPanel;
    
    [Header("Sliders")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _musicSlider;

    private void Awake()
    {
        isPaused = false;
        Time.timeScale = 1f; // IMPORTANT: reset global timeScale on scene load
    }
    
    private void Start()
    {
        if (AudioManager.instance != null)
        {
            if (_masterSlider != null)
                _masterSlider.value = AudioManager.instance.GetMasterVolume;

            if (_sfxSlider != null)
                _sfxSlider.value = AudioManager.instance.GetSFXVolume;

            if (_musicSlider != null)
                _musicSlider.value = AudioManager.instance.GetMusicVolume;
        }

        // Hook up listeners
        _masterSlider?.onValueChanged.AddListener(OnMasterChanged);

        _sfxSlider?.onValueChanged.AddListener(OnSfxChanged);

        _musicSlider?.onValueChanged.AddListener(OnMusicChanged);
    }

    private void Update()
    {
        // Esc will toggle pause & back out of any settings menu instantly to unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // No real pause menu on main menu, so just exit out of settings when Esc is pressed
            if (TransitionScene.instance.GetCurrentScene == "MainMenu")
            {
                _settingsPanel.SetActive(false);
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
            if (DeckAndHandManager.instance.GetSelectedCard != null &&
                DeckAndHandManager.instance.GetSelectedCard.GetCardTransform.TryGetComponent(out CardSelect card))
            {
                TargetingStopped();
                //card.ReturnCardToHand();
                DeckAndHandManager.instance.OnCardAblityCancel?.Invoke();
            }
        }
        
        if (isPaused)
            Time.timeScale = 0f; // Pause the game
        else
            Time.timeScale = 1f; // Resume the game

        _pauseMenuPanel?.SetActive(isPaused);
        _settingsPanel?.SetActive(false); // close settings menu 
    }

    private void OnDestroy()
    {
        if (_masterSlider != null)
            _masterSlider.onValueChanged.RemoveListener(OnMasterChanged);

        if (_sfxSlider != null)
            _sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);

        if (_musicSlider != null)
            _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
    }
    
    public void OpenSettings()
    {
        if (_settingsPanel != null)
            _settingsPanel.SetActive(true);
        _pauseMenuPanel.SetActive(false);
    }
    
    public void CloseSettings()
    {
        if (_settingsPanel != null)
            _settingsPanel.SetActive(false);

        if (TransitionScene.instance.GetCurrentScene != "MainMenu")
            _pauseMenuPanel.SetActive(true);
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
