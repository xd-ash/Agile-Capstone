using System;
using CardSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardAndPackLibrary))]
public class CardAndPackLibraryEditor : Editor
{
    private CardAndPackLibrary _library;

    private void OnEnable()
    {
        CardAndPackLibrary.GrabAssets += () =>
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
        if(_library == null) _library = (CardAndPackLibrary)target;
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

        if (_library == null) _library = (CardAndPackLibrary)target;

        var cardGUIDS = AssetDatabase.FindAssets("t:CardAbilityDefinition", new[] { "Assets/ScriptableObjects/CardAbilities" });

        if (cardGUIDS.Length != _library.GetCardsInProject.Count)
        {
            _library.ClearCardLibrary();
            foreach (var guid in cardGUIDS)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Split('/')[2] != "CardAbilities") continue;
                _library.AddCardToLibrary(AssetDatabase.LoadAssetAtPath<CardAbilityDefinition>(path));
                tmp = true;
            }
        }
        return tmp;
    }
}
