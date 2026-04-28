using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SettingsData;

public class AudioManager : MonoBehaviour
{
    private AudioSource _musicSourceA;
    private AudioSource _musicSourceB;
    private AudioSource _sfxSource;

    private bool _usingSourceA = true;
    private AudioSource ActiveMusic => _usingSourceA ? _musicSourceA : _musicSourceB;
    private AudioSource InactiveMusic => _usingSourceA ? _musicSourceB : _musicSourceA;

    [SerializeField] private AudioLibrary _audioLibrary;

    [Header("Volumes")]
    [Range(0f, 1f), SerializeField] private float _masterVolume = 1.0f;
    [Range(0f, 1f), SerializeField] private float _sfxVolume = 1.0f;
    [Range(0f, 1f), SerializeField] private float _musicVolume = 0.5f;
    
    [Header("SFX Pitch")]
    [SerializeField] private float _pitchMin = 0.9f;
    [SerializeField] private float _pitchMax = 1.1f;

    [Header("Crossfade")]
    [SerializeField] private float _crossfadeDuration = 2f;

    [Header("Pause Duck")]
    [Range(0f, 1f), SerializeField] private float _duckMultiplier = 0.3f;
    [SerializeField] private float _duckDuration = 0.5f;
    
    private List<AudioClip> _combatShuffleBag = new();
    private AudioClip _lastCombatClip;

    private bool _isDucked = false;
    private Coroutine _duckCoroutine;
    private Coroutine _crossfadeCoroutine;

    private Dictionary<string, float> _savedMusicTimes = new();
    private AudioClip _activeClip;

    [System.Serializable]
    public class SceneMusicEntry
    {
        public string sceneName;
        public List<AudioClip> clips;
        public bool loop = true;
        [Range(0f, 1f)] public float volume = 0.5f;
    }
    [SerializeField] private List<SceneMusicEntry> _sceneMusic = new();

    public float GetMasterVolume => _masterVolume;
    public float GetSFXVolume => _sfxVolume;
    public float GetMusicVolume => _musicVolume;

    private AudioClip _pendingUseClip;

    public void PlayDrawCardSfx() => PlaySFX(_audioLibrary.GetDrawCardSFX);
    public void PlayCardSelectSfx() => PlaySFX(_audioLibrary.GetSelectCardSFX);
    public void PlayButtonSFX() => PlaySFX(_audioLibrary.GetGetMenuButtonSFX);
    public void PlayEndTurnSFX(Unit unit) => PlaySFX(_audioLibrary.GetEndTurnSFX);
    public void PlayDamageTakeSFX(Unit unit) => PlayGameSFX(unit.GetTeam == Team.Friendly ? _audioLibrary.GetDamageTakeSFX1 : _audioLibrary.GetDamageTakeSFX2);
    public void PlayShieldHitSFX() => PlayGameSFX(_audioLibrary.GetShieldHitSFX);

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

        InitAudioSource(ref _musicSourceA, _musicVolume, true);
        InitAudioSource(ref _musicSourceB, _musicVolume, true);
        InitAudioSource(ref _sfxSource, _sfxVolume, false);

