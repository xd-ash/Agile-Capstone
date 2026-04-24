using CardSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUpgradeController : MonoBehaviour
{
    [SerializeField] private Button _continueButton;
    [SerializeField] private GameObject _upgradePreviewPanel;
    [SerializeField] private Transform _cardPrefabParent;

    [SerializeField, Space(10f)] private int _numUpgradesAllowed = 3;
    private int _numUpgradesRemaining = 3;

    private Card _selectedCard;
    private bool _canGoBack = true;
    public static bool IsAbleToUpgrade { get; private set; } = true;
    public static bool IsPreviewingUpgrade { get; private set; } = false;

    private Action _onComplete;

    private void OnEnable()
    {
        _upgradePreviewPanel?.SetActive(false);
        IsAbleToUpgrade = true;
    }
    public static CardUpgradeController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    public void InitUpgradeController(Action onComplete)
    {
        _numUpgradesRemaining = _numUpgradesAllowed;
        IsAbleToUpgrade = true;
        _canGoBack = true;
        _selectedCard = null;

        _onComplete = onComplete;

        DeckViewerScript.Instance.UpdateChipsBalance();
        DeckViewerScript.Instance.UpdateUpgradeAmountRemaining(_numUpgradesRemaining);
    }

    public void ShowUpgradePreview(Card cardToUpgrade)
    {
        if (cardToUpgrade == null) return;

        _selectedCard = cardToUpgrade;

        if (cardToUpgrade.GetCardRarity == CardRarity.Epic)
        {
            Debug.LogWarning($"{cardToUpgrade.GetCardName} is max rarity (epic). Upgrade preview failed.)");
            CloseUpgradePreview();
            return;
        }

        _upgradePreviewPanel?.SetActive(true);
        var previewTitle = _upgradePreviewPanel?.transform.GetComponentInChildren<TextMeshProUGUI>();
        if (previewTitle != null)
            previewTitle.text = $"Upgrade {cardToUpgrade.GetCardName} for {cardToUpgrade.GetShopCost} Chips?";

        GameObject cardPrefab = Resources.Load<GameObject>("NewCardPrefab");

        CardRarity upgradedRarity = cardToUpgrade.GetNextCardRarity;

        GameObject initCard = Instantiate(cardPrefab, _cardPrefabParent);
        Card tempCard = new(cardToUpgrade,initCard.transform);
        CardPrefabSetterUpper.SetupCardPrefab(tempCard, CardState.UpgradeMenu);

        GameObject tempUpgradeCard = Instantiate(cardPrefab, _cardPrefabParent);
        Card tempUpgrade = new(cardToUpgrade.GetCardAbility, upgradedRarity, tempUpgradeCard.transform);
        CardPrefabSetterUpper.SetupCardPrefab(tempUpgrade, CardState.UpgradeMenu);

        CardPrefabSetterUpper.SetInactiveVisuals(initCard.transform, false);
        CardPrefabSetterUpper.SetInactiveVisuals(tempUpgradeCard.transform, false);

        IsPreviewingUpgrade = true;
    }

    public void UpgradeSelectedCard()
    {
        if (_selectedCard == null)
        {
            Debug.LogWarning("Selected card was null on upgrade attempt");
            return;
        }

        _selectedCard.UpgradeCard();

        if (!CurrencyManager.Instance.TrySpend(_selectedCard.GetShopCost))
        {
            Debug.LogWarning($"Not enough chips to upgrade.");
            return;
        }

        _numUpgradesRemaining--;
        IsAbleToUpgrade = _numUpgradesRemaining > 0;

        UpdateUI();
        CloseUpgradePreview();

        if (_canGoBack)
            ToggleContinueButton();

        //rebuild to show upgrades
        DeckViewerScript.Instance.BuildDeckScrollViewContent(CardState.UpgradeMenu);
    }
    private void UpdateUI()
    {
        DeckViewerScript.Instance.UpdateChipsBalance();
        DeckViewerScript.Instance.UpdateUpgradeAmountRemaining(_numUpgradesRemaining);
    }

    public void CloseUpgradePreview()
    {
        ClearSelection(_selectedCard?.GetCardTransform);
        _selectedCard = null;
        for (int i = _cardPrefabParent.childCount - 1; i >= 0; i--)
            Destroy(_cardPrefabParent.GetChild(i).gameObject);
        _upgradePreviewPanel.SetActive(false);
        IsPreviewingUpgrade = false;
    }
    private void ToggleContinueButton()
    {
        _canGoBack = false;

        DeckViewerScript.Instance.ToggleBackButton(false);
        _continueButton?.onClick.RemoveAllListeners();
        _continueButton?.onClick.AddListener(OnCompleteUpgrades);
    }
    public void OnCompleteUpgrades()
    {
        _continueButton?.onClick.RemoveAllListeners();

        CloseUpgradePreview();

        DeckViewerScript.Instance?.ToggleBackButton(true);
        DeckViewerScript.Instance?.gameObject?.SetActive(false);

        _onComplete?.Invoke();
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