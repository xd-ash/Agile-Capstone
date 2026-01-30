using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static IsoMetricConversions;
using WFC;

namespace AStarPathfinding
{
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
        }

        private Tilemap _tilemap;

        [SerializeField] private Vector2Int _mapSize;
        private byte[,] _map;
        private List<MapLocation> _directions = new List<MapLocation>() { new MapLocation(1,0),
                                                                          new MapLocation(0,1),
                                                                          new MapLocation(-1,0),
                                                                          new MapLocation(0,-1) };
        [Header("Placeholder Obstacle/Enemy Spawn")]
        [SerializeField] private List<Vector2Int> obstLocationList;
        [SerializeField] private List<Vector2Int> enemyLocationList;
        [SerializeField] private GameObject _placeholderObstacle;
        [SerializeField] private GameObject _rangeEnemyPlaceholder;
        [SerializeField] private GameObject _meleeEnemyPlaceholder;
        [SerializeField] private GameObject _playerPlaceholder;
        [SerializeField] private int _maxEnemiesToSpawn = 2;
        [SerializeField] private int _maxPlayersToSpawn = 1;
        int _tempEnemyCounter = 0;
        int _tempPlayerCounter = 0;

        [SerializeField] private EnvironmentTileSet _moduleSet;

        // Getters
        public byte[,] GetByteMap => _map;
        public Vector2Int GetMapSize => _mapSize;
        public List<MapLocation> GetDirections => _directions;

        private void OnEnable()
        {
            _tilemap = transform.Find("MainTileMap").GetComponent<Tilemap>();
        }

        private void Start()
        {
            _map = new byte[_mapSize.x, _mapSize.y];

            int moduleWidth = _moduleSet.GetTrueModuleWidth;
            var environmentMap = WaveFunctionCollapse.WFCGenerate(_moduleSet.Modules, new Vector2Int(_mapSize.x / moduleWidth, _mapSize.y / moduleWidth));
            _tilemap.CompressBounds();

            _tempEnemyCounter = 0;
            _tempPlayerCounter = 0;

            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for (int y = 0; y < _map.GetLength(1); y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    var environmentElement = environmentMap[x / moduleWidth, y / moduleWidth];
                    var module = environmentElement.GetSelectedModule as EnvironmentTileModule;
                    var tile = module.GetTrueTiles[module.GetTrueTileIndex(x % moduleWidth, y % moduleWidth)];

                    if (tile != null)
                        switch (tile.name)
                        {
                            case "ER_Tile":
                                _map[x, y] = 4;
                                break;
                            case "EM_Tile":
                                _map[x, y] = 3;
                                break;
                            case "O_Tile":
                                _map[x, y] = 2;
                                break;
                            case "P_Tile":
                                _map[x, y] = 1;
                                break;
                        }
                    else
                        _map[x, y] = 0;

                    //if (tile != null)
                        //Debug.Log($"{tile.name} - {_map[x,y]} @ ({x},{y})");

                    SpawnTileContents(_map[x, y], gridPos);

                    _tilemap.SetTileFlags((Vector3Int)gridPos, TileFlags.None);
                }
            }
        }
        public void UpdateUnitPositionByteMap(Vector2Int startPos, Vector2Int endPos, Unit unit)
        {
            _map[startPos.x, startPos.y] = 0;
            _map[endPos.x, endPos.y] = unit.GetTeam == Team.Friendly ? (byte)1 : (byte)3;
        }

        private void SpawnTileContents(int byteIndicator, Vector2Int mapPos)
        {
            Vector3 truePos = ConvertToIsometricFromGrid(mapPos);
            GameObject objToSpawn = null;
            if (byteIndicator == 2)
                objToSpawn = _placeholderObstacle;
            else if (byteIndicator == 4)
            {
                if (_tempEnemyCounter < _maxEnemiesToSpawn)
                {
                    objToSpawn = _rangeEnemyPlaceholder;
                    _tempEnemyCounter++;
                    _map[mapPos.x, mapPos.y] = 3; // after specific enemy spawned, swap byte back to general enemy value
                }
                else// if max enemies spawned reset byte to 0
                    _map[mapPos.x, mapPos.y] = 0;
            }
            else if (byteIndicator == 3)
            {
                if (_tempEnemyCounter < _maxEnemiesToSpawn)
                {
                    objToSpawn = _meleeEnemyPlaceholder;
                    _tempEnemyCounter++;
                    _map[mapPos.x, mapPos.y] = 3;// after specific enemy spawned, swap byte back to general enemy value
                }
                else// if max enemies spawned reset byte to 0
                    _map[mapPos.x, mapPos.y] = 0;
            }
            else if (byteIndicator == 1)
            {
                if (_tempPlayerCounter < _maxPlayersToSpawn)
                {
                    objToSpawn = _playerPlaceholder;
                    _tempPlayerCounter++;
                }
                else // if max players spawned reset byte to 0
                    _map[mapPos.x, mapPos.y] = 0;
            }

            if (objToSpawn != null)
            {
                // z pos adjusted with y value to allow for easy layering of sprites (.01f holds no signifigance, just to make it small)
                Vector3 adjustedPos = new Vector3(truePos.x, truePos.y, truePos.y * 0.01f); 

                GameObject newObj = Instantiate(objToSpawn, Vector3.zero, Quaternion.identity, transform);
                newObj.transform.localPosition = adjustedPos;
            }
        }
    }
}
