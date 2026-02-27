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

    [Header("Toggles")]
    [SerializeField] private Toggle _cardSelectOnClickToggle;

    private void Awake()
    {
        isPaused = false;
        Time.timeScale = 1f; // IMPORTANT: reset global timeScale on scene load

    }
    private void OnEnable()
    {
        _cardSelectOnClickToggle.isOn = OptionsSettings.IsCardSelectOnClick;
    }

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            if (_masterSlider != null)
                _masterSlider.value = AudioManager.Instance.GetMasterVolume;

            if (_sfxSlider != null)
                _sfxSlider.value = AudioManager.Instance.GetSFXVolume;

            if (_musicSlider != null)
                _musicSlider.value = AudioManager.Instance.GetMusicVolume;
        }

        // Hook up listeners
        _masterSlider?.onValueChanged.AddListener(OnMasterChanged);
        _sfxSlider?.onValueChanged.AddListener(OnSfxChanged);
        _musicSlider?.onValueChanged.AddListener(OnMusicChanged);

        _cardSelectOnClickToggle.onValueChanged.AddListener(OptionsSettings.UpdateCardSelect);
    }

    private void Update()
    {
        // Esc will toggle pause & back out of any settings menu instantly to unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // No real pause menu on main menu, so just exit out of settings when Esc is pressed
            if (TransitionScene.Instance.GetCurrentScene == "MainMenu")
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
            if (DeckAndHandManager.Instance.GetSelectedCard != null &&
                DeckAndHandManager.Instance.GetSelectedCard.GetCardTransform.TryGetComponent(out CardSelect card))
            {
                TargetingStopped();
                DeckAndHandManager.Instance.OnCardAblityCancel?.Invoke();
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
        _masterSlider?.onValueChanged.RemoveListener(OnMasterChanged);
        _sfxSlider?.onValueChanged.RemoveListener(OnSfxChanged);
        _musicSlider?.onValueChanged.RemoveListener(OnMusicChanged);
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

        if (TransitionScene.Instance.GetCurrentScene != "MainMenu")
            _pauseMenuPanel.SetActive(true);
    }


    private void OnMasterChanged(float value)
    {
        AudioManager.Instance?.SetMasterVolume(value);
    }

    private void OnSfxChanged(float value)
    {
        AudioManager.Instance?.SetSfxVolume(value);
    }

    private void OnMusicChanged(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
    }
}
