using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardSystem; // Make sure this matches your CardAbilityDefinition namespace

public class CardInspectPopup : MonoBehaviour
{
    public static CardInspectPopup Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button backgroundButton; // Optional: click outside to close

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private Vector3 showScale = Vector3.one;
    [SerializeField] private Vector3 hideScale = new Vector3(0.8f, 0.8f, 0.8f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Start hidden
        canvasGroup.alpha = 0f;
        panelRoot.localScale = hideScale;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (backgroundButton != null)
            backgroundButton.onClick.AddListener(Hide);
    }

    public void Show(CardAbilityDefinition def)
    {
        if (def == null) return;

        if (nameText != null) nameText.text = def.GetCardName;
        if (descriptionText != null) descriptionText.text = def.GetDescription;
        if (costText != null) costText.text = def.GetApCost.ToString();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
