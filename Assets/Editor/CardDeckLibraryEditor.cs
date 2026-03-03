using System;
using CardSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardAndDeckLibrary))]
public class CardDeckLibraryEditor : Editor
{
    private CardAndDeckLibrary _library;

    private void OnEnable()
    {
        CardAndDeckLibrary.GrabAssets += () =>
        {
            if (GrabAssets())
            {
                EditorUtility.SetDirty(_library);
                AssetDatabase.SaveAssetIfDirty(_library);
            }
        };
    }
    public override void OnInspectorGUI()
    {
        if(_library == null) _library = (CardAndDeckLibrary)target;
        _library.CleanUpLists();

        if (GrabAssets())
        {
            EditorUtility.SetDirty(_library);
            AssetDatabase.SaveAssetIfDirty(_library);
        }
        base.OnInspectorGUI(); 
    }
    public bool GrabAssets()
    {
        bool tmp = false;

        if (_library == null) _library = (CardAndDeckLibrary)target;

        var cardGUIDS = AssetDatabase.FindAssets("t:CardAbilityDefinition", new[] { "Assets/ScriptableObjects/CardAbilities" });

        if (cardGUIDS.Length != _library.GetCardsInProject.Count)
        {
            _library.ClearCardLibrary();
            foreach (var guid in cardGUIDS)
            {
                _library.AddCardToLibrary(AssetDatabase.LoadAssetAtPath<CardAbilityDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
                tmp = true;
            }
        }
        return tmp;
    }
}
