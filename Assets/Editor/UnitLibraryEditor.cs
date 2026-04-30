using UnityEditor;

[CustomEditor(typeof(UnitLibrary))]
public class UnitLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = (UnitLibrary)target;
        t.SetEnemyData();
        base.OnInspectorGUI();
    }
}