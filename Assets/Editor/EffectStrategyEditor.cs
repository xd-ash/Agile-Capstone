using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace CardSystem
{
    [CustomNodeEditor(typeof(EffectStrategy))]
    public class EffectStrategyEditor : NodeEditor
    {
        bool isAura = false;

        public override void OnBodyGUI()
        {
            serializedObject.Update();
            EffectStrategy node = target as EffectStrategy;
            if (node is BuffEffect || node is DebuffEffect)
            {
                node.HasDuration = true;
                isAura = true;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));
                GUILayout.Space(-10);
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("effectVisuals"));
            }

            if (node is DebugEffect)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("message"), new GUIContent($"Debug Message:"));
            else if (node is DeckEffect)
            {
                // Make me look better
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_action"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_amount"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_logResults"));
            }
            else
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_effectValue"), new GUIContent($"{DetermineStratType(node)} Value:"));

            if (!isAura)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_hasDuration"));
                EditorGUI.indentLevel++;
            }
            if (node.HasDuration)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_duration"));
            if (!isAura) EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
        public string DetermineStratType(EffectStrategy node)
        {
            switch (node)
            {
                case HealEffect:
                    return "Heal";
                case DamageEffect:
                    return "Damage";
                case BuffEffect:
                    return "Buff";
                case DebuffEffect:
                    return "Debuff";
                default:
                    return "Effect";
            }
        }
    }
}