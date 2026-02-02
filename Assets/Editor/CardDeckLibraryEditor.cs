using CardSystem;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CardAndDeckLibrary))]
public class CardDeckLibraryEditor : Editor
{
    private CardAndDeckLibrary _library;

    private void OnEnable()
    {
        if (_library == null) _library = (CardAndDeckLibrary)target;

        //ClearOtherLibraries();

        GrabAssets();
    }
    public override void OnInspectorGUI()
    {
        if(_library == null) _library = (CardAndDeckLibrary)target;
        _library.CleanUpLists();
        GrabAssets();

        base.OnInspectorGUI();
    }
    private void GrabAssets()
    {
        if (_library == null) _library = (CardAndDeckLibrary)target;

        var deckGUIDS = AssetDatabase.FindAssets("t:Deck", new[] { "Assets/ScriptableObjects/DeckSOs" });
        var cardGUIDS = AssetDatabase.FindAssets("t:CardAbilityDefinition", new[] { "Assets/ScriptableObjects/CardAbilities" });

        if (deckGUIDS.Length != _library.GetDecksInProject.Count)
            foreach (var guid in deckGUIDS)
                _library.AddDeckToLibrary(AssetDatabase.LoadAssetAtPath<Deck>(AssetDatabase.GUIDToAssetPath(guid)));
        if (cardGUIDS.Length != _library.GetCardsInProject.Count)
            foreach (var guid in cardGUIDS)
                _library.AddCardToLibrary(AssetDatabase.LoadAssetAtPath<CardAbilityDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
    }
    /*private void ClearOtherLibraries()
    {
        if (_library == null) _library = (CardAndDeckLibrary)target;

        var libraryGUIDs = AssetDatabase.FindAssets("t:CardAndDeckLibrary", new[] { "Assets/Resources" });
        if (libraryGUIDs.Length > 1) 
        {
            for (int i = libraryGUIDs.Length - 1; i >= 0; i--)
            {
                var libraryAsset = AssetDatabase.LoadAssetAtPath<CardAndDeckLibrary>(AssetDatabase.GUIDToAssetPath(libraryGUIDs[i]));
                if (libraryAsset == null || libraryAsset == _library) continue;
                //EditorUtility.ClearDirty(libraryAsset);
                DestroyImmediate(libraryAsset, true);
            }
            Debug.LogError("message");
        }
    }*/
}
