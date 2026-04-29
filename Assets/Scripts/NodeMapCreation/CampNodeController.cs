using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardSystem;
using System.Collections.Generic;

public enum RestOptions { AP = 0, MaxHealth = 1, StartingHandSize = 2 }

public class CampNodeController : MonoBehaviour
{
    private DeckViewerScript _deckViewPanel;

    private Card _selectedCard;

    [Header("Edit Option")]
    [SerializeField] private GameObject _deckEditPanel;
    [SerializeField] private GameObject _editPanelBackButton;
    [SerializeField] private CardChoiceSelectScript _chooseCardScript;
    [SerializeField] private Transform _upgradePreviewCardParent;
    [SerializeField] private GameObject _upgradePreviewPanel;

    [SerializeField, Space(5)] private TextMeshProUGUI _remainingUpgradesText;
    [SerializeField] private TextMeshProUGUI _remainingRemovalsText;

    [SerializeField, Space(5)] private Button _upgradeButton;
    [SerializeField] private Button _removeButton;

    [SerializeField, Space(5)] private int _numUpgradesAllowed = 3;
    [SerializeField] private int _numUpgradeCardOptions = 3;
    [SerializeField] private int _numRemovalsAllowed = 1;

    [Header("Rest Option")]
    [SerializeField] private GameObject _restOptionsPanel;
    [SerializeField] private int _apIncrease = 1,
                                 _maxHealthIncrease = 5,
                                 _startingHandSizeIncrease = 1;

    private Action _onComplete;

    public static int RemainingUpgrades { get; private set; }
    public static int RemainingRemovals { get; private set; }
    public static bool IsPreviewingUpgrade { get; private set; }

    public void InitCampNode(Action onComplete)
    {
        _onComplete = onComplete;
        _deckViewPanel = FindFirstObjectByType<DeckViewerScript>(FindObjectsInactive.Include);

        RemainingUpgrades = _numUpgradesAllowed;
        RemainingRemovals = _numRemovalsAllowed;
    }
    public void OnStartRest()
    {
        _restOptionsPanel?.SetActive(true);
    }
    public void OnStartEdit()
    {
        _deckEditPanel?.SetActive(true);

        if (_remainingUpgradesText != null)
            _remainingUpgradesText.text = RemainingUpgrades.ToString();
        if (_remainingRemovalsText != null)
            _remainingRemovalsText.text = RemainingRemovals.ToString();
    }
    public void OnCompleteCampNode()
    {
        _deckEditPanel?.SetActive(false);
        _restOptionsPanel?.SetActive(false);

        _onComplete?.Invoke();

        gameObject.SetActive(false);
    }

