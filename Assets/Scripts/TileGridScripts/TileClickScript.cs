using UnityEngine;
using UnityEngine.Tilemaps;

public class TileClickScript : MonoBehaviour
{
    public Tilemap tileMap;
    public GameObject empty;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("test");
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var tpos = tileMap.WorldToCell(worldPoint);

            // Try to get a tile from cell position
            var tile = tileMap.GetTile(tpos);

            if (tile)
            {
                empty.transform.localPosition = tpos;
            }
        }
    }
}
