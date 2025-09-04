using UnityEngine;

public class TileHoverScript : MonoBehaviour
{
    public GameObject tileInvis, tileSomeAlpha;

    private void Start()
    {
        tileInvis.SetActive(true);
        tileSomeAlpha.SetActive(false);
    }
    private void OnMouseEnter()
    {
        tileInvis.SetActive(false);
        tileSomeAlpha.SetActive(true);
    }
    private void OnMouseExit()
    {
        tileInvis.SetActive(true);
        tileSomeAlpha.SetActive(false);
    }
}
