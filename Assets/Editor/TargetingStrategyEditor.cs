using UnityEditor;
using XNodeEditor;

namespace CardSystem
{
    //[CustomNodeEditor(typeof(TargetingStrategy))]
    public class TargetingStrategyEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            serializedObject.Update();
            TargetingStrategy node = target as TargetingStrategy;

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            //NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("isAOE"));
            EditorGUI.indentLevel++;
            //if (node.isAOE)
                //NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}