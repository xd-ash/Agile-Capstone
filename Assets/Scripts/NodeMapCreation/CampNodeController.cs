using System;
using UnityEngine;

public enum RestOptions { AP = 0, MaxHealth = 1, StartingHandSize = 2 }

public class CampNodeController : MonoBehaviour
{
    [SerializeField] private GameObject _restOptionsPanel;
    private DeckViewerScript _deckViewPanel;

    // buff values, make better &/or random?
    [SerializeField] private int _apIncrease = 1,
                                 _maxHealthIncrease = 5,
                                 _startingHandSizeIncrease = 1;

    private Action _onComplete;

    public void InitCampNode(Action onComplete)
    {
        _onComplete = onComplete;

        _deckViewPanel = FindFirstObjectByType<DeckViewerScript>(FindObjectsInactive.Include);
    }

    public void OnStartRest()
    {
        _restOptionsPanel.SetActive(true);
    }

    public void OnStartUpgrade()
    {
        if (_deckViewPanel == null)
        {
            Debug.LogError("DeckViewerScript instance is null.");
            return;
        }

        _deckViewPanel?.gameObject?.SetActive(true);
        _deckViewPanel.BuildDeckScrollViewContent(CardState.UpgradeMenu);

        Action onUpgradeComplete = () =>
        {
            _onComplete?.Invoke();
            gameObject.SetActive(false);
        };
        CardUpgradeController.Instance?.InitUpgradeController(onUpgradeComplete);
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
