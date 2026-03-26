using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace CardSystem
{
    [CustomNodeEditor(typeof(EffectStrategy)), CanEditMultipleObjects]
    public class EffectStrategyEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            serializedObject.Update();

            EffectStrategy node = target as EffectStrategy;

            
            if (node is IUseEffectValue)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_effectValue"), new GUIContent($"{DetermineStratType(node)}:"));

            serializedObject.ApplyModifiedProperties();
            base.OnBodyGUI();
            serializedObject.ApplyModifiedProperties();
        }
        public string DetermineStratType(EffectStrategy node)
        {
            switch (node)
            {
                case HealEffect:
                    return "Heal Value";
                case DamageEffect:
                    return "Damage Value";
                case BuffEffect:
                    return "Buff/Shield Value";
                case DebuffEffect:
                    return "Debuff Value";
                case OverTimeEffect:
                    return "Duration (Turns)";
                case DeckEffect:
                    return "Card Amount";
                case KnockBackEffect:
                    return "Knockback Distance";
                case RestoreAPEffect:
                    return "AP Value";
                case MultiplyEffect:
                    return "Number of Times";
                default:
                    return "Effect Value";
            }
        }
    }
}