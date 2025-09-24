using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
        [SerializeField] private Tilemap _tilemap;

        [HideInInspector] public Vector2Int tileMousePos;

        [SerializeField] private Vector2Int _mapSize;
        [SerializeField] private float _mapScale = 1;
        [SerializeField] private byte[,] _map;
        [SerializeField] private List<MapLocation> _directions = new List<MapLocation>() {
                                                                 new MapLocation(1,0),
                                                                 new MapLocation(0,1),
                                                                 new MapLocation(-1,0),
                                                                 new MapLocation(0,-1) };
        private GameObject _tilePrefab;
        private GameObject _emptyMapAnchor;

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
                    _map[x, y] = 0;
                }
            }
            for (int x = 0; x < _mapSize.x; x++)
            {
                for (int y = 0; y < _mapSize.y; y++)
                {
                    GameObject tile = GameObject.Instantiate(_tilePrefab, _emptyMapAnchor.transform);
                    tile.transform.localPosition = new Vector3(x * _mapScale, y * _mapScale, 0);
                    tile.transform.localScale = tile.transform.localScale * _mapScale;
                    tile.name = $"{x}-{y}";
                }
            }

        }

        //insert WFC for map gen
    }
}
