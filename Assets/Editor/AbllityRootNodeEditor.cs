using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace CardSystem
{
    [CustomNodeEditor(typeof(AbilityRootNode))]
    public class AbilityRootNodeEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            serializedObject.Update();
            AbilityRootNode node = target as AbilityRootNode;

            GUILayout.Label($"{(target.graph as CardAbilityDefinition).GetCardName}: ", EditorStyles.boldLabel);
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_effectTypes"));
            GUILayout.Space(10);
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("targeting"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("filtering"));
            GUILayout.Space(10);
            List<string> effectTypes = new List<string>(node.GetEffectTypes.ToString().Split(", "));
            if (effectTypes.Contains("Helpful") || effectTypes.Contains("-1"))
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("helpfulEffects"));
            if (effectTypes.Contains("Harmful") || effectTypes.Contains("-1"))
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("harmfulEffects"));
            if (effectTypes.Contains("Misc") || effectTypes.Contains("-1"))
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("miscEffects"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}