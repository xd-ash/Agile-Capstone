using UnityEngine;

public class ViewDeckButtonAlt : MonoBehaviour
{
    GameObject _deckViewWindow;

    private void OnEnable()
    {
        _deckViewWindow = FindObjectsByType<DeckViewerScript>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0].gameObject;
    }
    public void OnCLick()
    {
        _deckViewWindow?.gameObject?.SetActive(true);
    }
}
