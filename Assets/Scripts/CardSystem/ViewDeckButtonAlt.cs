using UnityEngine;

public class ViewDeckButtonAlt : MonoBehaviour
{
    DeckViewerScript _deckViewWindow;

    private void OnEnable()
    {
        _deckViewWindow = FindObjectsByType<DeckViewerScript>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
    }
    public void OnCLick()
    {
        _deckViewWindow?.gameObject?.SetActive(true);
        _deckViewWindow.BuildDeckScrollViewContent();
    }
}
