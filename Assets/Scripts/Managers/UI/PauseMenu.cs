using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Toggle _autoEndTurnToggle;

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Dropdown _fullscreenDropdown;
    [SerializeField] private TMP_Dropdown _frameRateDropdown;

    private List<Vector2Int> _availableResolutions = new();
    private readonly int[] _frameRateOptions = { 30, 60, 120, 144, -1 };
    private readonly string[] _frameRateLabels = { "30", "60", "120", "144", "Unlimited" };

    private void Awake()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    private void OnEnable() { }

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

        _masterSlider?.onValueChanged.AddListener(OnMasterChanged);
        _sfxSlider?.onValueChanged.AddListener(OnSfxChanged);
        _musicSlider?.onValueChanged.AddListener(OnMusicChanged);

        _cardSelectOnClickToggle.isOn = OptionsSettings.IsCardSelectOnClick;
        _autoEndTurnToggle.isOn = OptionsSettings.AutoEndTurn;
        _cardSelectOnClickToggle.onValueChanged.AddListener(OptionsSettings.UpdateCardSelect);
        _autoEndTurnToggle?.onValueChanged.AddListener(OptionsSettings.UpdateAutoEndTurn);

        PopulateResolutionDropdown();
        PopulateFullscreenDropdown();
        PopulateFrameRateDropdown();
    }

    private void PopulateResolutionDropdown()
    {
        if (_resolutionDropdown == null) return;

        _availableResolutions = Screen.resolutions
            .Select(r => new Vector2Int(r.width, r.height))
            .Distinct()
            .OrderByDescending(r => r.x)
            .ThenByDescending(r => r.y)
            .ToList();

        _resolutionDropdown.ClearOptions();
        List<string> options = _availableResolutions.Select(r => $"{r.x} x {r.y}").ToList();
        _resolutionDropdown.AddOptions(options);

        int currentIndex = _availableResolutions.FindIndex(
            r => r.x == OptionsSettings.ResolutionWidth && r.y == OptionsSettings.ResolutionHeight);
        if (currentIndex >= 0)
            _resolutionDropdown.SetValueWithoutNotify(currentIndex);

        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void PopulateFullscreenDropdown()
    {
        if (_fullscreenDropdown == null) return;

        _fullscreenDropdown.ClearOptions();
        _fullscreenDropdown.AddOptions(new List<string>
        {
            "Exclusive Fullscreen", "Borderless Window", "Maximized Window", "Windowed"
        });

        _fullscreenDropdown.SetValueWithoutNotify(OptionsSettings.FullscreenModeIndex);
        _fullscreenDropdown.onValueChanged.AddListener(OnFullscreenChanged);
    }

    private void PopulateFrameRateDropdown()
    {
        if (_frameRateDropdown == null) return;

        _frameRateDropdown.ClearOptions();
        _frameRateDropdown.AddOptions(new List<string>(_frameRateLabels));

        int currentIndex = System.Array.IndexOf(_frameRateOptions, OptionsSettings.TargetFrameRate);
        if (currentIndex >= 0)
            _frameRateDropdown.SetValueWithoutNotify(currentIndex);

        _frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
    }

    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= _availableResolutions.Count) return;
        Vector2Int res = _availableResolutions[index];
        OptionsSettings.UpdateResolution(res.x, res.y);
    }

    private void OnFullscreenChanged(int index) => OptionsSettings.UpdateFullscreenMode(index);

    private void OnFrameRateChanged(int index)
    {
        if (index < 0 || index >= _frameRateOptions.Length) return;
        OptionsSettings.UpdateTargetFrameRate(_frameRateOptions[index]);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
            if (DeckAndHandManager.Instance.GetSelectedCard != null)
            {
                TargetingStopped();
                DeckAndHandManager.Instance.OnCardAblityCancel?.Invoke();
            }
        }

        if (isPaused)
        {
            Time.timeScale = 0f;
            AudioManager.Instance?.DuckMusic();
        }
        else
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.UnduckMusic();
        }

        _pauseMenuPanel?.SetActive(isPaused);
        _settingsPanel?.SetActive(false);
    }

    private void OnDestroy()
    {
        _masterSlider?.onValueChanged.RemoveListener(OnMasterChanged);
        _sfxSlider?.onValueChanged.RemoveListener(OnSfxChanged);
        _musicSlider?.onValueChanged.RemoveListener(OnMusicChanged);
        _resolutionDropdown?.onValueChanged.RemoveListener(OnResolutionChanged);
        _fullscreenDropdown?.onValueChanged.RemoveListener(OnFullscreenChanged);
        _frameRateDropdown?.onValueChanged.RemoveListener(OnFrameRateChanged);
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

    private void OnMasterChanged(float value) => AudioManager.Instance?.SetMasterVolume(value);
    private void OnSfxChanged(float value) => AudioManager.Instance?.SetSfxVolume(value);
    private void OnMusicChanged(float value) => AudioManager.Instance?.SetMusicVolume(value);
}