using CardSystem;
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
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(new GUIContent($"{DetermineStratType(node)}:"));
                    GUILayout.FlexibleSpace();
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_effectValue"), new GUIContent(""));
                }
            }
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
                case HealthPercentEffect:
                    return "Health Threshold";
                default:
                    return "Effect Value";
            }
        }
    }
}
[CustomNodeEditor(typeof(OtherTarget)), CanEditMultipleObjects]
public class TargetingsTratEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        base.OnBodyGUI();
        serializedObject.ApplyModifiedProperties();

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent($"Targets Tiles:"));
            GUILayout.FlexibleSpace();
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_targetTilesNotUnits"), new GUIContent(""));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
[CustomNodeEditor(typeof(OverTimeEffect)), CanEditMultipleObjects]
public class OverTimeEffectEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        base.OnBodyGUI();
        serializedObject.ApplyModifiedProperties();

        GUILayout.Label(new GUIContent($"Ticks:"));

        EditorGUI.indentLevel++;
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent($"   On Application:"));
            GUILayout.FlexibleSpace();
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_doEffectOnApply"), new GUIContent(""));
            GUILayout.FlexibleSpace();
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent($"   On Turn Start:  "));
            GUILayout.FlexibleSpace();
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_tickOnStart"), new GUIContent(""));
            GUILayout.FlexibleSpace();
        }
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}
[CustomNodeEditor(typeof(RollDieOverUnderEffect)), CanEditMultipleObjects]
public class RollDieOverUnderEffectEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();

        base.OnBodyGUI();
        serializedObject.ApplyModifiedProperties();

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(new GUIContent($"Roll Threshold:"));
            GUILayout.FlexibleSpace();
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("_desiredMinRoll"), new GUIContent(""));
            GUILayout.FlexibleSpace();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
