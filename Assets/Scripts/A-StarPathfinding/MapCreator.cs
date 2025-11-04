using System.Collections.Generic;
using UnityEngine;

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
        public Vector2 ToVector()
        {
            return new Vector2(x, y);
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
            {
                instance = this;
            }
            else
            {
                Debug.LogError($"{this.gameObject.name} has been destroyed due to singleton conflict");
                Destroy(this.gameObject);
            }
        }

        //[SerializeField] private Tilemap _tilemap;  //Unused currently

        [HideInInspector] public Vector2Int tileMousePos = new(-1,-1);
        [SerializeField] private Vector2Int _mapSize;
        [SerializeField] private float _mapScale = 1;
        [SerializeField] private byte[,] _map;
        [SerializeField] private List<MapLocation> _directions = new List<MapLocation>() {
                                                                     new MapLocation(1,0),      //new MapLocation(0.5f, 0.25f),
                                                                     new MapLocation(0,1),      //new MapLocation(-0.5f, 0.25f),
                                                                     new MapLocation(-1,0),     //new MapLocation(-0.5f, -0.25f),
                                                                     new MapLocation(0,-1) };   //new MapLocation(0.5f, -0.25f)};
        private GameObject _tilePrefab;
        private GameObject _emptyMapAnchor;

        [Header("Placeholder Obstacle/Enemy Spawn")]
        [SerializeField] private List<Vector2Int> obstLocationList;
        [SerializeField] private List<Vector2Int> enemyLocationList;
        [SerializeField] private GameObject _placeholderObstacle;
        [SerializeField] private GameObject _enemyPlaceholder;

        public byte[,] GetByteMap { get { return _map; } }
        public Vector2Int GetMapSize { get { return _mapSize; } }
        public float GetMapScale { get { return _mapScale; } }
        public List<MapLocation> GetDirections { get { return _directions; } }

        private void OnEnable()
        {
            _tilePrefab = Resources.Load<GameObject>("TileGridPrefabs/TileBase"); //maybe change to another grabbing method?

            // Messy but should be removed on fixing isometric tilemap movement
            _emptyMapAnchor = new("EmptyTileAnchor");
            _emptyMapAnchor.transform.parent = transform.Find("UnitMoveEmpty");
            _emptyMapAnchor.transform.localEulerAngles = Vector3.zero;
            _emptyMapAnchor.transform.localPosition = Vector3.zero;
            _emptyMapAnchor.transform.localScale = Vector3.one;
        }

        private void Start()
        {
            _map = new byte[_mapSize.x, _mapSize.y];

            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for (int y = 0; y < _map.GetLength(1); y++)
                {
                    //if (new Vector2Int((int)_enemyPlaceholder.localPosition.x, (int)_enemyPlaceholder.localPosition.y) == new Vector2Int(x, y)) //Placeholder enemy spawn
                    if (enemyLocationList.Contains(new Vector2Int(x, y)))
                    {
                        _map[x, y] = 2;
                    }
                    else if (obstLocationList.Contains(new Vector2Int(x, y))) //Placeholder obstacle spawn
                    {
                        _map[x, y] = 1;
                    }
                    else
                    {
                        _map[x, y] = 0;
                    }
                     
                    SpawnTileContents(_map[x, y], new Vector2Int(x,y));
                }
            }
        }
        private void SpawnTileContents(int byteIndicator, Vector2Int mapPos)
        {
            GameObject tile = GameObject.Instantiate(_tilePrefab, _emptyMapAnchor.transform);
            tile.transform.localPosition = new Vector3(mapPos.x * _mapScale, mapPos.y * _mapScale, 0);
            tile.transform.localScale = tile.transform.localScale * _mapScale;
            tile.name = $"{mapPos.x}-{mapPos.y}";

            if (byteIndicator == 1)
            {
                GameObject obstacle = GameObject.Instantiate(_placeholderObstacle, Vector3.zero, Quaternion.identity);
                obstacle.transform.parent = _emptyMapAnchor.transform;
                obstacle.transform.localPosition = new Vector3(mapPos.x * _mapScale, mapPos.y * _mapScale, 0);
            }
            else if (byteIndicator == 2)
            {
                GameObject enemy = GameObject.Instantiate(_enemyPlaceholder, _enemyPlaceholder.transform.position, Quaternion.identity);
                enemy.transform.parent = transform.Find("UnitMoveEmpty");
                enemy.transform.localPosition = new Vector3(mapPos.x * _mapScale, mapPos.y * _mapScale, _enemyPlaceholder.transform.position.z);
            }
        }

        //insert WFC for map gen
    }
}
