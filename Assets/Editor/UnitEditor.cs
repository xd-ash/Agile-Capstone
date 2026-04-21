using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Unit))]
public class UnitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        DrawDefaultInspector();

        var u = (Unit)target;

        GUILayout.Space(10f);
        GUILayout.Label(new GUIContent("Unit Stats"), EditorStyles.boldLabel);
        GUILayout.Space(10f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_unitSO"));
        GUILayout.Space(10f);
        GUILayout.Label($"Team: {u.GetTeam}");
        GUILayout.Space(5f);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label($"Current Health: {u.GetHealth}");
            GUILayout.Space(10f);
            GUILayout.Label($" | ");
            GUILayout.Space(10f);
            GUILayout.Label($"Max Health: {u.GetMaxHealth}");
            GUILayout.FlexibleSpace();
        }
        GUILayout.Space(5f);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label($"Current Shield: {u.GetShield}");
            GUILayout.Space(10f);
            GUILayout.Label($" | ");
            GUILayout.Space(10f);
            GUILayout.Label($"Max Shield: {u.GetMaxShield}");
            GUILayout.FlexibleSpace();
        }
        GUILayout.Space(5f);
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label($"Current AP: {u.GetAP}");
            GUILayout.Space(30f);
            GUILayout.Label($" | ");
            GUILayout.Space(10f);
            GUILayout.Label($"Max AP: {u.GetMaxAP}");
            GUILayout.FlexibleSpace();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
