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

        _moduleSet.SetNeighbours();
        _tileLibrary = Resources.Load<ProceduralTileLibrary>("Libraries/TileDataLibrary");
        _tilemap = transform.Find("MainTileMap").GetComponent<Tilemap>();
    }

    private Tilemap _tilemap;
    private ProceduralTileLibrary _tileLibrary;

    [SerializeField] private Vector2Int _mapSize;

    [Header("Tile Module Set")]
    [SerializeField] private TileSet _moduleSet;

    public Vector2Int GetMapSize => _mapSize;

    public byte[,] CreateMap()
    {
        if (_tileLibrary == null)
        {
            Debug.LogError($"Tile Library is Null");
            return null;
        }

        _tilemap.CompressBounds();

        int failSafeCount = -1;
        var emptyTilePositions = new List<Vector2Int>();
        byte[,] map;

        do
        {
            map = new byte[_mapSize.x, _mapSize.y];
            int moduleWidth = _moduleSet.GetTrueModuleWidth;
            TileElement[,] environmentMap = environmentMap = TileWaveFunctionCollapse.WFCGenerate(_moduleSet.Modules,
                    new Vector2Int(_mapSize.x / moduleWidth, _mapSize.y / moduleWidth)); ;

            emptyTilePositions.Clear();

            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    var environmentElement = environmentMap[x / moduleWidth, y / moduleWidth];
                    var module = environmentElement.GetSelectedModule;
                    var tile = module.GetTrueTiles[module.GetTrueTileIndex(x % moduleWidth, y % moduleWidth)];

                    if (tile == null)
                        map[x, y] = 0;
                    else
                        map[x, y] = (byte)_tileLibrary.GetIndicatorFromName(tile.name);

                    if (map[x, y] != 2 && map[x, y] != 5)
                        emptyTilePositions.Add(gridPos);
                }
            }

            failSafeCount++;
            if (failSafeCount >= 100)
                Debug.LogError("Excessive map generation fails from unreachable positions");
        } while (!CheckMapForTruePath(map, emptyTilePositions.Count) && failSafeCount < 100);

        GenerateUnitPositions(map, emptyTilePositions);

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);

                SpawnTileContents(map, map[x, y], gridPos);
                _tilemap.SetTileFlags((Vector3Int)gridPos, TileFlags.None);
            }
        }

        return map;
    }
    //Check each valid tile and determine if there are unreachable positions
    public bool CheckMapForTruePath(byte[,] map, int totalNonObstacleTiles)
    {
        List<Vector2Int> validLocs = new();
        Vector2Int startLoc = new Vector2Int(-1,-1);

        for (int x = 0; x < map.GetLength(0); x++)
        {
            if (startLoc.x != -1) break;

            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x,y] != 2 && map[x,y] != 5) // obstacle indicators
                {
                    startLoc = new Vector2Int(x, y);
                    break;
                }
            }
        }

        CheckNeighbours(map, startLoc, ref validLocs); 
        bool result = totalNonObstacleTiles == validLocs.Count;
        if (!result)
            PlayerDataManager.Instance.GetRandomSeed();// regen seed only after fail
        return result;
    }

    //recursive method to check neighboring tiles and add locations to list of valid locations
    private void CheckNeighbours(byte[,] map, Vector2Int tilePos, ref List<Vector2Int> validLocs)
    {
        if (!validLocs.Contains(tilePos))
            validLocs.Add(tilePos);

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if ((x == 0 && y == 0) || (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)) // setting up for neighbors, but not diags or same tile ((-)1,(-)1), (0,0)
                    continue;

                Vector2Int neighborPos = new Vector2Int(tilePos.x + x, tilePos.y + y);

                if (neighborPos.x < 0 || neighborPos.y < 0 || neighborPos.x > map.GetLength(0) - 1 || neighborPos.y > map.GetLength(1) - 1)
                    continue;

                //if (_map[neighborPos.x, neighborPos.y] != 2 && _map[neighborPos.x, neighborPos.y] != 5 && !validLocs.Contains(neighborPos))
                if ((map[neighborPos.x, neighborPos.y] == 0 || map[neighborPos.x, neighborPos.y] == 1 || map[neighborPos.x, neighborPos.y] == 3) && 
                    !validLocs.Contains(neighborPos))
                {
                    validLocs.Add(neighborPos);
                    CheckNeighbours(map, neighborPos, ref validLocs);
                }
            }
        }
    }

    private void SpawnTileContents(byte[,] map, int byteIndicator, Vector2Int mapPos)
    {
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
