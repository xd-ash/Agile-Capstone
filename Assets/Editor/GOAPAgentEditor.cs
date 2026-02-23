using UnityEditor;

[CustomEditor(typeof(GoapAgent), true), CanEditMultipleObjects]
public class GOAPAgentEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        GoapAgent agent = (GoapAgent)target;
        agent.GrabActionsFromEnum();
        agent.GrabGoalsFromEnum();

        foreach (var a in agent.GetActions)
            a?.GrabConditionsFromEnums(agent);

        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }
}
