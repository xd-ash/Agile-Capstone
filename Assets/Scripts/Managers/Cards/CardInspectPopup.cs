using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardSystem;

public class CardInspectPopup : MonoBehaviour
{
    public static CardInspectPopup Instance { get; private set; }

    [Header("Core UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform popupPanel;

    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;

    [Header("Optional")]
    [SerializeField] private Button backgroundButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideInstant();

        if (backgroundButton != null)
            backgroundButton.onClick.AddListener(Hide);
    }

    public void Show(CardAbilityDefinition def)
    {
        if (def == null)
        {
            Debug.LogError("[CardInspectPopup] Tried to show NULL CardAbilityDefinition");
            return;
        }

        nameText.text = def.GetCardName;
        descriptionText.text = def.GetDescription;
        costText.text = def.GetApCost.ToString();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        popupPanel.localScale = Vector3.one;
    }

    public void Hide()
    {
        HideInstant();
    }

    private void HideInstant()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        popupPanel.localScale = Vector3.zero;
    }
}
