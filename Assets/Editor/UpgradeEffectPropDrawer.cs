using CardSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using XNode;

[CustomPropertyDrawer(typeof(EffectUpgrade)), CanEditMultipleObjects]
public class EffectUpgradePropDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EffectUpgrade target = property.GetUnderlyingValue() as EffectUpgrade;

        if (target.cardDef == null) return;

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        var valueRect = new Rect(position.x, position.y + 20, position.width, EditorGUIUtility.singleLineHeight);

        // grab all valid effect nodes is card def graph
        List<EffectStrategy> effectOptions = new();
        foreach (var node in target.cardDef.nodes)
            if (node is IUseEffectValue)
                effectOptions.Add(node as EffectStrategy);

        // create string array to use for popup content
        string[] optionStrings = new string[effectOptions.Count];
        for (int i = 0; i < effectOptions.Count; i++)
        {
            var node  = effectOptions[i];
            optionStrings[i] = GetNodePath(node, string.Empty);
        }
        
        //grab current index of selected effect
        int currIndex = target.effectToUpgrade != null ? effectOptions.IndexOf(target.effectToUpgrade) : 0;

        // create popup menu to select effects
        currIndex = EditorGUI.Popup(dropdownRect, currIndex, optionStrings);

        //set effect from popup index
        target.effectToUpgrade = effectOptions[currIndex];
        
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("valueToAdd"), new GUIContent("Increase Effect Value by:"));

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lineCount = 2;
        float totalHeight = EditorGUIUtility.singleLineHeight * lineCount +
                            EditorGUIUtility.standardVerticalSpacing * (lineCount - 1);

        return totalHeight;
    }
    private string GetNodePath(Node node, string curPath)
    {
        if (node == null || node is AbilityRootNode) return curPath;
        Node parent = null;
        curPath = node.name + (curPath == string.Empty ? "" : $">{curPath}");
        foreach (var port in node.Inputs)
        {
            parent = port.Connection.node;
            if (parent == null) continue;
            break;
        }
        return GetNodePath(parent, curPath);
    }
}
