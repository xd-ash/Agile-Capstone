using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static IsoMetricConversions;
using WFC;
using System.Linq;
using System;

public class MapLocation
{
    public int x;
    public int y;

    public MapLocation(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public Vector2Int ToVector()
    {
        return new Vector2Int(x, y);
    }

    public static MapLocation operator +(MapLocation a, MapLocation b)
       => new MapLocation(a.x + b.x, a.y + b.y);

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        else
            return x == ((MapLocation)obj).x && y == ((MapLocation)obj).y;
    }

    public override int GetHashCode()
    {
        return 0;
    }
}
public class MapCreator : MonoBehaviour
{
    //Singleton setup
    public static MapCreator Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        _tileLibrary = Resources.Load<ProceduralTileLibrary>("Libraries/TileDataLibrary");
        _tilemapSOLibrary = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTileMapSOLibrary");
        _tileMapPos = transform.Find("TileMapPos");
    }

    private Transform _tileMapPos;
    private ProceduralTileLibrary _tileLibrary;
    private CustomTileMapSOLibrary _tilemapSOLibrary;

    [SerializeField] private Vector2Int _mapSize;

    public Vector2Int GetMapSize => _mapSize;

    public byte[,] CreateMap()
    {
        if (_tileLibrary == null)
        {
            Debug.LogError($"Tile Library is Null");
            return null;
        }

        UnityEngine.Random.InitState(PlayerDataManager.Instance.GetGeneralSeed);
        int rngMap = UnityEngine.Random.Range(0, _tilemapSOLibrary.GetSOsInProject.Count);
        var so = _tilemapSOLibrary.GetSOsInProject[rngMap];

        var tilemap = SetUpTileMapPrefab(so);
        TileBase[,] tileBaseMap = so.GetTileBaseMap;

        tilemap.CompressBounds();

        var emptyTilePositions = new List<Vector2Int>();
        var map = new byte[_mapSize.x, _mapSize.y];

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                var tile = tileBaseMap[x, y];

                if (tile == null)
                    map[x, y] = 0;
                else
                    map[x, y] = (byte)_tileLibrary.GetIndicatorFromName(tile.name);

                if (map[x, y] != 2 && map[x, y] != 5)
                    emptyTilePositions.Add(gridPos);
            }
        }

        GenerateUnitPositions(map, emptyTilePositions);
        
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);

                SpawnTileContents(map, map[x, y], gridPos);
                tilemap.SetTileFlags((Vector3Int)gridPos, TileFlags.None);
            }
        }

        return map;
    }
    private Tilemap SetUpTileMapPrefab(CustomTileMapSO so)
    {
        var gridPrefab = Instantiate<GameObject>(so.GetMainTileMap);
        var t = gridPrefab.GetComponentInChildren<Tilemap>().transform;
        t.parent = transform;
        t.localPosition = _tileMapPos.localPosition;
        return t.GetComponent<Tilemap>();
    }
    private void SpawnTileContents(byte[,] map, int byteIndicator, Vector2Int mapPos)
    {
        if (byteIndicator != 3 && byteIndicator != 4 && byteIndicator != 1) return; //quick fix for WFC removal. Only spawn units

        Vector3 truePos = ConvertToIsometricFromGrid(mapPos);
        GameObject objToSpawn = _tileLibrary.GetGOFromIndicator(byteIndicator);

        if (byteIndicator == 4)
            map[mapPos.x, mapPos.y] = 3; // after range enemy spawned, swap byte back to general enemy value

        if (objToSpawn == null)
        {
            if (byteIndicator != 0)
                Debug.LogError($"No Prefab found for byte indicator: {byteIndicator}");
            return;
        }

        // z pos adjusted with y value to allow for easy layering of sprites (.01f holds no signifigance, just to make it small)
        //Vector3 adjustedPos = new Vector3(truePos.x, truePos.y, truePos.y * 0.01f);

        GameObject newObj = Instantiate(objToSpawn, Vector3.zero, Quaternion.identity, transform);
        newObj.transform.localPosition = truePos;

        if (newObj.TryGetComponent(out Unit unit))
            ByteMapController.Instance.InitUnitPosition(unit, mapPos);
    }
    private void GenerateUnitPositions(byte[,] map, List<Vector2Int> emptyPositions)
    {
        int players = PlayerDataManager.Instance.GetCurrMapNodeData.maxPlayersAllowed;
        int enemies = PlayerDataManager.Instance.GetCurrMapNodeData.maxEnemiesAllowed;

        int[] selectedPositionIndexes = new int[players + enemies];

        for (int i = 0; i < players + enemies; i++)
        {
            int index = -1;
            do
            {
                index = UnityEngine.Random.Range(0, emptyPositions.Count);
            } while (selectedPositionIndexes.Contains(index));
            selectedPositionIndexes[i] = index;
            var pos = emptyPositions[index];

            if (i < players)
                map[pos.x, pos.y] = 1;
            else
                map[pos.x, pos.y] = (byte)UnityEngine.Random.Range(3, 5); // randomly choose enemy type (3 or 4) on enemy spawn
        }
    }
}
