using CardSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CardAbilityDefinition)), CanEditMultipleObjects]
public class CardDefEditor : Editor
{
    CardAndPackLibrary _library;
    private bool _optionFoldout = false;

    private void OnEnable()
    {
        AddToLibrary();
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        AddToLibrary();
        
        DisplayNodeOptionsForDescriptions();
        base.OnInspectorGUI();

        var c = (CardAbilityDefinition)target;
        c.SetEffectDefForUpgradeCollections();

        serializedObject.ApplyModifiedProperties();
    }
    private void AddToLibrary()
    {
        CardAbilityDefinition card = (CardAbilityDefinition)target;

        var path = AssetDatabase.GetAssetPath(card);
        if (path.Split('/')[2] != "CardAbilities") return;

        if (_library == null)
            _library = Resources.Load<CardAndPackLibrary>("Libraries/CardAndPackLibrary");
        if (_library != null && !_library.GetCardsInProject.Contains(card))
            _library.AddCardToLibrary(card);
        if (_library == null)
            Debug.Log("library null");
    }
    private void DisplayNodeOptionsForDescriptions()
    {
        var c = (CardAbilityDefinition)target;
        c.GetEffectOptions();
        var optionsStrings = c.GetEffectOptionsStrings();

        _optionFoldout = EditorGUILayout.Foldout(_optionFoldout, "Effect Options & Indices:");
        if (_optionFoldout)
        {
            GUILayout.Label("(Use the index displayed with the desired effect & value)\nDynamic Effect Values entered as \"~index~\" within description");
            //EditorGUI.indentLevel++;
            foreach (var option in optionsStrings)
                GUILayout.Label(option);
            //EditorGUI.indentLevel--;
        }
    }
}
