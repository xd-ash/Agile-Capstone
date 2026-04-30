using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static IsoMetricConversions;

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

        var so = PlayerDataManager.Instance.GetCurrCombatNodeData.selectedMap;
        if (so == null)
        {
            so = _tilemapSOLibrary.GetSOsInProject[0];
            if (so == null)
            {
                Debug.LogError("SO is null");
                return null;
            }
        }
        var tilemap = SetUpTileMapPrefab(so);
        TileBase[,] tileBaseMap = so.GenerateTileBaseMap(_mapSize);

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
                else if (map[x, y] == 3 || map[x, y] == 6 || map[x, y] == 7)
                    enemySpawnPositions.Add(gridPos);
                else if (map[x, y] == 0)
                    emptyPositions.Add(gridPos);
            }
        }

        int players = 1; // PlayerDataManager.Instance.GetCurrCombatNodeData.maxPlayersAllowed;
        int enemies = PlayerDataManager.Instance.GetCurrCombatNodeData.maxEnemiesAllowed;

        //check if tilemap prefab had enough spawners for the number of units and sidestep tilebase system if failed
        if (playerSpawnPositions.Count < players)
            SidestepUnitSpawnerTileBasesOnFail(ref map, players, playerSpawnPositions, emptyPositions, 1);
        if (enemySpawnPositions.Count < enemies)
            SidestepUnitSpawnerTileBasesOnFail(ref map, enemies, enemySpawnPositions, emptyPositions, 3);

        GenerateUnitPositions(ref map, players, playerSpawnPositions);
        GenerateUnitPositions(ref map, enemies, enemySpawnPositions);

        _enemiesSpawned.Clear();

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);

                SpawnUnits(map, map[x, y], gridPos);
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

    private Dictionary<UnitSO, int> _enemiesSpawned = new();
    private void SpawnUnits(byte[,] map, int byteIndicator, Vector2Int mapPos)
    {
        if (byteIndicator == 2 || byteIndicator == 5 || byteIndicator == 0) return; // quick fix for WFC removal. 2 & 5 are obstacle tiles (2 is full cover,
                                                                                    // 5 is half-cover which isn't really implemented yet)
        
        Vector3 truePos = ConvertToIsometricFromGrid(mapPos);
        GameObject unitPrefab = null;
        string nameAddition = string.Empty;

        if (byteIndicator == 1)
            unitPrefab = UnitLibrary.GetPlayerUnitPrefab();
        else
        {
            if (byteIndicator != 3)
                map[mapPos.x, mapPos.y] = 3; //reset any possible accidental "specific" enemy spawn tiles to the generic enemy indicator

            var selectedEnemies = PlayerDataManager.Instance.GetCurrCombatNodeData.selectedEnemies;
            if (selectedEnemies == null)
                Debug.LogError($"Selected Enemies is null");
            var enemy = selectedEnemies[_enemiesSpawned.Count == 0 ? 0 : _enemiesSpawned.Values.Sum()];
            unitPrefab = UnitLibrary.GetUnitPrefab(enemy);

            if (unitPrefab != null)
            {
                if (_enemiesSpawned.ContainsKey(enemy))
                    _enemiesSpawned[enemy] = _enemiesSpawned[enemy] + 1;
                else
                    _enemiesSpawned.Add(enemy, 1);

                nameAddition = _enemiesSpawned[enemy].ToString();
            }
        }

        if (unitPrefab == null)
        {
            if (byteIndicator != 0)
                Debug.LogError($"No Prefab found for byte indicator: {byteIndicator}");
            return;
        }

        GameObject newObj = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity, transform);
        newObj.transform.localPosition = truePos;
        newObj.name = $"{newObj.name.Split('(')[0]} {nameAddition}";

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

            int failCount = 0;
            do
            {
                UnityEngine.Random.InitState(PlayerDataManager.Instance.GetGeneralSeed - failCount);
                index = UnityEngine.Random.Range(0, unitSpawnPoints.Count);
                pos = unitSpawnPoints[index];
                failCount++;
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
