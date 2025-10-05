using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
/*
namespace OldCardSystem
{
    [CustomEditor(typeof(CardSO))]
    [CanEditMultipleObjects]
    public class CardSOEditor : Editor
    {
        //general card data
        public SerializedProperty _cardName;
        public SerializedProperty _description;
        public SerializedProperty _apCost;
        public SerializedProperty _isTileTargeted;
        public SerializedProperty _cardPrefab;
        public SerializedProperty _cardType;

        //public SerializedProperty _cardSubTypes;
        public SerializedProperty _doesDamage;
        public SerializedProperty _isAreaOfEffect;
        public SerializedProperty _causesStatuses;
        public SerializedProperty _isDelayedEffect;
        public SerializedProperty _isUtility;

        //range card data
        public SerializedProperty _projectilePrefab;
        public SerializedProperty _projectileSpeed;
        public SerializedProperty _isRepeating;
        public SerializedProperty _numRepeats;
        public SerializedProperty _range;

        //melee card data
        //public SerializedProperty _weaponPrefab;

        //AoE card data
        public SerializedProperty _aoeType;
        public SerializedProperty _aoeRange;

        //utility card data
        public SerializedProperty _utilityTypes;
        public SerializedProperty _hasMultipleUtilities;
        public SerializedProperty _cardReturnValue;
        public SerializedProperty _apRestoreValue;
        public SerializedProperty _healValue;
        public SerializedProperty _buffValue;

        //delayed card data
        public SerializedProperty _delayDuration;

        //Damage card data
        public SerializedProperty _damageTypes;
        public SerializedProperty _damageValue;

        //Status card data
        public SerializedProperty _statusTypes;
        public SerializedProperty _statusDuration;

        private void OnEnable()
        {
            //general
            _cardName = serializedObject.FindProperty("_cardName");
            _description = serializedObject.FindProperty("_description");
            _apCost = serializedObject.FindProperty("_apCost");
            _isTileTargeted = serializedObject.FindProperty("_isTileTargeted");
            _cardPrefab = serializedObject.FindProperty("_cardPrefab");
            _cardType = serializedObject.FindProperty("_cardType");

            //_cardSubTypes = serializedObject.FindProperty("_cardSubTypes");
            _doesDamage = serializedObject.FindProperty("_isDealDamage");
            _isAreaOfEffect = serializedObject.FindProperty("_isAreaOfEffect");
            _causesStatuses = serializedObject.FindProperty("_isCauseStatus");
            _isDelayedEffect = serializedObject.FindProperty("_isDelayedEffect");
            _isUtility = serializedObject.FindProperty("_isUtility");

            //range
            _projectilePrefab = serializedObject.FindProperty("_projectilePrefab");
            _projectileSpeed = serializedObject.FindProperty("_projectileSpeed");
            _isRepeating = serializedObject.FindProperty("_isRepeating");
            _numRepeats = serializedObject.FindProperty("_numRepeats");
            _range = serializedObject.FindProperty("_range");

            //melee
            //_weaponPrefab = serializedObject.FindProperty("_weaponPrefab");

            //utility
            _utilityTypes = serializedObject.FindProperty("_utilityTypes");
            _hasMultipleUtilities = serializedObject.FindProperty("_hasMultipleUtilities");
            _cardReturnValue = serializedObject.FindProperty("_cardReturnValue");
            _apRestoreValue = serializedObject.FindProperty("_apRestoreValue");
            _healValue = serializedObject.FindProperty("_healValue");
            _buffValue = serializedObject.FindProperty("_buffValue");

            //AoE
            _aoeType = serializedObject.FindProperty("_aoeType");
            _aoeRange = serializedObject.FindProperty("_aoeRange");

            //delay
            _delayDuration = serializedObject.FindProperty("_delayDuration");

            //damage
            _damageTypes = serializedObject.FindProperty("_damageTypes");
            _damageValue = serializedObject.FindProperty("_damageValue");

            //status
            _statusTypes = serializedObject.FindProperty("_statusTypes");
            _statusDuration = serializedObject.FindProperty("_statusDuration");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(_cardName);
            EditorGUILayout.PropertyField(_description);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_apCost);
            EditorGUILayout.PropertyField(_isTileTargeted);
            EditorGUILayout.PropertyField(_cardPrefab);
            EditorGUILayout.PropertyField(_cardType);

            GUILayout.Space(10);
            if (_cardType.enumValueIndex == 0)
            {
                GUILayout.Label(new GUIContent("* No Card Type Specified *"), EditorStyles.boldLabel);
            }
            else
            {
                switch (_cardType.enumValueIndex)
                {
                    case 1:
                        GUILayout.Label(new GUIContent("* Misc Specific Card Data Not Implemented *"), EditorStyles.boldLabel);
                        break;
                    case 2:
                        GUILayout.Label(new GUIContent("Range Card Data:", "Data fields for the range card type"), EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(_range);
                        EditorGUILayout.PropertyField(_projectilePrefab);
                        EditorGUILayout.PropertyField(_projectileSpeed);
                        EditorGUILayout.PropertyField(_isRepeating);
                        EditorGUI.indentLevel++;
                        if (_isRepeating.boolValue)
                            EditorGUILayout.PropertyField(_numRepeats);
                        EditorGUI.indentLevel -= 2;
                        break;
                    case 3:
                        GUILayout.Label(new GUIContent("* Melee Specific Card Data Not Implemented *"), EditorStyles.boldLabel);
                        //GUILayout.Label(new GUIContent("Melee Card Data:", "Data fields for the melee card type"));
                        //EditorGUILayout.PropertyField(_weaponPrefab);
                        break;
                }

                GUILayout.Space(10);

                GUILayout.Label("Card Sub-Types:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_doesDamage);
                EditorGUILayout.PropertyField(_isAreaOfEffect);
                EditorGUILayout.PropertyField(_causesStatuses);
                EditorGUILayout.PropertyField(_isDelayedEffect);
                EditorGUILayout.PropertyField(_isUtility);
                EditorGUI.indentLevel--;

                GUILayout.Space(10);

                if (_doesDamage.boolValue)
                {
                    GUILayout.Label(new GUIContent("Does Damage Data:", "Data for IDealDamage interface"), EditorStyles.boldLabel);
                    GUILayout.Space(5);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_damageTypes);
                    EditorGUILayout.PropertyField(_damageValue);
                    EditorGUI.indentLevel--;
                    GUILayout.Space(10);
                }
                if (_isAreaOfEffect.boolValue)
                {
                    GUILayout.Label(new GUIContent("Area of Effect Data:", "Data for IAreaOfEffect interface"), EditorStyles.boldLabel);
                    GUILayout.Space(5);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_aoeType);
                    EditorGUILayout.PropertyField(_aoeRange);
                    EditorGUI.indentLevel--;
                    GUILayout.Space(10);
                }
                if (_causesStatuses.boolValue)
                {
                    GUILayout.Label(new GUIContent("Causes Statuses Data:", "Data for ICauseStatuses interface"), EditorStyles.boldLabel);
                    GUILayout.Space(5);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_statusTypes);
                    EditorGUILayout.PropertyField(_statusDuration);
                    EditorGUI.indentLevel--;
                    GUILayout.Space(10);
                }
                if (_isDelayedEffect.boolValue)
                {
                    GUILayout.Label(new GUIContent("Delayed Effect Data:", "Data for IDelayedEffect interface"), EditorStyles.boldLabel);
                    GUILayout.Space(5);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_delayDuration);
                    EditorGUI.indentLevel--;
                    GUILayout.Space(10);
                }
                if (_isUtility.boolValue)
                {
                    GUILayout.Label(new GUIContent("Utility Data:", "Data for IUtility interface"), EditorStyles.boldLabel);
                    GUILayout.Space(5);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_utilityTypes);
                    for (int i = 0; i < _utilityTypes.arraySize; i++)
                    {
                        if (_utilityTypes.GetArrayElementAtIndex(i).enumValueIndex == 1) //cardreturn
                            EditorGUILayout.PropertyField(_cardReturnValue);
                        if (_utilityTypes.GetArrayElementAtIndex(i).enumValueIndex == 2) //ap restore
                            EditorGUILayout.PropertyField(_apRestoreValue);
                        if (_utilityTypes.GetArrayElementAtIndex(i).enumValueIndex == 3) //heal
                            EditorGUILayout.PropertyField(_healValue);
                        if (_utilityTypes.GetArrayElementAtIndex(i).enumValueIndex == 4) //buff
                            EditorGUILayout.PropertyField(_buffValue);
                    }
                    EditorGUI.indentLevel--;
                    //EditorGUILayout.PropertyField(_hasMultipleUtilities);
                    GUILayout.Space(10);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
*/