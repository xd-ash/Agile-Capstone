using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ByteTileMapHelper : MonoBehaviour
{
    [SerializeField, HideInInspector] private Tilemap _tileMap;
    [SerializeField, HideInInspector] private CustomTileMapSO _so;
    [SerializeField, HideInInspector] private Vector2Int _mapsize;

    public void InitHelper(CustomTileMapSO so, Vector2Int mapSize)
    {
        _tileMap = GetComponent<Tilemap>();
        _so = so;
        _mapsize = mapSize;
    }

    public void GenerateTileBaseMap()
    { 
        var map = new TileBase[_mapsize.x, _mapsize.y];
        if (_tileMap == null)
            _tileMap = GetComponent<Tilemap>();
        _tileMap.CompressBounds();

        //x & y start at 1 because of the "border" that is present in the tilemap prefab to help frame the grid for editing
        for (int x = 1; x <= _mapsize.x; x++)
            for (int y = 1; y <= _mapsize.y; y++)
                map[x - 1, y - 1] = _tileMap.GetTile(new Vector3Int(x, y));

        _so.SetTileBaseMap(map);
        EditorUtility.SetDirty(_so);
        AssetDatabase.SaveAssetIfDirty(_so);
    }
}
