using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static IsoMetricConversions;
using WFC;

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
    public static MapCreator instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        _moduleSet.SetNeighbours();
    }

    private Tilemap _tilemap;

    [SerializeField] private Vector2Int _mapSize;
    private byte[,] _map;
    private List<MapLocation> _directions = new List<MapLocation>() { new MapLocation(1,0),
                                                                      new MapLocation(0,1),
                                                                      new MapLocation(-1,0),
                                                                      new MapLocation(0,-1) };
    [Header("Placeholder Obstacle/Enemy Spawn")]
    [SerializeField] private GameObject _placeholderObstacle;
    [SerializeField] private GameObject _rangeEnemyPlaceholder;
    [SerializeField] private GameObject _meleeEnemyPlaceholder;
    [SerializeField] private GameObject _playerPlaceholder;

    [Header("Tile Module Set")]
    [SerializeField] private TileSet _moduleSet;

    public byte[,] GetByteMap => _map;
    public Vector2Int GetMapSize => _mapSize;
    public List<MapLocation> GetDirections => _directions;

    private void OnEnable()
    {
        _tilemap = transform.Find("MainTileMap").GetComponent<Tilemap>();
    }

    private void Start()
    {
        _tilemap.CompressBounds();

        int totalNonObstacleTiles;
        int failSafeCount = -1;

        do
        {
            _map = new byte[_mapSize.x, _mapSize.y];
            int moduleWidth = _moduleSet.GetTrueModuleWidth;
            TileElement[,] environmentMap = null;
            totalNonObstacleTiles = 0;

            int wfcFailCounter = -1;
            do
            {
                environmentMap = TileWaveFunctionCollapse.WFCGenerate(_moduleSet.Modules,
                    new Vector2Int(_mapSize.x / moduleWidth, _mapSize.y / moduleWidth));
                wfcFailCounter++;
                if (wfcFailCounter >= 100)
                    Debug.Log("Excessive map generation fails from player/enemy spawning");
            } while ((environmentMap == null || TileWaveFunctionCollapse.CheckCanSpawnPlayer || TileWaveFunctionCollapse.CheckCanSpawnEnemy) && wfcFailCounter < 100);

            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for (int y = 0; y < _map.GetLength(1); y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    var environmentElement = environmentMap[x / moduleWidth, y / moduleWidth];
                    var module = environmentElement.GetSelectedModule;
                    var tile = module.GetTrueTiles[module.GetTrueTileIndex(x % moduleWidth, y % moduleWidth)];

                    if (tile == null)
                        _map[x, y] = 0;
                    else
                        switch (tile.name)
                        {
                            case "P_Tile":
                                _map[x, y] = 1;
                                break;
                            case "O_Tile":
                                _map[x, y] = 2;
                                break;
                            case "EM_Tile":
                                _map[x, y] = 3;
                                break;
                            case "ER_Tile":
                                _map[x, y] = 4;
                                break;
                        }

                    if (_map[x, y] != 2)
                        totalNonObstacleTiles++;
                }
            }

            failSafeCount++;
            if (failSafeCount >= 100)
                Debug.LogError("Excessive map generation fails from unreachable positions");
        } while (!CheckMapForTruePath(totalNonObstacleTiles) && failSafeCount < 100);

        for (int x = 0; x < _map.GetLength(0); x++)
        {
            for (int y = 0; y < _map.GetLength(1); y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);

                SpawnTileContents(_map[x, y], gridPos);
                _tilemap.SetTileFlags((Vector3Int)gridPos, TileFlags.None);
            }
        }
    }
    //Check each valid tile and determine if there are unreachable positions
    public bool CheckMapForTruePath(int totalNonObstacleTiles)
    {
        List<Vector2Int> validLocs = new();
        Vector2Int startLoc = new Vector2Int(-1,-1);

        for (int x = 0; x < _map.GetLength(0); x++)
        {
            if (startLoc.x != -1) break;

            for (int y = 0; y < _map.GetLength(1); y++)
            {
                if (_map[x,y] != 2)
                {
                    startLoc = new Vector2Int(x, y);
                    break;
                }
            }
        }

        CheckNeighbours(startLoc, ref validLocs);
        bool result = totalNonObstacleTiles == validLocs.Count;
        if (!result)
            PlayerDataManager.Instance.GetRandomSeed();
        return result;
    }
    //recursive method to check neighboring tiles and add locations to list of valid locations
    private void CheckNeighbours(Vector2Int tilePos, ref List<Vector2Int> validLocs)
    {
        if (!validLocs.Contains(tilePos))
            validLocs.Add(tilePos);

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if ((x == 0 && y == 0) || (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)) // setting up for neighbors, but not diags (1,1)
                    continue;

                Vector2Int neighborPos = new Vector2Int(tilePos.x + x, tilePos.y + y);

                if (neighborPos.x < 0 || neighborPos.y < 0 || neighborPos.x > _map.GetLength(0) - 1 || neighborPos.y > _map.GetLength(1) - 1)
                    continue;

                if (_map[neighborPos.x, neighborPos.y] != 2 && !validLocs.Contains(neighborPos))
                {
                    validLocs.Add(neighborPos);
                    CheckNeighbours(neighborPos, ref validLocs);
                }
            }
        }
    }
    public void UpdateUnitPositionByteMap(Vector2Int startPos, Vector2Int endPos, Unit unit)
    {
        _map[startPos.x, startPos.y] = 0;
        _map[endPos.x, endPos.y] = unit.GetTeam == Team.Friendly ? (byte)1 : (byte)3;
    }
    public void UpdateUnitPositionByteMap(Vector2Int deathPos, Unit unit)
    {
        _map[deathPos.x, deathPos.y] = 0;
    }

    private void SpawnTileContents(int byteIndicator, Vector2Int mapPos)
    {
        Vector3 truePos = ConvertToIsometricFromGrid(mapPos);
        GameObject objToSpawn = GetGameObjectFromByte(byteIndicator);

        if (byteIndicator == 4)
            _map[mapPos.x, mapPos.y] = 3; // after range enemy spawned, swap byte back to general enemy value

        if (objToSpawn == null)
        {
            if (byteIndicator != 0)
                Debug.LogError($"No Prefab found for byte indicator: {byteIndicator}");
            return;
        }

        // z pos adjusted with y value to allow for easy layering of sprites (.01f holds no signifigance, just to make it small)
        Vector3 adjustedPos = new Vector3(truePos.x, truePos.y, truePos.y * 0.01f);

        GameObject newObj = Instantiate(objToSpawn, Vector3.zero, Quaternion.identity, transform);
        newObj.transform.localPosition = adjustedPos;
    }
    private GameObject GetGameObjectFromByte(int indicator)
    {
        switch (indicator)
        {
            case 1:
                return _playerPlaceholder;
            case 2:
                return _placeholderObstacle;
            case 3:
                return _meleeEnemyPlaceholder;
            case 4:
                return _rangeEnemyPlaceholder;
            default:
                return null;
        }
    }
}
