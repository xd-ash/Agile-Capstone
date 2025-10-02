using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
namespace AStarPathfinding2
{
    public class MapLocation
    {
        public float x;
        public float y;

        public MapLocation(float _x, float _y)
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
        [SerializeField] private Tilemap _tilemap;

        [HideInInspector] public Vector2Int tileMousePos;
        [SerializeField] private Vector2Int _mapSize;
        [SerializeField] private float _mapScale = 1;
        [SerializeField] private byte[,] _map;
        [SerializeField] private List<MapLocation> _directions = new List<MapLocation>() {
                                                                     new MapLocation(0.5f,0.25f),
                                                                     new MapLocation(-0.5f,0.25f),
                                                                     new MapLocation(-0.5f,-0.25f),
                                                                     new MapLocation(0.5f,-0.25f) };
        private GameObject _tilePrefab;
        private GameObject _emptyMapAnchor;

        [Header("Placeholder Obstacle Spawn")]
        [SerializeField] private List<Vector2Int> obstLocationList;
        [SerializeField] private GameObject _placeholderObstacle;

        public byte[,] GetByteMap { get { return _map; } }
        public Vector2Int GetMapSize { get { return _mapSize; } }
        public float GetMapScale { get { return _mapScale; } }
        public List<MapLocation> GetDirections { get { return _directions; } }

        private void OnEnable()
        {
            _tilePrefab = Resources.Load<GameObject>("TileGridPrefabs/TileBase"); //maybe change to another grabbing method?
            _emptyMapAnchor = Instantiate(new GameObject(), transform);
            _emptyMapAnchor.name = "EmptyTileAnchor";
        }

        private void Start()
        {
            _map = new byte[_mapSize.x, _mapSize.y];

            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for (int y = 0; y < _map.GetLength(1); y++)
                {
                    if (obstLocationList.Contains(new Vector2Int(x, y))) //Placeholder obstacle spawn
                    {
                        _map[x, y] = 1;
                    }
                    else
                    {
                        _map[x, y] = 0;
                    }
                     
                    SpawnTileContents(_map[x, y], new Vector2(x * 0.5f,y * 0.25f));
                }
            }
        }
        private void SpawnTileContents(int byteIndicator, Vector2 mapPos)
        {
            GameObject tile = GameObject.Instantiate(_tilePrefab, _emptyMapAnchor.transform);
            tile.transform.localPosition = new Vector3(mapPos.x * _mapScale, mapPos.y * _mapScale, 0);
            tile.transform.localScale = tile.transform.localScale * _mapScale;
            tile.name = $"{mapPos.x}-{mapPos.y}";

            if (byteIndicator == 1)
            {
                GameObject obstacle = Instantiate(_placeholderObstacle, Vector3.zero, Quaternion.identity);
                obstacle.transform.parent = _emptyMapAnchor.transform;
                obstacle.transform.localPosition = new Vector3(mapPos.x * _mapScale, mapPos.y * _mapScale, 0);
            }
        }


        //insert WFC for map gen
    }
}
*/