    public void OnStartUpgrading()
    {
        _chooseCardScript?.gameObject.SetActive(true);
        _chooseCardScript.ShowOptions(GetRandomUpgradeOptions());
    }
    public void ShowUpgradePreview(Card selectedCard)
    {
        _selectedCard = selectedCard;

        _upgradePreviewPanel?.SetActive(true);
        IsPreviewingUpgrade = true;

        GameObject cardPrefab = Resources.Load<GameObject>("NewCardPrefab");

        GameObject selectedCardGO = Instantiate(cardPrefab, _upgradePreviewCardParent);
        Card tempCard = new(selectedCard, selectedCardGO.transform);

        CardPrefabSetterUpper.SetupCardPrefab(tempCard, CardState.FreeUpgradeMenu);
        CardPrefabSetterUpper.SetInactiveVisuals(selectedCardGO.transform, false);
        CardPrefabSetterUpper.SetCostTextGO(tempCard, false);

        CardRarity upgradedRarity = selectedCard.GetNextCardRarity;

        GameObject tempUpgradeCard = Instantiate(cardPrefab, _upgradePreviewCardParent);
        Card tempUpgrade = new(selectedCard.GetCardAbility, upgradedRarity, tempUpgradeCard.transform);

        CardPrefabSetterUpper.SetupCardPrefab(tempUpgrade, CardState.FreeUpgradeMenu);
        CardPrefabSetterUpper.SetInactiveVisuals(tempUpgradeCard.transform, false);
        CardPrefabSetterUpper.SetCostTextGO(tempUpgrade, false);
    }
    public void HideUpgradePreview()
    {
        ClearSelection(_selectedCard?.GetCardTransform);
        _selectedCard = null;
        if (_upgradePreviewCardParent != null)
        {
            for (int i = _upgradePreviewCardParent.childCount - 1; i >= 0; i--)
                Destroy(_upgradePreviewCardParent.GetChild(i).gameObject);
        }
        _upgradePreviewPanel?.SetActive(false);
        IsPreviewingUpgrade = false;
    }
    private void ClearSelection(Transform cardTransform)
    {
        if (cardTransform == null) return;
        var cfs = cardTransform.GetComponentInParent<CardFunctionScript>();
        var cs = cardTransform.GetComponentInParent<CardSelect>();
        cs?.ToggleHighlightAndScale(false);
        cfs?.ClearSelection(0f);
    }
    public void OnConfirmUpgrade()
    {
        _selectedCard.UpgradeCard();
        HideUpgradePreview();
        _chooseCardScript.gameObject.SetActive(false);
        RemainingUpgrades--;

        _remainingUpgradesText.text = RemainingUpgrades.ToString();

        _editPanelBackButton?.SetActive(RemainingUpgrades == _numUpgradesAllowed);
        ToggleButtonInteractable(CardState.FreeUpgradeMenu, RemainingUpgrades > 0);
    }
    private Card[] GetRandomUpgradeOptions()
    {
        var deckCards = PlayerDataManager.Instance?.GetPlayerDeck?.GetCardsInDeck;
        if (deckCards == null) return null;

        List<Card> temp = new();
        for (int i = 0; i < _numUpgradeCardOptions; i++)
        {
            Card selectedCard = null;
            int failCounter = 0;
            do
            {
                if (!int.TryParse($"{PlayerDataManager.Instance.GetGeneralSeed / 1000}{PlayerDataManager.Instance.GetCurrentNodeIndex.x}{RemainingUpgrades}{failCounter}", out int adjustedSeed))
                    Debug.LogWarning($"try parse failed on adjusted seed");
                UnityEngine.Random.InitState(adjustedSeed);

                int rng = UnityEngine.Random.Range(0, deckCards.Count);
                selectedCard = deckCards[rng];
                failCounter++;
            } while ((selectedCard == null || temp.Contains(selectedCard)) && failCounter < 50);
            
            if (selectedCard == null) continue;
            if (failCounter >= 50)
            {
                Debug.LogWarning($"Upgrade card choice while loop eccessive failures.");
                continue;
            }

            temp.Add(selectedCard);
        }
        return temp.ToArray();
    }
    public void OnStartRemoving() => StartEditing(CardState.FreeCardRemoval);
    private void StartEditing(CardState state)
    {
        if (_deckViewPanel == null)
        {
            Debug.LogError("DeckViewerScript instance is null.");
            return;
        }

        Action<int> onComplete = (i) =>
        {
            var allowedEdits = state == CardState.UpgradeMenu || state == CardState.FreeUpgradeMenu ? _numUpgradesAllowed : _numRemovalsAllowed;
            var remainingText = state == CardState.UpgradeMenu || state == CardState.FreeUpgradeMenu ? _remainingUpgradesText : _remainingRemovalsText;
            bool hasRequiredChips = CheckForRequiredChips(state);

            _editPanelBackButton?.SetActive(i == allowedEdits);
            ToggleButtonInteractable(state, i > 0 && hasRequiredChips);

            if (remainingText != null)
                remainingText.text = hasRequiredChips ? i.ToString() : "0";

            if (state == CardState.FreeUpgradeMenu)
                RemainingUpgrades = hasRequiredChips ? i : 0;
            else if (state == CardState.FreeCardRemoval)
                RemainingRemovals = hasRequiredChips ? i : 0;
        };

        _deckViewPanel?.gameObject?.SetActive(true);
        _deckViewPanel?.InitDeckViewer(onComplete, state);
    }

    private void ToggleButtonInteractable(CardState state, bool isActive)
    {
        var button = state == CardState.UpgradeMenu || state == CardState.FreeUpgradeMenu ? _upgradeButton : _removeButton;
        if (button == null) return;
        button.interactable = isActive;
    }
    public void OnRestOptionChosen(int restOption)
    {
        if (restOption >= Enum.GetNames(typeof(RestOptions)).Length)
            return;

        RestOptions option = (RestOptions)restOption;

        PlayerDataManager.Instance?.UpdateBuff(option, GetRestOptionVal(option));

        _restOptionsPanel.SetActive(false);
        gameObject?.SetActive(false);
        _onComplete?.Invoke();
    }

    private int GetRestOptionVal(RestOptions option)
    {
        switch (option)
        {
            case RestOptions.AP:
                return _apIncrease;
            case RestOptions.MaxHealth:
                return _maxHealthIncrease;
            case RestOptions.StartingHandSize:
                return _startingHandSizeIncrease;
        }
        return 0;
    }
    private bool CheckForRequiredChips(CardState state)
    {
        int balance = PlayerDataManager.Instance.GetBalance;

        switch (state)
        {
            case CardState.UpgradeMenu:
                if (PlayerDataManager.Instance == null) return false;
                int minUpgradeCost = int.MaxValue;

                for (int i = 0; i < PlayerDataManager.Instance.GetPlayerDeck.GetCardsInDeck.Count; i++)
                {
                    var card = PlayerDataManager.Instance.GetPlayerDeck.GetCardsInDeck[i];
                    if (card == null) continue;
                    if (card.GetShopCost >= minUpgradeCost) continue;
                    minUpgradeCost = card.GetShopCost;
                }
                return minUpgradeCost <= balance;
            case CardState.CardRemoval:
            case CardState.CardSwap:
                var deckEditController = FindFirstObjectByType<DeckEditingController>(FindObjectsInactive.Include);
                return deckEditController?.GetRemovalCost <= balance;
            case CardState.FreeUpgradeMenu:
            case CardState.FreeCardRemoval:
            default:
                return true;
        }
    }
}
