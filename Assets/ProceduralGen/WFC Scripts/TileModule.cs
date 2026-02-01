using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC
{
    //[CreateAssetMenu(fileName = "NewItemTileModule", menuName = "WFC/Modules/New Item Tile Module")]
    public class TileModule : ScriptableObject
    {
        //public Vector2Int keyDepthByModuleWidth;
        public int keyDepth;
        public int moduleWidth;

        [SerializeField] private TileModule[] _northNeighbours;
        [SerializeField] private TileModule[] _eastNeighbours;
        [SerializeField] private TileModule[] _southNeighbours;
        [SerializeField] private TileModule[] _westNeighbours;

        [SerializeField] private TileBase[] _trueTiles;
        [SerializeField] private TileBase[] _nKey, _eKey, _sKey, _wKey;
        [SerializeField] private TileType _tileType;

        public TileModule[] North => _northNeighbours;
        public TileModule[] East => _eastNeighbours;
        public TileModule[] South => _southNeighbours;
        public TileModule[] West => _westNeighbours;
        public TileBase[] NorthKey => _nKey;
        public TileBase[] EastKey => _eKey;
        public TileBase[] SouthKey => _sKey;
        public TileBase[] WestKey => _wKey;
        public TileBase[] GetTrueTiles => _trueTiles;
        public TileType GetTileType => _tileType;

        public void InitModuleValues(TileBase[] tileArray, TileType tileType)
        {
            int width = moduleWidth - 1;
            List<TileBase> n = new(), e = new(), s = new(), w = new();
            List<TileBase> trueTileList = new();

            int topRight = GetIndex(width, width);
            int topLeft = GetIndex(0, width);

            for (int y = 0; y <= width; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    int index = GetIndex(x, y);
                    //Debug.Log($"index: {index} ({x},{y})");

                    if (index == 0 || index == width || index == topLeft || index == topRight)
                        continue;

                    if (y == width)
                        n.Add(tileArray[index]);
                    else if (x == width)
                        e.Add(tileArray[index]);
                    else if (y == 0)
                        s.Add(tileArray[index]);
                    else if (x == 0)
                        w.Add(tileArray[index]);
                    else
                        trueTileList.Add(tileArray[index]);
                }
            }

            _nKey = n.ToArray();
            _eKey = e.ToArray();
            _sKey = s.ToArray();
            _wKey = w.ToArray();
            _trueTiles = trueTileList.ToArray();

            _tileType = tileType;
        }
        public void SetNeighborArrays(TileModule[] north, TileModule[] east, TileModule[] south, TileModule[] west)
        {
            _northNeighbours = north;
            _eastNeighbours = east;
            _southNeighbours = south;
            _westNeighbours = west;
        }
        private int GetIndex(int x, int y)
        {
            return y * moduleWidth + x;
        }
        public int GetTrueTileIndex(int x, int y)
        {
            return y * (moduleWidth - 2 * keyDepth) + x;
        }
    }
}