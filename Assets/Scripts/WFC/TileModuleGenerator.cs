using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    OnlyObstacles,
    SingleEnemy,
    SinglePlayer
}

#if UNITY_EDITOR
namespace WFC
{
    [RequireComponent(typeof(Tilemap))]
    public class TileModuleGenerator : MonoBehaviour
    {
        [SerializeField] private TileSet _tileSet;
        private List<TileModule> _tileModules;

        [SerializeField, HideInInspector] private int _moduleWidth = 5;
        [SerializeField, HideInInspector] private int _keyDepth = 1;

        [SerializeField, HideInInspector] private TileType _tileType;

        private int _maxNullTilesInModule;// number of tiles within a module, that are null & have no tilebase (catches tilemap sections with no modules)
        //private int _numEmptyModules = 0;// number of modules with no object tiles present

        public void GenerateTileModules()
        {
            _tileModules = new List<TileModule>();
            _maxNullTilesInModule = (_moduleWidth - 2 * _keyDepth) * (_moduleWidth - 2 * _keyDepth) + 4 * _keyDepth; // max null tiles
            //_maxNullTilesInModule = _moduleWidth * _moduleWidth - (_moduleWidth - 2 * _keyDepth) * 4;

            Tilemap tilemap = GetComponent<Tilemap>();
            tilemap.CompressBounds();

            Vector2Int tileGridDimensions = new Vector2Int(tilemap.size.x / _moduleWidth, tilemap.size.y / _moduleWidth);
            
            for (int x = 0; x < tileGridDimensions.x; x++)
            {
                for (int y = 0; y < tileGridDimensions.y; y++)
                {
                    Vector3Int moduleOrigin = new Vector3Int(tilemap.origin.x + x * _moduleWidth, tilemap.origin.y + y * _moduleWidth);
                    List<TileBase> tiles = new List<TileBase>();

                    for (int j = 0; j < _moduleWidth; j++)
                    {
                        for (int i = 0; i < _moduleWidth; i++)
                        {
                            Vector3Int tilePos = moduleOrigin + new Vector3Int(i, j);
                            tiles.Add(tilemap.GetTile(tilePos));
                        }
                    }

                    string assetPath = $"Assets/ScriptableObjects/WFC SOs/TileModules/{_tileType}Tile({x},{y}).asset";
                    TileModule asset = AssetDatabase.LoadAssetAtPath<TileModule>(assetPath);

                    if (asset != null)
                        ClearTileModuleAsset(asset);
                    AssetDatabase.SaveAssets();

                    if (!CheckNullAndEmptyModules(tiles)) continue;

                    asset = ScriptableObject.CreateInstance<TileModule>();
                    AssetDatabase.CreateAsset(asset, assetPath);

                    asset.InitModuleValues(tiles.ToArray(), _tileType, _keyDepth, _moduleWidth);
                    EditorUtility.SetDirty(asset);

                    _tileModules.Add(asset);
                }
            }
            AssetDatabase.SaveAssets();

            _tileSet.AddModules(_tileModules);
            _tileSet.SetNeighbours();
            EditorUtility.SetDirty(_tileSet);

            AssetDatabase.SaveAssets();
        }

        private bool CheckNullAndEmptyModules(List<TileBase> tiles)
        {
            int nullCount = 0;

            foreach (TileBase tile in tiles)
            {
                if (tile == null)
                    nullCount++;

                //catch for completely empty sections of tilemap prefab (no keys, no tiles)
                if (nullCount > _maxNullTilesInModule)
                    return false;
            }

            if (nullCount == _maxNullTilesInModule)
            {
                /*if (_numEmptyModules == 0)
                    _numEmptyModules++;
                else
                    return false;*/
            }

            return true;
        }

        public void ClearTileModuleAsset(TileModule asset)
        {
            EditorUtility.ClearDirty(asset);
            DestroyImmediate(asset, true);
        }
    }
}
#endif