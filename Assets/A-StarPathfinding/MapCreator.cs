using System.Collections.Generic;
using CustomGridTool;
using NUnit.Framework;
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
        //automnate these?
        [SerializeField] private Tilemap _tilemap;
        [SerializeField] private GridAnchorScript _gridAnchor;
        //

        [SerializeField] private byte[,] _map;
        [SerializeField] private List<MapLocation> _directions = new List<MapLocation>() {
                                                                 new MapLocation(1,0),
                                                                 new MapLocation(0,1),
                                                                 new MapLocation(-1,0),
                                                                 new MapLocation(0,-1) };

        public byte[,] GetByteMap { get { return _map; } }
        public List<MapLocation> GetDirections { get { return _directions; } }

        private void Start()
        {
            _map = new byte[_gridAnchor.GridCols, _gridAnchor.GridRows];

            for (int x = 0; x < _map.GetLength(0); x++)
            {
                for (int y = 0; y < _map.GetLength(1); y++)
                {
                    _map[x, y] = 0;
                }
            }
        }

        //insert WFC for map gen
    }
}
