using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static IsoMetricConversions;

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
        //private Grid _grid;

        [SerializeField] private Vector2Int _mapSize;
        //[SerializeField] private float _mapScale = 1;
        [SerializeField] private byte[,] _map;
        [SerializeField] private List<MapLocation> _directions = new List<MapLocation>() {
                                                                     new MapLocation(1,0),      //new MapLocation(0.5f, 0.25f),
                                                                     new MapLocation(0,1),      //new MapLocation(-0.5f, 0.25f),
                                                                     new MapLocation(-1,0),     //new MapLocation(-0.5f, -0.25f),
                                                                     new MapLocation(0,-1) };   //new MapLocation(0.5f, -0.25f)};
        [Header("Placeholder Obstacle/Enemy Spawn")]
        [SerializeField] private List<Vector2Int> obstLocationList;
        [SerializeField] private List<Vector2Int> enemyLocationList;
        [SerializeField] private GameObject _placeholderObstacle;
        [SerializeField] private GameObject _rangeEnemyPlaceholder;
        [SerializeField] private GameObject _meleeEnemyPlaceholder;

        public byte[,] GetByteMap { get { return _map; } }
        public Vector2Int GetMapSize { get { return _mapSize; } }
        //public float GetMapScale { get { return _mapScale; } }
        public List<MapLocation> GetDirections { get { return _directions; } }
        //public Grid GetGrid { get { return _grid; } }

        private void OnEnable()
        {
            _tilemap = transform.Find("TileMap").GetComponent<Tilemap>();
            //_grid = transform.GetComponent<Grid>();
        }

        private void Start()
        {
            _map = new byte[_mapSize.x, _mapSize.y];

            _tilemap.CompressBounds();

            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for (int y = 0; y < _map.GetLength(1); y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);

                    if (enemyLocationList.Contains(new Vector2Int(x, y))) //placeholder enemy spawn
                        _map[x, y] = 2;
                    else if (obstLocationList.Contains(new Vector2Int(x, y))) //Placeholder obstacle spawn
                        _map[x, y] = 1;
                    else
                        _map[x, y] = 0;


                    SpawnTileContents(_map[x, y], gridPos);

                    _tilemap.SetTileFlags((Vector3Int)gridPos, TileFlags.None);
                }
            }
        }
        public void UpdateUnitPositionByteMap(Vector2Int startPos, Vector2Int endPos, Unit unit)
        {
            _map[startPos.x, startPos.y] = 0;
            _map[endPos.x, endPos.y] = unit.team == Team.Friendly ? (byte)3 : (byte)2;
        }

        //temp
        int tempEnemyCounter = 0;

        private void SpawnTileContents(int byteIndicator, Vector2Int mapPos)
        {
            Vector3 truePos = ConvertToIsometricFromGrid(mapPos);
            GameObject objToSpawn = null;


            if (byteIndicator == 1)
                objToSpawn = _placeholderObstacle;
            else if (byteIndicator == 2)
            {
                if (tempEnemyCounter % 2 == 0)
                    objToSpawn = _rangeEnemyPlaceholder;
                else
                    objToSpawn = _meleeEnemyPlaceholder;
                tempEnemyCounter++;
            }

            if (objToSpawn != null)
            {
                // z pos adjusted with y value to allow for easy layering of sprites (.01f holds no signifigance, just to make it small)
                Vector3 adjustedPos = new Vector3(truePos.x, truePos.y, truePos.y * 0.01f); 

                GameObject newObj = Instantiate(objToSpawn, Vector3.zero, Quaternion.identity);

                newObj.transform.parent = transform;
                newObj.transform.localPosition = adjustedPos;
            }
        }

        //insert WFC for map gen
    }
}
