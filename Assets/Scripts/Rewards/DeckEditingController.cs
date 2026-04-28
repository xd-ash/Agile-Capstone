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
    [SerializeField] private int _baseRemovalCost = 15;

    [Header("Card Swap")]
    [SerializeField] private GameObject _swapPreviewPanel;
    [SerializeField] private Transform _swapCardPrefabParent;
    [SerializeField] private Card _cardToSwapIn = null;

    private int _numEditsRemaining = 0;

    private Transform PrefabParent => GetPrefabParent();
    private GameObject PreviewPanel => GetPreviewPanel();

    private Transform GetPrefabParent()
    {
        switch (DeckViewerScript.Instance.ViewerState)
        {
            case CardState.UpgradeMenu:
                return _upgradeCardPrefabParent;
            case CardState.CardRemoval:
                return _removalCardPrefabParent;
            case CardState.CardSwap:
                return _swapCardPrefabParent;
            default:
                return null;
        }
    }
    private GameObject GetPreviewPanel()
    {
        switch (DeckViewerScript.Instance.ViewerState)
        {
            case CardState.UpgradeMenu:
                return _upgradePreviewPanel;
            case CardState.CardRemoval:
                return _removalPreviewPanel;
            case CardState.CardSwap:
                return _swapPreviewPanel;
            default: 
                return null;
        }
    }

    public int GetRemovalCost => CardShopManager.Instance == null ? _baseRemovalCost : CardShopManager.Instance.GetRemovalCost;

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

        if (NodeMapManager.Instance == null || !NodeMapManager.Instance.gameObject.activeInHierarchy)
            _numEditsRemaining = 1;
        else
            _numEditsRemaining = state == CardState.UpgradeMenu ? CampNodeController.RemainingUpgrades : CampNodeController.RemainingRemovals;
        IsAbleToEdit = true;
        _canGoBack = true;
        _selectedCard = null;
        _cardToSwapIn = null;

        _onComplete = onComplete;

        UpdateUI();
    }

    public void SetCardSwap(Card cardToSwapIn)
    {
        _cardToSwapIn = cardToSwapIn;
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
        {
            switch (state)
            {
                case CardState.UpgradeMenu:
                    previewTitle.text = $"Upgrade {selectedCard.GetCardName} for {selectedCard.GetShopCost} Chips?";
                    break;
                case CardState.CardRemoval:
                    previewTitle.text = $"Remove {selectedCard.GetCardName} for {GetRemovalCost} Chips?";
                    break;
                case CardState.CardSwap:
                    previewTitle.text = $"Swap {selectedCard.GetCardName} for {_cardToSwapIn.GetCardName}?";
                    break;
            }
        }

        GameObject cardPrefab = Resources.Load<GameObject>("NewCardPrefab");

        GameObject selectedCardGO = Instantiate(cardPrefab, PrefabParent);
        Card tempCard = new(selectedCard, selectedCardGO.transform);

        CardPrefabSetterUpper.SetupCardPrefab(tempCard, state);
        if (state != CardState.CardSwap)
        {
            CardPrefabSetterUpper.SetInactiveVisuals(selectedCardGO.transform, false);
            CardPrefabSetterUpper.SetCostTextGO(tempCard, false);
        }

        if (state == CardState.UpgradeMenu)
        {
            CardRarity upgradedRarity = selectedCard.GetNextCardRarity;

            GameObject tempUpgradeCard = Instantiate(cardPrefab, PrefabParent);
            Card tempUpgrade = new(selectedCard.GetCardAbility, upgradedRarity, tempUpgradeCard.transform);

            CardPrefabSetterUpper.SetupCardPrefab(tempUpgrade, state);
            CardPrefabSetterUpper.SetInactiveVisuals(tempUpgradeCard.transform, false);
            CardPrefabSetterUpper.SetCostTextGO(tempUpgrade, false);
        }
        else if (state == CardState.CardSwap && _cardToSwapIn != null)
        {
            GameObject cardToSwapInGO = Instantiate(cardPrefab, PrefabParent);
            _cardToSwapIn.OnPrefabCreation(cardToSwapInGO.transform);

            CardPrefabSetterUpper.SetupCardPrefab(_cardToSwapIn, state);
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

        var editCost = DeckViewerScript.Instance.ViewerState == CardState.UpgradeMenu ? _selectedCard.GetShopCost : _baseRemovalCost;
        if (!CurrencyManager.Instance.TrySpend(editCost))
        {
            Debug.LogWarning($"Not enough chips to edit.");
            return;
        }

        _numEditsRemaining--;
        IsAbleToEdit = _numEditsRemaining > 0;

        if (!IsAbleToEdit)
        {
            OnCompleteEdits();
            return;
        }

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
        else if (DeckViewerScript.Instance.ViewerState == CardState.CardSwap)
        {
            PlayerDataManager.Instance.UpdateCardData(_selectedCard, false);
            OnCompleteEdits();
        }
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
        if (PrefabParent != null)
        {
            for (int i = PrefabParent.childCount - 1; i >= 0; i--)
                Destroy(PrefabParent.GetChild(i).gameObject);
        }
        PreviewPanel?.SetActive(false);
        IsPreviewingEdit = false;
    }
    private void ToggleContinueButton()
    {
        _canGoBack = false;

        _continueButton?.onClick.RemoveAllListeners();
        _continueButton?.onClick.AddListener(OnCompleteEdits);
    }
    public void OnCompleteEdits()
    {
        _continueButton?.onClick.RemoveAllListeners();

        CloseEditPreview();

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