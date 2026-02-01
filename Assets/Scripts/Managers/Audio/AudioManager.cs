using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SettingsData;

public class AudioManager : MonoBehaviour
{
    private AudioSource _musicSource;   // looped bgm
    private AudioSource _sfxSource;     // one-shots

    [SerializeField] private AudioLibrary _audioLibrary;

    [Header("Volumes")]
    [Range(0f, 1f), SerializeField] private float _masterVolume = 1.0f;
    [Range(0f, 1f), SerializeField] private float _sfxVolume = 1.0f;
    [Range(0f, 1f), SerializeField] private float _musicVolume = 0.5f;

    [System.Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;
        public AudioClip clip;
        public bool loop = true;
        [Range(0f, 1f)] public float volume = 0.5f;
    }
    [SerializeField] private List<SceneMusicEntry> _sceneMusic = new List<SceneMusicEntry>();

    public float GetMasterVolume => _masterVolume;
    public float GetSFXVolume => _sfxVolume;
    public float GetMusicVolume => _musicVolume;
    
    // Queued clip to play when AbilityEvents.AbilityUsed() fires
    private AudioClip _pendingUseClip;

    public static AudioManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitAudioSource(ref _musicSource, _musicVolume);
        InitAudioSource(ref _sfxSource, _sfxVolume);

        /*_musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.spatialBlend = 0f;
        _musicSource.playOnAwake = false;
        _musicSource.volume = _musicVolume;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.spatialBlend = 0f;
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = _sfxVolume;*/

        SaveLoadScript.LoadSettings?.Invoke();
        ApplyVolumes();
    }

    // Quick play methods
    public void PlayDrawCardSfx() => PlaySFX(_audioLibrary.GetDrawCardSFX);
    public void PlayCardSelectSfx() => PlaySFX(_audioLibrary.GetSelectCardSFX);
    public void PlayButtonSFX() => PlaySFX(_audioLibrary.GetGetMenuButtonSFX);
    public void PlayEndTurnSFX() => PlaySFX(_audioLibrary.GetEndTurnSFX);

    private void OnEnable()
    {
        TransitionScene.SceneSwap += OnSceneSwap;
        AbilityEvents.OnAbilityUsed += HandleAbilityUsed; //removed the -= of this as audio manager (is never disabled at the moment)
    }
    
    public void OnSceneSwap(string sceneLoaded)
    {
        // Unsubscribe TurnManager listener if we return to main menu
        if (sceneLoaded == "MainMenu" && TurnManager.Instance != null)
            TurnManager.Instance.OnPlayerTurnEnd -= PlayEndTurnSFX;

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
        if (_audioLibrary.GetBGM != null)
            PlayMusic(_audioLibrary.GetBGM, true);
    }

    private void HandleAbilityUsed(Team unitTeam = Team.Friendly)
    {
        if (_pendingUseClip == null) return;
        PlaySFX(_pendingUseClip);
        _pendingUseClip = null; // clear after playing
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip);
    }

    // Added optional volume parameter so each scene entry can control music volume
    public void PlayMusic(AudioClip clip, bool loop = true, float volume = -1f)
    {
        if (clip == null || _musicSource == null) return;
        if (_musicSource.clip == clip && _musicSource.isPlaying) return;
        _musicSource.loop = loop;
        _musicSource.clip = clip;
        _musicSource.volume = (volume >= 0f) ? Mathf.Clamp01(volume) : _musicVolume;
        ApplyVolumes();
        _musicSource.Play();
    }

    public void StopMusic()
    {
        // Add some kind of fade here?
        // Add logic for transitioning to new music?
        _musicSource?.Stop();
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
    public void ApplyVolumes()
    {
        if (_sfxSource != null)
            _sfxSource.volume = _masterVolume * _sfxVolume;

        if (_musicSource != null)
            _musicSource.volume = _masterVolume * _musicVolume;

        SaveLoadScript.SaveSettings?.Invoke();
    }
    private void InitAudioSource(ref AudioSource audioSource, float volume)
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
    }
}
