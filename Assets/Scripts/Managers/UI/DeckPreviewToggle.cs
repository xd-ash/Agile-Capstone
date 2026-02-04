using UnityEngine;

public class DeckPreviewToggle : MonoBehaviour
{
    [SerializeField] private GameObject deckPanel;

    public void Toggle()
    {
        deckPanel.SetActive(!deckPanel.activeSelf);
    }
}