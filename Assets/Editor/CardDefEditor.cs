using CardSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardAbilityDefinition)), CanEditMultipleObjects]
public class CardDefEditor : Editor
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
        CardAbilityDefinition card = (CardAbilityDefinition)target;
        var library = Resources.Load<CardAndDeckLibrary>("CardAndDeckLibrary");
        if (library != null && !library.GetCardsInProject.Contains(card))
            library.AddCardToLibrary(card);
    }
}
