using UnityEditor;

[CustomEditor(typeof(UnitLibrary))]
public class UnitLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = (UnitLibrary)target;

        if (t.SetEnemyData())
        {
            EditorUtility.SetDirty(t);
            AssetDatabase.SaveAssetIfDirty(t);
        }

        base.OnInspectorGUI();
    }
}