using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ByteTileMapHelper)), CanEditMultipleObjects]
public class ByteTileMapHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var t = (ByteTileMapHelper)target;

        if (GUILayout.Button("Generate TileBase Map"))
            t.GenerateTileBaseMap();
    }
}
