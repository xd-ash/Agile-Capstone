using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace CardSystem
{
    [CustomNodeEditor(typeof(EffectStrategy))]
    public class EffectStrategyEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            serializedObject.Update();
            EffectStrategy node = target as EffectStrategy;
            bool isAura = false;
            if (node is BuffEffect || node is DebuffEffect)
            {
                node.HasDuration = true;
                isAura = true;
            }

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("input"));

            if (node is DebugEffect)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("message"), new GUIContent($"Debug Message:"));
            else if (node is DeckEffect)
                GUILayout.Label("* Insert Deck Stuff Here *");
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