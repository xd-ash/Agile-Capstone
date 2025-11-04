using System.Collections;
using TMPro;
using UnityEngine;

public class OutOfApPopup : MonoBehaviour
{
    public static OutOfApPopup Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayTime = 1f;
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine _showCoro;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // find CanvasGroup if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();

        // keep the GameObject active (so coroutines can run). hide visuals via canvasGroup
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.gameObject.SetActive(false); // visual root can be inactive; Show() will activate it
        }
    }

    /// <summary>
    /// Show popup. If message is null or empty, the existing TextMeshPro text (set in the Editor) is preserved.
    /// If a non-empty message is supplied, it will override the inspector text for this show.
    /// </summary>
    public void Show(string message = null)
    {
        // Only overwrite inspector text when a non-empty message is provided.
        if (!string.IsNullOrEmpty(message) && messageText != null)
            messageText.text = message;

        // Ensure this component and its GameObject are active so StartCoroutine runs immediately
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
        if (!enabled)
            enabled = true;

        // ensure we have a canvasGroup to operate on
        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();

        if (canvasGroup == null)
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
        if (canvasGroup == null)
        {
            yield break;
        }

        // Make sure the visual root is active and visible
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Wait visible time
        yield return new WaitForSeconds(displayTime);

        // Fade out
        float t = 0f;
        float start = canvasGroup.alpha;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.gameObject.SetActive(false);

        _showCoro = null;
    }
}
