using CardSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardAbilityDefinition)), CanEditMultipleObjects]
public class CardDefEditor : Editor
{
    CardAndPackLibrary _library;

    private void OnEnable()
    {
        AddToLibrary();
    }
    public override void OnInspectorGUI()
    {
        AddToLibrary();
        base.OnInspectorGUI();

        var c = (CardAbilityDefinition)target;
        c.SetEffectDefForUpgradeCollections();
    }
    private void AddToLibrary()
    {
        CardAbilityDefinition card = (CardAbilityDefinition)target;
        if (_library == null)  
            _library = Resources.Load<CardAndPackLibrary>("Libraries/CardAndPackLibrary");
        if (_library != null && !_library.GetCardsInProject.Contains(card))
            _library.AddCardToLibrary(card);
        if (_library == null)
            Debug.Log("library null");
    }
}
