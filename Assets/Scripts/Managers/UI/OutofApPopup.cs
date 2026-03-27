using System.Collections;
using TMPro;
using UnityEngine;

public class OutOfApPopup : MonoBehaviour
{
    public static OutOfApPopup Instance { get; private set; }

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private float _displayTime = 1f;
    [SerializeField] private float _fadeDuration = 0.25f;

    private Coroutine _showCoro;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // find CanvasGroup if not assigned
        if (_canvasGroup == null)
            _canvasGroup = GetComponentInChildren<CanvasGroup>();

        // keep the GameObject active (so coroutines can run). hide visuals via canvasGroup
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.gameObject.SetActive(false); // visual root can be inactive; Show() will activate it
        }
    }

    /// <summary>
    /// Show popup. If message is null or empty, the existing TextMeshPro text (set in the Editor) is preserved.
    /// If a non-empty message is supplied, it will override the inspector text for this show.
    /// </summary>
    public void Show(string message = null)
    {
        // Only overwrite inspector text when a non-empty message is provided.
        if (!string.IsNullOrEmpty(message) && _messageText != null)
            _messageText.text = message;

        // Ensure this component and its GameObject are active so StartCoroutine runs immediately
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
        if (!enabled)
            enabled = true;

        // ensure we have a canvasGroup to operate on
        if (_canvasGroup == null)
            _canvasGroup = GetComponentInChildren<CanvasGroup>();

        if (_canvasGroup == null)
        {
            Debug.LogWarning("[OutOfApPopup] No CanvasGroup assigned/found. Cannot show popup.");
            return;
        }

        // stop any existing show coroutine then start a fresh one
        if (_showCoro != null) StopCoroutine(_showCoro);
        _showCoro = StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        if (_canvasGroup == null)
        {
            yield break;
        }

        // Make sure the visual root is active and visible
        _canvasGroup.gameObject.SetActive(true);
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;

        // Wait visible time
        yield return new WaitForSeconds(_displayTime);

        // Fade out
        float t = 0f;
        float start = _canvasGroup.alpha;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(start, 0f, t / _fadeDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.gameObject.SetActive(false);

        _showCoro = null;
    }
}
