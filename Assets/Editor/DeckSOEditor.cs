using CardSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Deck)), CanEditMultipleObjects]
public class DeckSOEditor : Editor
{
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
        var library = Resources.Load<CardAndDeckLibrary>("CardAndDeckLibrary");
        if (library != null && !library.GetDecksInProject.Contains(deck))
            library.AddDeckToLibrary(deck);
    }
}
