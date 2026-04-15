using UnityEditor;

[CustomEditor(typeof(GoapAgentSO)), CanEditMultipleObjects]
public class GOAPAgentSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        GoapAgentSO t = (GoapAgentSO)target;
        t.GrabActionsFromEnum();
        t.GrabGoalsFromEnum();

        foreach (var a in t.GetActions)
            a?.GrabConditionsFromEnums();

        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}