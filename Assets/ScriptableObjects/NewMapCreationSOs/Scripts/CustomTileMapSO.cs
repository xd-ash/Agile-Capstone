using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "CustomTileMapSO", menuName = "Custom Map Creation/CustomTileMapSO")]
public class CustomTileMapSO : ScriptableObject
{
    [SerializeField] private Vector2Int _gridSize = new Vector2Int(9, 9);

    private string _isoTileMapPrefabPath = "Assets/Prefabs/NewCustomMapCreationPrefabs/MainTileMapPrefab.prefab";
    private string _byteMapTileMapPrefabPath = "Assets/Prefabs/NewCustomMapCreationPrefabs/ByteMapTileMapPrefab.prefab";
    private string _parentFolderPath = "Assets/ScriptableObjects/NewMapCreationSOs/TileMapPrefabs";
    private string _folderPath;
    private string _newIsoPrefabName = "IsoTileMap.prefab";
    private string _newBytePrefabName = "ByteTileMap.prefab";

    [SerializeField] private GameObject _mainIsoTileMap;
    [SerializeField] private GameObject _byteMapTileMap;
    private TileBase[,] _tileBaseMap;


    public GameObject GetMainTileMap => _mainIsoTileMap;
    public TileBase[,] GetTileBaseMap => _tileBaseMap;

    public void SetTileBaseMap(TileBase[,] map)
    {
        _tileBaseMap = map;
    }

    // on SO creation, create new folder with new "blank" tilemaps for editing (called via editor script)
    public void InitSO()
    {
        //try create folder, if fail then just return. bandaid fix for asset loading issues
        AssetDatabase.CreateFolder(_parentFolderPath, name);
        _folderPath = $"{_parentFolderPath}/{name}";

        GameObject isoTilemapAsset = AssetDatabase.LoadAssetAtPath<GameObject>(_isoTileMapPrefabPath);
        GameObject byteTilemapAsset = AssetDatabase.LoadAssetAtPath<GameObject>(_byteMapTileMapPrefabPath);

        PrefabUtility.SaveAsPrefabAsset(isoTilemapAsset, $"{_folderPath}/{_newIsoPrefabName}");
        PrefabUtility.SaveAsPrefabAsset(byteTilemapAsset, $"{_folderPath}/{_newBytePrefabName}");
         
        _mainIsoTileMap = AssetDatabase.LoadAssetAtPath<GameObject>($"{_folderPath}/{_newIsoPrefabName}");
        _byteMapTileMap = AssetDatabase.LoadAssetAtPath<GameObject>($"{_folderPath}/{_newBytePrefabName}");
        EditorUtility.SetDirty(_mainIsoTileMap);
        EditorUtility.SetDirty(_byteMapTileMap);

        if (!_byteMapTileMap.TryGetComponent(out ByteTileMapHelper helper))
            helper = _byteMapTileMap.AddComponent<ByteTileMapHelper>();
        helper.InitHelper(this, _gridSize);

        AssetDatabase.SaveAssetIfDirty(_mainIsoTileMap);
        AssetDatabase.SaveAssetIfDirty(_byteMapTileMap);
    }
    // method to auto set folder name to match SO name using editor script
    public void SetTileMapFolderName()
    {
        if (_folderPath == null || _folderPath == string.Empty) return;

        string[] splitPath = _folderPath.Split("/");
        string folderName = splitPath[^1];
        if (folderName == name) return;
        splitPath[^1] = name;
        string newFolderPath = string.Join('/', splitPath);
        AssetDatabase.MoveAsset(_folderPath, newFolderPath);
    }
}
