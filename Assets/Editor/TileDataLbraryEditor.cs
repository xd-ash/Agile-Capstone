using UnityEditor;

[CustomEditor(typeof(ProceduralTileLibrary))]
public class TileDataLbraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ProceduralTileLibrary t = (ProceduralTileLibrary)target;
        t.SetTileNamesOnGUI();
        base.OnInspectorGUI();
    }
}
