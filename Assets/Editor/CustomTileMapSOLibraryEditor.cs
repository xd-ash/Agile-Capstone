using UnityEditor;

[CustomEditor(typeof(CustomTileMapSOLibrary))]
public class CustomTileMapSOLibraryEditor : Editor
{
    private CustomTileMapSOLibrary _library;

    private void OnEnable()
    {
        CustomTileMapSOLibrary.GrabAssets += GrabAssetsConnector;
    }
    public override void OnInspectorGUI()
    {
        if (_library == null) _library = (CustomTileMapSOLibrary)target;
        _library.CleanUpList();

        if (GrabAssets())
        {
            EditorUtility.SetDirty(_library);
            AssetDatabase.SaveAssetIfDirty(_library);
        }
        base.OnInspectorGUI();
    }
    private void GrabAssetsConnector()
    {
        if (GrabAssets())
        {
            EditorUtility.SetDirty(_library);
            AssetDatabase.SaveAssetIfDirty(_library);
        }
    }
    public bool GrabAssets()
    {
        bool tmp = false;

        if (_library == null) _library = (CustomTileMapSOLibrary)target;

        var soGUIDS = AssetDatabase.FindAssets("t:CustomTileMapSO", new[] { "Assets/ScriptableObjects/NewMapCreationSOs" });

        if (soGUIDS.Length != _library.GetSOsInProject.Count)
        {
            _library.ClearList();
            foreach (var guid in soGUIDS)
            {
                _library.AddTileMapSOToLibrary(AssetDatabase.LoadAssetAtPath<CustomTileMapSO>(AssetDatabase.GUIDToAssetPath(guid)));
                tmp = true;
            }
        }
        return tmp;
    }
}
