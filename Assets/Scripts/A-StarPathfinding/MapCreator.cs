using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using WFC;
using static IsoMetricConversions;
using static UnityEditor.PlayerSettings;

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

    public byte[,] CreateMap(string forcedMapSOName = "")
    {
        if (_tileLibrary == null)
        {
            Debug.LogError($"Tile Library is Null");
            return null;
        }

        var so = PlayerDataManager.Instance.GetCurrMapNodeData.selectedMap;
        if (so == null)
        {
            Debug.LogError("SO is null");
            return null;
        }
        var tilemap = SetUpTileMapPrefab(so);
        TileBase[,] tileBaseMap = so.GenerateTileBaseMap(_mapSize);

        tilemap.CompressBounds();

        var playerSpawnPositions = new List<Vector2Int>();
        var enemySpawnPositions = new List<Vector2Int>();
        var emptyPositions = new List<Vector2Int>();
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

                if (map[x, y] == 1)
                    playerSpawnPositions.Add(gridPos);
                else if (map[x, y] == 3)
                    enemySpawnPositions.Add(gridPos);
                else if (map[x, y] == 0)
                    emptyPositions.Add(gridPos);
            }
        }

        int players = PlayerDataManager.Instance.GetCurrMapNodeData.maxPlayersAllowed;
        int enemies = PlayerDataManager.Instance.GetCurrMapNodeData.maxEnemiesAllowed;

        //check if tilemap prefab had enough spawners for the number of units and sidestep tilebase system if failed
        if (playerSpawnPositions.Count < players)
            SidestepUnitSpawnerTileBasesOnFail(ref map, players, playerSpawnPositions, emptyPositions, 1);
        if (enemySpawnPositions.Count < enemies)
            SidestepUnitSpawnerTileBasesOnFail(ref map, enemies, enemySpawnPositions, emptyPositions, 3);
        
        GenerateUnitPositions(ref map, players, playerSpawnPositions);
        GenerateUnitPositions(ref map, enemies, enemySpawnPositions);

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
        if (so == null)
        {
            Debug.LogError("TileMap SO null");
            return null;
        }
        var gridPrefab = Instantiate<GameObject>(so.GetMainTileMap, transform);

        var tilemap = gridPrefab.GetComponentInChildren<Tilemap>();
        tilemap.transform.parent = transform;
        tilemap.transform.SetLocalPositionAndRotation(_tileMapPos.localPosition, Quaternion.identity);
        tilemap.transform.localScale = Vector3.one;
        Destroy(gridPrefab);

        tilemap.enabled = true;
        tilemap.GetComponent<TileMapObjRepositioner>().enabled = true;

        return tilemap;
    }
    private void SpawnTileContents(byte[,] map, int byteIndicator, Vector2Int mapPos)
    {
        if (byteIndicator != 3 && byteIndicator != 1) return; //quick fix for WFC removal. Only spawn units

        Vector3 truePos = ConvertToIsometricFromGrid(mapPos);
        GameObject[] objs = _tileLibrary.GetGOFromIndicator(byteIndicator);
        GameObject objToSpawn = objs[0];

        //grab random prefab from array if length > 1 (replace with proper enemy determination system)
        if (objs.Length > 1)
        {
            UnityEngine.Random.InitState(PlayerDataManager.Instance.GetGeneralSeed - int.Parse($"{mapPos.x}{mapPos.y}"));
            int rng = UnityEngine.Random.Range(0, objs.Length);
            objToSpawn = objs[rng];
        }

        if (objToSpawn == null)
        {
            if (byteIndicator != 0)
                Debug.LogError($"No Prefab found for byte indicator: {byteIndicator}");
            return;
        }

        GameObject newObj = Instantiate(objToSpawn, Vector3.zero, Quaternion.identity, transform);
        newObj.transform.localPosition = truePos;

        if (newObj.TryGetComponent(out Unit unit))
            ByteMapController.Instance.InitUnitPosition(unit, mapPos);
    }
    private void GenerateUnitPositions(ref byte[,] map, int numUnits, List<Vector2Int> unitSpawnPoints)
    {
        List<Vector2Int> selectedUnitSpawns = new();

        for (int i = 0; i < numUnits; i++)
        {
            int index = -1;
            Vector2Int pos;
            do
            {
                index = UnityEngine.Random.Range(0, unitSpawnPoints.Count);
                pos = unitSpawnPoints[index];
            } while (selectedUnitSpawns.Contains(pos));
            selectedUnitSpawns.Add(pos);
        }

        //reset non-selected spawn positions to empty tile/byte indicators
        for (int i = 0; i < unitSpawnPoints.Count; i++)
        {
            var gridPos = unitSpawnPoints[i];
            if (selectedUnitSpawns.Contains(gridPos)) continue; //ignore selected spawn positions
            map[gridPos.x, gridPos.y] = 0;
        }
    }
    private void SidestepUnitSpawnerTileBasesOnFail(ref byte[,] map, int numUnits, List<Vector2Int> unitPositions, List<Vector2Int> emptyPositions, int unitIndicator)
    {
        int diff = numUnits - unitPositions.Count;
        for (int i = 0; i < diff; i++)
        {
            Vector2Int tempPos;
            do
            {
                UnityEngine.Random.InitState(PlayerDataManager.Instance.GetGeneralSeed);
                var rng = UnityEngine.Random.Range(0, emptyPositions.Count);
                tempPos = emptyPositions[rng];
            } while (unitPositions.Contains(tempPos));
            unitPositions.Add(tempPos);
            emptyPositions.Remove(tempPos);
            map[tempPos.x, tempPos.y] = (byte)unitIndicator;
        }
        Debug.LogWarning("Unit spawner system failed. Likely due to \"byte\" tilemap containing less \"spawner\" tilebases than required units for this node. " +
            "\n(Generally need at least 1 player spawn and at least 3 enemy spawns per map prefab)");
    }
}
