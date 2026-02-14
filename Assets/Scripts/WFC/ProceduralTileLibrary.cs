using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileLibrary", menuName = "WFC/New Tile Library")]
public class ProceduralTileLibrary : ScriptableObject
{
    [SerializeField] private TileData[] _tileLibrary;

    public int GetIndicatorFromName(string tileName)
    {
        TileData tile = GetTileFromLibrary(tileName);
        if (tile == null)
            return 0;

        return tile.GetByteIndicator;
    }
    public GameObject GetGOFromIndicator(int indicator)
    {
        TileData tile = GetTileFromLibrary(indicator);
        if (tile == null)
            return null;

        return tile.GetTileContentPrefab;
    }
   
    public TileBase GetTileBaseFromName(string tileName)
    {
        TileData tile = GetTileFromLibrary(tileName);
        if (tile == null)
            return null;

        return tile.GetTileBase;
    }

    //internal tile data grabbers
    private TileData GetTileFromLibrary(string tileName)
    {
        if (_tileLibrary.Length == 0)
        {
            Debug.LogError($"Tile Library length is 0. Please create some dang tile data.");
            return null;
        }
        TileData tileData = null;
        foreach (var tile in _tileLibrary)
            if (tile.GetTileName == tileName)
            {
                tileData = tile;
                break;
            }
        if (tileData == null)
        {
            Debug.LogError($"Tilename ({tileName}) not found in tile library");
            return null;
        }
        return tileData;
    }
    private TileData GetTileFromLibrary(int indicator)
    {
        if (indicator == 0) return null;

        if (_tileLibrary.Length == 0)
        {
            Debug.LogError($"Tile Library length is 0. Please create some dang tile data.");
            return null;
        }
        TileData tileData = null;
        foreach (var tile in _tileLibrary)
            if (tile.GetByteIndicator == indicator)
            {
                tileData = tile;
                break;
            }
        if (tileData == null)
        {
            Debug.LogError($"Tile indicator ({indicator}) not found in tile library");
            return null;
        }
        return tileData;
    }

    public bool SetTileNamesOnGUI()
    {
        bool tmp = false;
        for (int i = 0; i < _tileLibrary.Length; i++)
        {
            var td = _tileLibrary[i];
            string eNumber = $"Element {i}";

            if (td.GetTileBase == null && td.GetTileName != eNumber)
            {
                td.SetTileName(eNumber);
                tmp = true;
                continue;
            }
            if (td.GetTileBase == null) continue;

            if (td.GetTileName != td.GetTileBase.name)
            {
                td.SetTileName();
                tmp = true;
            }
        }
        return tmp;
    }

    [System.Serializable]
    private class TileData
    {
        [SerializeField, HideInInspector] private string _tileName;
        [SerializeField] private TileBase _tileBase;
        [SerializeField] private int _byteIndicator;
        [SerializeField] private GameObject _tileContentPrefab;

        public TileBase GetTileBase => _tileBase;
        public int GetByteIndicator => _byteIndicator;
        public GameObject GetTileContentPrefab => _tileContentPrefab;
        public string GetTileName => _tileName;
        public void SetTileName(string fallBackName = "")
        {
            _tileName = fallBackName == string.Empty ? _tileBase.name : fallBackName;
        }
    }
}
