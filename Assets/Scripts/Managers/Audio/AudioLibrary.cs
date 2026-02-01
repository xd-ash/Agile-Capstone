using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioLibray", menuName = "Audio Library/New Library")]
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

}
