using CardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditController : MonoBehaviour
{
    private DeckViewerScript _deckViewPanel;
    private Button _continueButton;

    [SerializeField] private GameObject _upgradePreviewPanel;
    [SerializeField] private Transform _cardPrefabParent;
    [SerializeField] private TextMeshProUGUI _chipsBalanceText;

    private Card _selectedCard;

    public static bool IsPreviewingRemoval { get; private set; } = false;

    public static DeckEditController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    public void InitDeckEditing()
    {

    }

    public void ShowRemoveCardConfirmPopUp(Card selectedCard)
    {
        if (selectedCard == null) return;

        _selectedCard = selectedCard;

        if (selectedCard.GetCardRarity == CardRarity.Epic)
        {
            Debug.LogWarning($"{selectedCard.GetCardName} is max rarity (epic). Upgrade preview failed.)");
            CloseUpgradePreview();
            return;
        }

        _upgradePreviewPanel?.SetActive(true);
        var previewTitle = _upgradePreviewPanel?.transform.GetComponentInChildren<TextMeshProUGUI>();
        if (previewTitle != null)
            previewTitle.text = $"Remove {selectedCard.GetCardName} for {selectedCard.GetShopCost} Chips?";

        GameObject cardPrefab = Resources.Load<GameObject>("NewCardPrefab");

        CardRarity upgradedRarity = selectedCard.GetNextCardRarity;

        GameObject initCard = Instantiate(cardPrefab, _cardPrefabParent);
        Card tempCard = new(selectedCard, initCard.transform);
        CardPrefabSetterUpper.SetupCardPrefab(tempCard, CardState.DeckEdit);

        //bandaid fix for epic upgrade preview being greyed out and ahving shop cost
        var inactiveOverlay = initCard.transform.Find("InactiveOverlay")?.gameObject;
        inactiveOverlay?.SetActive(false);
        var costText = initCard?.transform.Find("CostTextBG")?.gameObject;
        costText?.SetActive(false);
        //

        IsPreviewingRemoval = true;
    }

    public void RemoveSelectedCard()
    {
        if (_selectedCard == null)
        {
            Debug.LogWarning("Selected card was null on removal attempt");
            return;
        }

        _selectedCard.UpgradeCard();

        if (!CurrencyManager.Instance.TrySpend(_selectedCard.GetShopCost))
        {
            Debug.LogWarning($"Not enough chips to remove card.");
            return;
        }

        CloseUpgradePreview();

        //rebuild to show upgrades
        DeckViewerScript.Instance.BuildDeckScrollViewContent(CardState.DeckEdit);
    }

    public void CloseUpgradePreview()
    {
        ClearSelection(_selectedCard?.GetCardTransform);
        _selectedCard = null;
        for (int i = _cardPrefabParent.childCount - 1; i >= 0; i--)
            Destroy(_cardPrefabParent.GetChild(i).gameObject);
        _upgradePreviewPanel.SetActive(false);
        IsPreviewingRemoval = false;
    }
    private void ClearSelection(Transform cardTransform)
    {
        if (cardTransform == null) return;
        var cfs = cardTransform.GetComponentInParent<CardFunctionScript>();
        var cs = cardTransform.GetComponentInParent<CardSelect>();
        cs?.ToggleHighlightAndScale(false);
        cfs?.ClearSelection(0f);
    }
}
