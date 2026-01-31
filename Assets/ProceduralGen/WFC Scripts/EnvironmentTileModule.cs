using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC
{
    //[CreateAssetMenu(fileName = "NewItemTileModule", menuName = "WFC/Modules/New Item Tile Module")]
    public class EnvironmentTileModule : ScriptableObject, IModule
    {
        //public Vector2Int keyDepthByModuleWidth;
        public int keyDepth;
        public int moduleWidth;

        [SerializeField] private EnvironmentTileModule[] _northNeighbours;
        [SerializeField] private EnvironmentTileModule[] _eastNeighbours;
        [SerializeField] private EnvironmentTileModule[] _southNeighbours;
        [SerializeField] private EnvironmentTileModule[] _westNeighbours;

        [SerializeField] private TileBase[] _trueTiles;
        [SerializeField] private TileBase[] _nKey, _eKey, _sKey, _wKey;
        [SerializeField] private TileType _tileType;

        public IModule[] North { get => _northNeighbours; set => _northNeighbours = value as EnvironmentTileModule[]; }
        public IModule[] East { get => _eastNeighbours; set => _eastNeighbours = value as EnvironmentTileModule[]; }
        public IModule[] South { get => _southNeighbours; set => _southNeighbours = value as EnvironmentTileModule[]; }
        public IModule[] West { get => _westNeighbours; set => _westNeighbours = value as EnvironmentTileModule[]; }
        public TileBase[] NorthKey { get => _nKey; set => _nKey = value; }
        public TileBase[] EastKey { get => _eKey; set => _eKey = value; }
        public TileBase[] SouthKey { get => _sKey; set => _sKey = value; }
        public TileBase[] WestKey { get => _wKey; set => _wKey = value; }
        public TileBase[] GetTrueTiles { get { return _trueTiles; } }
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