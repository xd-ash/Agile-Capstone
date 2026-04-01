using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomTileMapSOLibrary))]
public class CustomTileMapSOLibraryEditor : Editor
{
    private static CustomTileMapSOLibrary _library;

    public static Action AssetGrab => () => GrabAssetsConnector();

    public override void OnInspectorGUI()
    {
        if (_library == null) _library = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTilemapSOLibrary");

        _library.CleanUpList();

        if (GrabAssets())
        {
            EditorUtility.SetDirty(_library);
            AssetDatabase.SaveAssetIfDirty(_library);
        }
        base.OnInspectorGUI();
    }
    private static void GrabAssetsConnector()
    {
        if (GrabAssets())
        {
            EditorUtility.SetDirty(_library);
            AssetDatabase.SaveAssetIfDirty(_library);
        }
    }
    public static bool GrabAssets()
    {
        bool tmp = false;

        if (_library == null) _library = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTilemapSOLibrary");

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
