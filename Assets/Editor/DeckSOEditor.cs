using CardSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Deck)), CanEditMultipleObjects]
public class DeckSOEditor : Editor
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
        Deck deck = (Deck)target;
        if (_library == null)
            _library = Resources.Load<CardAndDeckLibrary>("CardAndDeckLibrary");
        if (_library != null && !_library.GetDecksInProject.Contains(deck))
            _library.AddDeckToLibrary(deck);
    }
}
