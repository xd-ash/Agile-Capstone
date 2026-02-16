using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioLibray", menuName = "Libraries/New Audio Library")]
public class AudioLibrary : ScriptableObject
{
    [Header("Music")]
    [SerializeField] private AudioClip _bgmClip;
    public AudioClip GetBGM => _bgmClip;

    [Header("UI")]
    [SerializeField] private AudioClip _endTurnSfx;
    [SerializeField] private AudioClip _menuButtonClick;
    public AudioClip GetEndTurnSFX => _endTurnSfx;
    public AudioClip GetGetMenuButtonSFX => _menuButtonClick;

    [Header("Misc Game")]
    [SerializeField] private AudioClip _selectCardSfx;
    [SerializeField] private AudioClip _drawCardSfx;
    public AudioClip GetSelectCardSFX => _selectCardSfx;
    public AudioClip GetDrawCardSFX => _drawCardSfx;

    [Header("Unit")]
    [SerializeField] private AudioClip _damageTakeSFX1;
    [SerializeField] private AudioClip _damageTakeSFX2;
    [SerializeField] private AudioClip _shieldHitSFX;
    public AudioClip GetDamageTakeSFX1 => _damageTakeSFX1;
    public AudioClip GetDamageTakeSFX2 => _damageTakeSFX2;
    public AudioClip GetShieldHitSFX => _shieldHitSFX;
}
