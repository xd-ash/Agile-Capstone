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
        if (ShopConfirmPopup.Instance != null && ShopConfirmPopup.Instance.gameObject.activeInHierarchy) return;

        _deckViewWindow?.gameObject?.SetActive(true);
        _deckViewWindow.BuildDeckScrollViewContent();
    }
}
