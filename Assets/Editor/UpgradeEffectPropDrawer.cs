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

        // get all valid effect nodes is card def graph
        var effectOptions = target.cardDef.GetEffectOptions();

        // get string array to use for popup content
        var optionStrings = target.cardDef.GetEffectOptionsStrings();

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
}
