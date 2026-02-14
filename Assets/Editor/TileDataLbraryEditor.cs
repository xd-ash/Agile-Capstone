using UnityEditor;

[CustomEditor(typeof(ProceduralTileLibrary))]
public class TileDataLbraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ProceduralTileLibrary t = (ProceduralTileLibrary)target;
        if (t.SetTileNamesOnGUI())
        {
            EditorUtility.SetDirty(t);
            AssetDatabase.SaveAssetIfDirty(t);
        }

        base.OnInspectorGUI();
    }
}
