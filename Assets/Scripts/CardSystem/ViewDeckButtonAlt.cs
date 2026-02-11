using UnityEngine;

public class ViewDeckButtonAlt : MonoBehaviour
{
    public void OnCLick()
    {
        DeckViewerScript.Instance?.gameObject?.SetActive(true);
    }
}
