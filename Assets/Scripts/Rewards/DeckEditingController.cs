using CardSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditingController : MonoBehaviour
{
    [SerializeField] private Button _continueButton;

    private Card _selectedCard;
    private bool _canGoBack = true;

    [Header("Card Upgrades")]
    [SerializeField] private GameObject _upgradePreviewPanel;
    [SerializeField] private Transform _upgradeCardPrefabParent;

    [Header("Card Removal")]
    [SerializeField] private GameObject _removalPreviewPanel;
    [SerializeField] private Transform _removalCardPrefabParent;
    [SerializeField] private int _removalCost = 15;
    
    private int _numEditsRemaining = 0;

    private Transform PrefabParent => DeckViewerScript.Instance.ViewerState == CardState.UpgradeMenu ? _upgradeCardPrefabParent : _removalCardPrefabParent;
    private GameObject PreviewPanel => DeckViewerScript.Instance.ViewerState == CardState.UpgradeMenu ? _upgradePreviewPanel : _removalPreviewPanel;

    public int GetRemovalCost => _removalCost;

    public static bool IsAbleToEdit { get; private set; } = true;
    public static bool IsPreviewingEdit { get; private set; } = false;

    private Action<int> _onComplete;

    public static DeckEditingController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void OnEnable()
    {
        _upgradePreviewPanel?.SetActive(false);
        _removalPreviewPanel?.SetActive(false);
        IsAbleToEdit = true;
    }

    public void InitEditingController(Action<int> onComplete)
    {
        var state = DeckViewerScript.Instance.ViewerState;

        _numEditsRemaining = state == CardState.UpgradeMenu ? CampNodeController.RemainingUpgrades : CampNodeController.RemainingRemovals;
        IsAbleToEdit = true;
        _canGoBack = true;
        _selectedCard = null;

        _onComplete = onComplete;

        UpdateUI();
    }

    public void ShowPreview(Card selectedCard)
    {
        if (selectedCard == null) return;

        _selectedCard = selectedCard;

        var state = DeckViewerScript.Instance.ViewerState;

        if (state == CardState.UpgradeMenu && selectedCard.GetCardRarity == CardRarity.Epic)
        {
            Debug.LogWarning($"{selectedCard.GetCardName} is max rarity (epic). Upgrade preview failed.)");
            CloseEditPreview();
            return;
        }

        PreviewPanel?.SetActive(true);
        var previewTitle = PreviewPanel?.transform.GetComponentInChildren<TextMeshProUGUI>();
        if (previewTitle != null)
            previewTitle.text = state == CardState.UpgradeMenu ? $"Upgrade {selectedCard.GetCardName} for {selectedCard.GetShopCost} Chips?" :
                                                                 $"Remove {selectedCard.GetCardName} for {_removalCost} Chips?";

        GameObject cardPrefab = Resources.Load<GameObject>("NewCardPrefab");

        GameObject selectedCardGO = Instantiate(cardPrefab, PrefabParent);
        Card tempCard = new(selectedCard, selectedCardGO.transform);

        CardPrefabSetterUpper.SetupCardPrefab(tempCard, state);
        CardPrefabSetterUpper.SetInactiveVisuals(selectedCardGO.transform, false);
        CardPrefabSetterUpper.SetCostTextGO(tempCard, false);

        if (state == CardState.UpgradeMenu)
        {
            CardRarity upgradedRarity = selectedCard.GetNextCardRarity;

            GameObject tempUpgradeCard = Instantiate(cardPrefab, PrefabParent);
            Card tempUpgrade = new(selectedCard.GetCardAbility, upgradedRarity, tempUpgradeCard.transform);

            CardPrefabSetterUpper.SetupCardPrefab(tempUpgrade, CardState.UpgradeMenu);
            CardPrefabSetterUpper.SetInactiveVisuals(tempUpgradeCard.transform, false);
            CardPrefabSetterUpper.SetCostTextGO(tempUpgrade, false);
        }

        IsPreviewingEdit = true;
    }

    public void EditSelectedCard()
    {
        if (_selectedCard == null)
        {
            Debug.LogWarning("Selected card was null on edit attempt");
            return;
        }

        MakeEdit();

        var editCost = DeckViewerScript.Instance.ViewerState == CardState.UpgradeMenu ? _selectedCard.GetShopCost : _removalCost;
        if (!CurrencyManager.Instance.TrySpend(editCost))
        {
            Debug.LogWarning($"Not enough chips to edit.");
            return;
        }

        _numEditsRemaining--;
        IsAbleToEdit = _numEditsRemaining > 0;

        UpdateUI();
        CloseEditPreview();

        if (_canGoBack)
            ToggleContinueButton();

        //rebuild
        DeckViewerScript.Instance.BuildDeckScrollViewContent();
    }

    private void MakeEdit()
    {
        if (DeckViewerScript.Instance.ViewerState == CardState.UpgradeMenu)
            _selectedCard.UpgradeCard();
        else if (DeckViewerScript.Instance.ViewerState == CardState.CardRemoval)
            PlayerDataManager.Instance.UpdateCardData(_selectedCard, false);
    }

    private void UpdateUI()
    {
        DeckViewerScript.Instance.UpdateChipsBalance();
        DeckViewerScript.Instance.UpdateEditAmountRemaining(_numEditsRemaining);
    }

    public void CloseEditPreview()
    {
        ClearSelection(_selectedCard?.GetCardTransform);
        _selectedCard = null;
        for (int i = PrefabParent.childCount - 1; i >= 0; i--)
            Destroy(PrefabParent.GetChild(i).gameObject);
        PreviewPanel?.SetActive(false);
        IsPreviewingEdit = false;
    }
    private void ToggleContinueButton()
    {
        _canGoBack = false;

        //DeckViewerScript.Instance.ToggleBackButton(false);
        _continueButton?.onClick.RemoveAllListeners();
        _continueButton?.onClick.AddListener(OnCompleteEdits);
    }
    public void OnCompleteEdits()
    {
        _continueButton?.onClick.RemoveAllListeners();

        CloseEditPreview();

        //DeckViewerScript.Instance?.ToggleBackButton(true);
        DeckViewerScript.Instance?.gameObject?.SetActive(false);

        _onComplete?.Invoke(_numEditsRemaining);
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