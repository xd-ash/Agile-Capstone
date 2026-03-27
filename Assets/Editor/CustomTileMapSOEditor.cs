using CardSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomTileMapSO)), CanEditMultipleObjects]
public class CustomTileMapSOEditor : Editor
{
    CustomTileMapSOLibrary _library;

    private void Awake()
    {
        AddToLibrary();
    }

    public override void OnInspectorGUI()
    {
        AddToLibrary();

        base.OnInspectorGUI();
        
        var t = (CustomTileMapSO)target;
        t.SetTileMapFolderName();
    }
    private void AddToLibrary()
    {
        var t = (CustomTileMapSO)target; 
        if (_library == null)
            _library = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTilemapSOLibrary");
        if (_library != null && !_library.GetSOsInProject.Contains(t))
        {
            _library.AddTileMapSOToLibrary(t);
            EditorUtility.SetDirty(_library);
            AssetDatabase.SaveAssetIfDirty(_library);
        }
        if (_library == null)
            Debug.Log("library null");
    }
}

//this class initializes the newly created customTileMapSO safely
public class CustomTileMapSOAssetProc : AssetModificationProcessor
{
    static string[] OnWillSaveAssets(string[] paths)
    {
        foreach (var path in paths)
        {
            string[] s = path.Split('/');
            if (s[^2] != "NewMapCreationSOs" || s[^1].Substring(s[^1].Length - 4) == "meta") continue;
            var t = AssetDatabase.LoadAssetAtPath<CustomTileMapSO>(path); 
            if (t.DidInit) continue;
            t.InitSO();
            CustomTileMapSOLibraryEditor.AssetGrab?.Invoke();
        }
        return paths;
    }
}
