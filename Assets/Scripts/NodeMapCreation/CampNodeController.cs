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
    //private int _remainingUpgrades;
    //private int _remainingRemovals;

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

        if (_remainingUpgradesText != null)
            _remainingUpgradesText.text = _numUpgradesAllowed.ToString();
        if (_remainingRemovalsText != null)
            _remainingRemovalsText.text = _numRemovalsAllowed.ToString();

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
    }
    public void OnCompleteCampNode()
    {
        _deckEditPanel?.SetActive(false);
        _restOptionsPanel?.SetActive(false);

        _onComplete?.Invoke();

        gameObject.SetActive(false);
    }

    public void OnStartUpgrading() => StartEditing(CardState.UpgradeMenu);
    public void OnStartRemoving() => StartEditing(CardState.CardRemoval);
    private void StartEditing(CardState state)
    {
        if (_deckViewPanel == null)
        {
            Debug.LogError("DeckViewerScript instance is null.");
            return;
        }

        Action<int> onComplete = (i) =>
        {
            var allowedEdits = state == CardState.UpgradeMenu ? _numUpgradesAllowed : _numRemovalsAllowed;
            var button = state == CardState.UpgradeMenu ? _upgradeButton : _removeButton;
            var remainingText = state == CardState.UpgradeMenu ? _remainingUpgradesText : _remainingRemovalsText;

            _editPanelBackButton?.SetActive(i == allowedEdits);
            if (button != null)
                button.interactable = i > 0;
            if (remainingText != null)
                remainingText.text = i.ToString();

            if (state == CardState.UpgradeMenu)
                RemainingUpgrades = i;
            else if (state == CardState.CardRemoval)
                RemainingRemovals = i;
        };

        _deckViewPanel?.gameObject?.SetActive(true);
        _deckViewPanel?.InitDeckViewer(onComplete, state);
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