        SaveLoadScript.LoadSettings?.Invoke();
        ApplyVolumes();
        OnSceneSwap("MainMenu");
    }

    private void OnEnable()
    {
        TransitionScene.SceneSwap += OnSceneSwap;
        AbilityEvents.OnAbilityUsed += HandleAbilityUsed;
    }

    public void OnSceneSwap(string sceneLoaded)
    {
        if (sceneLoaded == "MainMenu" && TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnEnd -= PlayEndTurnSFX;
        }

        if (_activeClip != null && ActiveMusic.isPlaying)
        {
            string previousScene = GetSceneNameForClip(_activeClip);
            if (previousScene != null)
            {
                _savedMusicTimes[previousScene] = ActiveMusic.time;
            }
        }

        var entry = _sceneMusic.FirstOrDefault(e => e.sceneName == sceneLoaded);
        if (entry != null)
        {
            AudioClip clip = GetNextClip(entry);
            if (clip != null)
            {
                float resumeTime = 0f;
                if (sceneLoaded != "Combat" && _savedMusicTimes.TryGetValue(sceneLoaded, out float saved))
                {
                    resumeTime = saved;
                }

                CrossfadeToMusic(clip, entry.loop, entry.volume, resumeTime);
                return;
            }
        }

        CrossfadeToMusic(null, false, 0f, 0f);
    }

    public void LevelLoadInits()
    {
        if (_audioLibrary.GetBGM != null)
        {
            CrossfadeToMusic(_audioLibrary.GetBGM, true, _musicVolume, 0f);
        }
    }

    private void CrossfadeToMusic(AudioClip clip, bool loop, float targetVolume, float startTime)
    {
        if (_crossfadeCoroutine != null)
        {
            StopCoroutine(_crossfadeCoroutine);
        }

        _crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(clip, loop, targetVolume, startTime));
    }

    private IEnumerator CrossfadeRoutine(AudioClip clip, bool loop, float targetVolume, float startTime)
    {
        AudioSource outgoing = ActiveMusic;
        AudioSource incoming = InactiveMusic;

        float outStartVol = outgoing.volume;
        float targetVol = _masterVolume * _musicVolume * targetVolume;

        //set up incoming source before fade begins
        if (clip != null)
        {
            incoming.clip = clip;
            incoming.loop = loop;
            incoming.volume = 0f;
            incoming.time = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, clip.length - 0.1f));
            incoming.Play();
            _activeClip = clip;
        }

        float elapsed = 0f;
        while (elapsed < _crossfadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; //unscaled so it works while paused
            float t = Mathf.Clamp01(elapsed / _crossfadeDuration);

            outgoing.volume = Mathf.Lerp(outStartVol, 0f, t);

            if (clip != null)
            {
                incoming.volume = Mathf.Lerp(0f, _isDucked ? targetVol * _duckMultiplier : targetVol, t);
            }

            yield return null;
        }

        outgoing.Stop();
        outgoing.clip = null;
        outgoing.volume = 0f;

        _usingSourceA = !_usingSourceA; //swap active source
        _crossfadeCoroutine = null;
    }

    public void DuckMusic()
    {
        _isDucked = true;
        if (_duckCoroutine != null) StopCoroutine(_duckCoroutine);
        _duckCoroutine = StartCoroutine(DuckRoutine(_masterVolume * _musicVolume * _duckMultiplier));
    }

    public void UnduckMusic()
    {
        _isDucked = false;
        if (_duckCoroutine != null) StopCoroutine(_duckCoroutine);
        _duckCoroutine = StartCoroutine(DuckRoutine(_masterVolume * _musicVolume));
    }

    private IEnumerator DuckRoutine(float targetVolume)
    {
        float startVolume = ActiveMusic.volume;
        float elapsed = 0f;

        while (elapsed < _duckDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _duckDuration);
            ActiveMusic.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        ActiveMusic.volume = targetVolume;
        _duckCoroutine = null;
    }

    private void HandleAbilityUsed(Team unitTeam = Team.Friendly)
    {
        if (_pendingUseClip == null)
        {
            return;
        }
        PlayGameSFX(_pendingUseClip);
        _pendingUseClip = null;
    }

    //no pitch shift for UI sounds
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || _sfxSource == null)
        {
            return;
        }
        _sfxSource.pitch = 1f;
        _sfxSource.PlayOneShot(clip);
    }

    //randomized pitch for game sounds
    public void PlayGameSFX(AudioClip clip)
    {
        if (clip == null || _sfxSource == null)
        {
            return;
        }
        _sfxSource.pitch = Random.Range(_pitchMin, _pitchMax);
        _sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float volume = -1f)
    {
        float vol = volume >= 0f ? volume : _musicVolume;
        CrossfadeToMusic(clip, loop, vol, 0f);
    }

    public void StopMusic()
    {
        CrossfadeToMusic(null, false, 0f, 0f);
    }

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
        {
            _sfxSource.volume = _masterVolume * _sfxVolume;
        }

        float musicTarget = _masterVolume * _musicVolume;
        if (ActiveMusic != null)
            ActiveMusic.volume = _isDucked ? musicTarget * _duckMultiplier : musicTarget;

        SaveLoadScript.SaveSettings?.Invoke();
    }

    private void InitAudioSource(ref AudioSource source, float volume, bool loop)
    {
        source = gameObject.AddComponent<AudioSource>();
        source.loop = loop;
        source.spatialBlend = 0f;
        source.playOnAwake = false;
        source.volume = volume;
    }

    private string GetSceneNameForClip(AudioClip clip)
    {
        var entry = _sceneMusic.FirstOrDefault(e => e.clips != null && e.clips.Contains(clip));
        return entry?.sceneName;
    }
    
    private AudioClip GetNextClip(SceneMusicEntry entry)
    {
        if (entry.clips == null || entry.clips.Count == 0)
            return null;

        if (entry.clips.Count == 1)
            return entry.clips[0];

        if (_combatShuffleBag.Count == 0)
        {
            _combatShuffleBag = new List<AudioClip>(entry.clips);

            for (int i = _combatShuffleBag.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = _combatShuffleBag[i]; 
                _combatShuffleBag[i] = _combatShuffleBag[j];
                _combatShuffleBag[j] = temp;
            }

            if (_combatShuffleBag[0] == _lastCombatClip && _combatShuffleBag.Count > 1)
            {
                var temp = _combatShuffleBag[0];
                _combatShuffleBag[0] = _combatShuffleBag[_combatShuffleBag.Count - 1];
                _combatShuffleBag[_combatShuffleBag.Count - 1] = temp;
            }
        }

        AudioClip next = _combatShuffleBag[0];
        _combatShuffleBag.RemoveAt(0);
        _lastCombatClip = next;
        return next;
    }
}