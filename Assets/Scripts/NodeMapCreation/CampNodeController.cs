using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RestOptions { AP = 0, MaxHealth = 1, StartingHandSize = 2 }

public class CampNodeController : MonoBehaviour
{
    private DeckViewerScript _deckViewPanel;

    [Header("Edit Option")]
    [SerializeField] private GameObject _deckEditPanel;
    [SerializeField] private GameObject _editPanelBackButton;
    [SerializeField, Space(5)] private TextMeshProUGUI _remainingUpgradesText;
    [SerializeField] private TextMeshProUGUI _remainingRemovalsText;
    [SerializeField, Space(5)] private Button _upgradeButton;
    [SerializeField] private Button _removeButton;
    [SerializeField, Space(5)] private int _numUpgradesAllowed = 3;
    [SerializeField] private int _numRemovalsAllowed = 1;

    [Header("Rest Option")]
    [SerializeField] private GameObject _restOptionsPanel;
    [SerializeField] private int _apIncrease = 1,
                                 _maxHealthIncrease = 5,
                                 _startingHandSizeIncrease = 1;

    public static int RemainingUpgrades { get; private set; }
    public static int RemainingRemovals { get; private set; }

    private Action _onComplete;

    public void InitCampNode(Action onComplete)
    {
        _onComplete = onComplete;
        _deckViewPanel = FindFirstObjectByType<DeckViewerScript>(FindObjectsInactive.Include);

        RemainingUpgrades = _numUpgradesAllowed;
        RemainingRemovals = _numRemovalsAllowed;

        if (_remainingUpgradesText != null)
            _remainingUpgradesText.text = RemainingUpgrades.ToString();
        if (_remainingRemovalsText != null)
            _remainingRemovalsText.text = RemainingRemovals.ToString();
    }
    private bool CheckForRequiredChips(CardState state)
    {
        int balance = PlayerDataManager.Instance.GetBalance;

        switch (state)
        {
            case CardState.UpgradeMenu:
            case CardState.FreeUpgradeMenu:
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
            case CardState.FreeCardRemoval:
            case CardState.CardSwap:
                var deckEditController = FindFirstObjectByType<DeckEditingController>(FindObjectsInactive.Include);
                return deckEditController?.GetRemovalCost <= balance;
            default:
                return true;
        }
    }
    public void OnStartRest()
    {
        _restOptionsPanel?.SetActive(true);
    }
    public void OnStartEdit()
    {
        _deckEditPanel?.SetActive(true);
    }
    public void OnCompleteCampNode()
    {
        _deckEditPanel?.SetActive(false);
        _restOptionsPanel?.SetActive(false);

        _onComplete?.Invoke();

        gameObject.SetActive(false);
    }

    public void OnStartUpgrading() => StartEditing(CardState.FreeUpgradeMenu);
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
}
