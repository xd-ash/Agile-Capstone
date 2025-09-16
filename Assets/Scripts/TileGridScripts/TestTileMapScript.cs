using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestTileMapScript : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject tileTEst;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //TileBase tb = tilemap.GetTile(Vector3Int.zero);
        //tileTEst.transform.position = tb.GetTileData().position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
