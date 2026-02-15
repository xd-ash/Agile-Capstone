using TMPro;
using UnityEngine;

public enum TextPresetType { MissTextPreset, CoinFlipPreset, Default };

[CreateAssetMenu(fileName = "FloatingTextSettingsLibrary", menuName = "Libraries/Floating Text Settings Library")]
public class FloatingTextSettingsLibrary : ScriptableObject
{
    [SerializeField] private FloatingTextPreset[] _presets;
    public FloatingTextPreset GetDefaultTextSettings => GetPresetFromType(TextPresetType.Default);

    public FloatingTextPreset GetPresetFromType(TextPresetType type)
    {
        foreach (var p in _presets)
            if (p.GetPresetType == type)
                return p;
        return null;
    }
}
[System.Serializable]
public class FloatingTextPreset
{
    [SerializeField] private TextPresetType _presetType;
    [SerializeField] private float _textDuration;
    [SerializeField] private float _fontSize;
    [SerializeField] private Color _fontColor;
    [SerializeField] private TextAlignmentOptions _textAlignment;

    public TextPresetType GetPresetType => _presetType;
    public float GetTextDuration => _textDuration;
    public float GetFontSize => _fontSize;
    public Color GetFontColor => _fontColor;
    public TextAlignmentOptions GetTextAlignment => _textAlignment;
}
