#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

public enum CombatMapType
{
    NormalCombat,
    Tutorial,
    // different "game mode" comabt (escort, protect npc)?
    // special boss combat maps?
}
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

    [SerializeField] private CombatMapType _combatType = CombatMapType.NormalCombat;
    [Space(10)]
    [SerializeField] private GameObject _mainIsoTileMapPrefab;
    [SerializeField] private GameObject _byteMapTileMapPrefab;

    private Tilemap _byteTileMap;

    private bool _didInit = false;
    public bool DidInit => _didInit;
    public CombatMapType GetCombatMapType => _combatType;

    public GameObject GetMainTileMap => _mainIsoTileMapPrefab;

#if UNITY_EDITOR

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

        _mainIsoTileMapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{_folderPath}/{_newIsoPrefabName}");
        _byteMapTileMapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{_folderPath}/{_newBytePrefabName}");

        _byteTileMap = _byteMapTileMapPrefab.GetComponent<Tilemap>();

        _didInit = true;

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssetIfDirty(this);
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
#endif
    public TileBase[,] GenerateTileBaseMap(Vector2Int mapsize)
    {
        var map = new TileBase[mapsize.x, mapsize.y];
        if (_byteTileMap == null)
            _byteTileMap = _byteMapTileMapPrefab.GetComponent<Tilemap>();
        _byteTileMap.CompressBounds();

        //x & y start at 1 because of the "border" that is present in the tilemap prefab to help frame the grid for editing
        for (int x = 1; x <= mapsize.x; x++)
            for (int y = 1; y <= mapsize.y; y++)
                map[x - 1, y - 1] = _byteTileMap.GetTile(new Vector3Int(x, y));
        return map;
    }
}
