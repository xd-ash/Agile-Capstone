using System.Collections;
using TMPro;
using UnityEngine;
using static GameObjectPool;

public class FloatingTextController : MonoBehaviour
{
    [SerializeField] private GameObject _textPrefab;
    [SerializeField] private FloatingTextSettingsLibrary _settingsLibrary;

    [SerializeField, Range(0, 1)] private float _textFloatDistance; //move me to settings class?

    private void Awake()
    {
        _settingsLibrary = Resources.Load<FloatingTextSettingsLibrary>("Libraries/FloatingTextSettingsLibrary");
        _textPrefab = Resources.Load<GameObject>("FloatingTextPrefab");
    }

    public void SpawnFloatingText(string textContent, TextPresetType preset = TextPresetType.Default)
    {
        var settings = _settingsLibrary.GetPresetFromType(preset);
        
        var textGO = Spawn(_textPrefab, transform.position, Quaternion.identity, Vector3.one, transform);
        textGO.transform.localPosition = Vector3.zero;

        if (!textGO.TryGetComponent(out TextMeshPro text))
            text = textGO.AddComponent<TextMeshPro>();

        text.text = textContent;
        InitTextSettings(text, settings);

        StartCoroutine(MoveTextCoro(text, settings.GetTextDuration));
    }
    private void InitTextSettings(TextMeshPro text, FloatingTextPreset preset)
    {
        text.fontSize = preset.GetFontSize;
        text.color = preset.GetFontColor;
        text.alignment = preset.GetTextAlignment;
    }

    // add specialized text movement/events with different coroutines?

    public IEnumerator MoveTextCoro(TextMeshPro text, float duration)
    {
        Vector3 initPos = text.transform.localPosition;
        Color color = text.color;

        for (float timer = 0f; timer < duration; timer += Time.deltaTime)
        {
            float ratio = timer / duration;
            transform.localPosition = Vector3.Lerp(initPos, initPos + new Vector3(0, _textFloatDistance, 0), ratio);
            color.a = Mathf.Lerp(1f, 0f, ratio);
            yield return null;
        }

        Remove(text.gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, _textFloatDistance, 0));
        Gizmos.DrawSphere(transform.position + new Vector3(0f, _textFloatDistance, 0f), 0.03f);
    }
}