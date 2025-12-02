using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }
    //public TurnManager turnManager = TurnManager.instance;
    public AudioClip endTurnSfx;
    public AudioClip drawCardSfx;
    public AudioClip selectCardSfx;
    public AudioClip bgmClip;
    public AudioClip menuButtonClick;

    [System.Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;
        public AudioClip clip;
        public bool loop = true;
        [Range(0f, 1f)] public float volume = 0.5f;
    }
    public List<SceneMusicEntry> sceneMusic = new List<SceneMusicEntry>();

    [Header("Volumes")]
    [Range(0f, 1f)] public float sfxVolume = 1.0f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    private AudioSource _music;   // looped bgm
    private AudioSource _sfx;     // one-shots

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
        _music.volume = musicVolume;

        _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.loop = false;
        _sfx.spatialBlend = 0f;
        _sfx.playOnAwake = false;
        _sfx.volume = sfxVolume;
    }

    private void OnEnable()
    {
        TransitionScene.SceneSwap += OnSceneSwap;

        AbilityEvents.OnAbilityUsed += HandleAbilityUsed; //removed the -= of this as audio manager
                                                          //is never disabled at the moment
    }

    private void OnDisable()
    {
        //moved to scene swap since audio manager is DontDestroyOnLoad currently
    }

    private void Start()
    {
       
    }

    public void OnSceneSwap(string sceneLoaded)
    {
        // Unsubscribe TurnManager listener if we return to main menu
        if (sceneLoaded == "MainMenu")
        {
            if (TurnManager.instance != null)
                TurnManager.instance.OnPlayerTurnEnd -= HandleTurnChanged;
        }

        // Look for a matching scene music entry
        var entry = sceneMusic.FirstOrDefault(e => e.sceneName == sceneLoaded);
        if (entry != null && entry.clip != null)
        {
            // play immediately (no special delay). If some scenes need a delay, add a conditional Invoke.
            PlayMusic(entry.clip, entry.loop, entry.volume);
            return;
        }

        // Fallback behavior for existing LevelOne behavior (keeps original delayed init)
        switch (sceneLoaded)
        {
            case "LevelOne":
                Invoke(nameof(LevelLoadInits), .5f); // Bandaid fix for turn manager not being loaded instantly on scene swap
                break;
        }
    }

    public void LevelLoadInits()
    {
        //TurnManager.instance.OnPlayerTurnEnd += HandleTurnChanged;
        if (bgmClip != null)
        {
            PlayMusic(bgmClip, true);
        }
    }
    private void HandleTurnChanged()
    {
        PlaySFX(endTurnSfx);
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
        _sfx.volume = sfxVolume;
        _sfx.PlayOneShot(clip);
    }

    public void PlayDrawCardSfx() => PlaySFX(drawCardSfx);
    public void PlayCardSelectSfx() => PlaySFX(selectCardSfx);
    public void PlayButtonSFX() => PlaySFX(menuButtonClick);

    // Added optional volume parameter so each scene entry can control music volume
    public void PlayMusic(AudioClip clip, bool loop = true, float volume = -1f)
    {
        if (clip == null || _music == null) return;
        if (_music.clip == clip && _music.isPlaying) return;
        _music.loop = loop;
        _music.clip = clip;
        _music.volume = (volume >= 0f) ? Mathf.Clamp01(volume) : musicVolume;
        _music.Play();
    }

    // Called by CardSelect before invoking the ability
    public void SetPendingUseSfx(AudioClip clip)
    {
        _pendingUseClip = clip;
    }

    public void SetSfxVolume(float v)   { sfxVolume = Mathf.Clamp01(v);   if (_sfx)   _sfx.volume = sfxVolume; }
    public void SetMusicVolume(float v) { musicVolume = Mathf.Clamp01(v); if (_music) _music.volume = musicVolume; }
}
