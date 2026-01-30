using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
namespace WFC
{
    [RequireComponent(typeof(Tilemap))]
    public class ItemTileModuleGenerator : MonoBehaviour
    {
        [SerializeField] private EnvironmentTileSet _tileSet;
        private List<EnvironmentTileModule> _tileModules;

        [SerializeField] private int _moduleWidth = 6;
        [SerializeField] private int _keyDepth = 1;
        //private int _maxNullTilesInModule;// number of tiles within a module, that are null & have no tilebase (catches tilemap sections with no modules)
        //private int _numEmptyTiles = 0;// number of tile modules with no items present

        public void GenerateTileModules()
        {
            _tileModules = new List<EnvironmentTileModule>();
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

                    string assetPath = $"Assets/ProceduralGen/WFC SOs/Modules/TileModules/Tile({x},{y}).asset";
                    EnvironmentTileModule asset = AssetDatabase.LoadAssetAtPath<EnvironmentTileModule>(assetPath);

                    if (asset != null)
                        ClearEnvironmentTileModule(asset);

                    asset = ScriptableObject.CreateInstance<EnvironmentTileModule>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                    AssetDatabase.SaveAssets();

                    //if (!CheckNullAndEmptyModules(tiles)) continue;

                    asset.keyDepth = _keyDepth;
                    asset.moduleWidth = _moduleWidth;
                    asset.SetBasesAndKeys(tiles.ToArray());
                    EditorUtility.SetDirty(asset);

                    _tileModules.Add(asset);
                }
            }

            _tileSet.Modules = _tileModules.ToArray();
            _tileSet.SetNeighbours();
        }

        /* private bool CheckNullAndEmptyModules(List<TileBase> tiles)
        {
            int nullCount = 0;

            foreach (TileBase tile in tiles)
            {
                if (tile == null)
                    nullCount++;

                if (nullCount > _maxNullTilesInModule)
                    return false;
            }

            if (nullCount == _maxNullTilesInModule)
            {
                if (_numEmptyTiles == 0)
                    _numEmptyTiles++;
                else
                    return false;
            }

            return true;
        }*/

        public void ClearEnvironmentTileModule(EnvironmentTileModule asset)
        {
            EditorUtility.ClearDirty(asset);
            DestroyImmediate(asset, true);
        }
    }
}
#endif