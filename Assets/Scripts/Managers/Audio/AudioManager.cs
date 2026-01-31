using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SettingsData;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }
    
    //public TurnManager turnManager = TurnManager.instance;
    [SerializeField] private AudioClip _endTurnSfx;
    [SerializeField] private AudioClip _drawCardSfx;
    [SerializeField] private AudioClip _selectCardSfx;
    [SerializeField] private AudioClip _bgmClip;
    [SerializeField] private AudioClip _menuButtonClick;

    [System.Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;
        public AudioClip clip;
        public bool loop = true;
        [Range(0f, 1f)] public float volume = 0.5f;
    }
    [SerializeField] private List<SceneMusicEntry> _sceneMusic = new List<SceneMusicEntry>();

    [Header("Volumes")]
    [Range(0f, 1f), SerializeField] private float _masterVolume = 1.0f;
    [Range(0f, 1f), SerializeField] private float _sfxVolume = 1.0f;
    [Range(0f, 1f), SerializeField] private float _musicVolume = 0.5f;

    public float GetMasterVolume => _masterVolume;
    public float GetSFXVolume => _sfxVolume;
    public float GetMusicVolume => _musicVolume;

    private AudioSource _music;   // looped bgm
    private AudioSource _sfx;     // one-shots
    
    private float _musicBaseVolume = 1f;
    
    // Queued clip to play when AbilityEvents.AbilityUsed() fires
    private AudioClip _pendingUseClip;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        _music = gameObject.AddComponent<AudioSource>();
        _music.loop = true;
        _music.spatialBlend = 0f;
        _music.playOnAwake = false;
        _music.volume = _musicVolume;

        _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.loop = false;
        _sfx.spatialBlend = 0f;
        _sfx.playOnAwake = false;
        _sfx.volume = _sfxVolume;

        SaveLoadScript.LoadSettings?.Invoke();
        ApplyVolumes();
    }

    private void OnEnable()
    {
        TransitionScene.SceneSwap += OnSceneSwap;
        AbilityEvents.OnAbilityUsed += HandleAbilityUsed; //removed the -= of this as audio manager (is never disabled at the moment)
    }
    
    public void OnSceneSwap(string sceneLoaded)
    {
        // Unsubscribe TurnManager listener if we return to main menu
        if (sceneLoaded == "MainMenu" && TurnManager.instance != null)
            TurnManager.instance.OnPlayerTurnEnd -= HandleTurnChanged;

        // Look for a matching scene music entry
        var entry = _sceneMusic.FirstOrDefault(e => e.sceneName == sceneLoaded);
        if (entry != null && entry.clip != null)
        {
            // play immediately (no special delay). If some scenes need a delay, add a conditional Invoke.
            PlayMusic(entry.clip, entry.loop, entry.volume);
            return;
        }
        else
            StopMusic();

        // Fallback behavior for existing LevelOne behavior
        switch (sceneLoaded)
        {
            case "LevelOne":
                //LevelLoadInits();
                break;
        }
    }

    public void LevelLoadInits()
    {
        if (_bgmClip != null)
            PlayMusic(_bgmClip, true);
    }
    private void HandleTurnChanged()
    {
        PlaySFX(_endTurnSfx);
    }

    private void HandleAbilityUsed(Team unitTeam = Team.Friendly)
    {
        if (_pendingUseClip != null)
        {
            PlaySFX(_pendingUseClip);
            _pendingUseClip = null; // clear after playing
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || _sfx == null) return;

        ApplyVolumes();
        _sfx.PlayOneShot(clip);
    }

    public void PlayDrawCardSfx() => PlaySFX(_drawCardSfx);
    public void PlayCardSelectSfx() => PlaySFX(_selectCardSfx);
    public void PlayButtonSFX() => PlaySFX(_menuButtonClick);

    // Added optional volume parameter so each scene entry can control music volume
    public void PlayMusic(AudioClip clip, bool loop = true, float volume = -1f)
    {
        if (clip == null || _music == null) return;
        if (_music.clip == clip && _music.isPlaying) return;
        _music.loop = loop;
        _music.clip = clip;
        _music.volume = (volume >= 0f) ? Mathf.Clamp01(volume) : _musicVolume;
        ApplyVolumes();
        _music.Play();
    }

    public void StopMusic()
    {
        // Add some kind of fade here?
        // Add logic for transitioning to new music?
        _music?.Stop();
    }

    // Called by CardSelect before invoking the ability
    public void SetPendingUseSfx(AudioClip clip)
    {
        _pendingUseClip = clip;
    }

    public void LoadVolumeSettings(AudioSettingsToken audioSettings)
    {
        _masterVolume = audioSettings.GetMasterVolume;
        _sfxVolume = audioSettings.GetSFXVolume;
        _musicVolume = audioSettings.GetMusicVolume;
    }

    public void ApplyVolumes()
    {
        if (_sfx != null)
            _sfx.volume = _masterVolume * _sfxVolume;

        if (_music != null)
            _music.volume = _masterVolume * _musicVolume * _musicBaseVolume;

        SaveLoadScript.SaveSettings?.Invoke();
    }

    public void SetMasterVolume(float v)
    {
        _masterVolume = Mathf.Clamp01(v);
        ApplyVolumes();
    }

    public void SetSfxVolume(float v)
    {
        _sfxVolume = Mathf.Clamp01(v);
        ApplyVolumes();
    }

    public void SetMusicVolume(float v)
    {
        _musicVolume = Mathf.Clamp01(v);
        ApplyVolumes();
    }
}
