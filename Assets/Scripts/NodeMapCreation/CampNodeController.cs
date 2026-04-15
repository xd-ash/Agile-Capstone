using System;
using UnityEngine;
using UnityEngine.UI;

public class CampNodeController : MonoBehaviour
{
    private DeckViewerScript _deckViewPanel;

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
}
