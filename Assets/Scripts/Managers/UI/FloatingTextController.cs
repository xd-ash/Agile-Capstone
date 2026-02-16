using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameObjectPool;

public class FloatingTextController : MonoBehaviour
{
    private GameObject _textPrefab;
    private FloatingTextSettingsLibrary _settingsLibrary;
    private Coroutine _queueCoro;
    private Queue<Action> _textCoroQueue = new();

    [SerializeField] private float _textQueueDelay = 1f;
    [SerializeField, Range(0, 1)] private float _textFloatDistance; //move me to settings class?

    private void Awake()
    {
        _settingsLibrary = Resources.Load<FloatingTextSettingsLibrary>("Libraries/FloatingTextSettingsLibrary");
        _textPrefab = Resources.Load<GameObject>("FloatingTextPrefab");
    }

    public void SpawnFloatingText(string textContent, TextPresetType preset = TextPresetType.Default)
    {
        var settings = _settingsLibrary.GetPresetFromType(preset);
        
        var textGO = GameObject.Instantiate(_textPrefab, transform.position, Quaternion.identity, transform);
        //var textGO = Spawn(_textPrefab, transform.position, Quaternion.identity, Vector3.one, transform);
        textGO.transform.localPosition = Vector3.zero;
        textGO.gameObject.SetActive(false);

        if (!textGO.TryGetComponent(out TextMeshPro text))
            text = textGO.AddComponent<TextMeshPro>();

        text.text = textContent;
        InitTextSettings(ref text, settings);

        _textCoroQueue.Enqueue(() => StartCoroutine(MoveTextCoro(text, settings.GetTextDuration)));

        if (_queueCoro == null)
            _queueCoro = StartCoroutine(TextQueueCoro());
    }
    private void InitTextSettings(ref TextMeshPro text, FloatingTextPreset preset)
    {
        text.fontSize = preset.GetFontSize;
        text.color = preset.GetFontColor;
        text.alignment = preset.GetTextAlignment;
    }

    public IEnumerator TextQueueCoro()
    {
        while (_textCoroQueue.Count > 0)
        {
            _textCoroQueue.Dequeue()?.Invoke(); //call action to start the text coro
            yield return new WaitForSecondsRealtime(_textQueueDelay);
        }

        _queueCoro = null;
    }
    public IEnumerator MoveTextCoro(TextMeshPro text, float duration)
    {
        text.gameObject.SetActive(true);

        Vector3 initPos = text.transform.localPosition;
        Color color = text.color;

        for (float timer = 0f; timer < duration; timer += Time.deltaTime)
        {
            float ratio = timer / duration;
            text.transform.localPosition = Vector3.Lerp(initPos, initPos + new Vector3(0, _textFloatDistance, 0), ratio);
            color.a = Mathf.Lerp(1f, 0f, ratio);
            yield return null;
        }

        Destroy(text.gameObject);
        //Remove(text.gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, _textFloatDistance, 0));
        Gizmos.DrawSphere(transform.position + new Vector3(0f, _textFloatDistance, 0f), 0.03f);
    }
}