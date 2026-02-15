using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace CardSystem
{
    //[CustomNodeEditor(typeof(EffectStrategy))]
    public class EffectStrategyEditor : NodeEditor
    {
        public override void OnBodyGUI()
        {
            /*
            serializedObject.Update();

            EffectStrategy node = target as EffectStrategy;

            if (node is not DebugEffect && node is not SpawnObjectEffect)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_effectValue"), new GUIContent($"{DetermineStratType(node)}:"));

            base.OnBodyGUI();

            serializedObject.ApplyModifiedProperties();
            */
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
                    return "Buff Value";
                case DebuffEffect:
                    return "Debuff Value";
                case StopMovementEffect:
                case OverTimeEffect:
                    return "Duration (Turns)";
                default:
                    return "Effect Value";
            }
        }
    }
}