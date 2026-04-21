using System;
using UnityEngine;
using UnityEngine.UI;

public enum RestOptions { AP = 0, MaxHealth = 1, HandSize = 2 }

public class CampNodeController : MonoBehaviour
{
    [SerializeField] private GameObject _restOptionsPanel;
    private DeckViewerScript _deckViewPanel;

    // buff values, make better &/or random?
    [SerializeField] private int _apIncrease = 2,
                                 _maxHealthIncrease = 5,
                                 _handSizeIncrease = 1;

    private Action _onComplete;

    public void InitCampNode(Action onComplete)
    {
        _onComplete = onComplete;

        _deckViewPanel = FindFirstObjectByType<DeckViewerScript>(FindObjectsInactive.Include);
    }

    public void DoOtherUpgrade()
    {
        Debug.Log("Non-CardUpgrade chosen.");
        gameObject?.SetActive(false);
        _onComplete?.Invoke();
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
        CardUpgradeController.Instance?.InitUpgradeController(_onComplete);

        gameObject?.SetActive(false);
    }
    public void OnOptionChosen(int restOption)
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
            case RestOptions.HandSize:
                return _handSizeIncrease;
        }
        return 0;
    }
}
