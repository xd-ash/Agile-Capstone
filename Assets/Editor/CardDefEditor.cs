using CardSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardAbilityDefinition)), CanEditMultipleObjects]
public class CardDefEditor : Editor
{
    CardAndDeckLibrary _library;

    private void OnEnable()
    {
        AddToLibrary();
    }
    public override void OnInspectorGUI()
    {
        AddToLibrary();

        base.OnInspectorGUI();
    }
    private void AddToLibrary()
    {
        CardAbilityDefinition card = (CardAbilityDefinition)target;
        if (_library == null)  
            _library = Resources.Load<CardAndDeckLibrary>("Libraries/CardAndDeckLibrary");
        if (_library != null && !_library.GetCardsInProject.Contains(card))
            _library.AddCardToLibrary(card);
        if (_library == null)
            Debug.Log("library null");
    }
}
