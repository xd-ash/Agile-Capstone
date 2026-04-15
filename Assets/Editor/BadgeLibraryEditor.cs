using UnityEditor;

[CustomEditor(typeof(BadgeLibrary))]
public class BadgeLibraryEditor : Editor
{
    private BadgeLibrary _library;

    private void OnEnable()
    {
        BadgeLibrary.GrabAssets += () =>
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
        if (_library == null) _library = (BadgeLibrary)target;
        _library.CleanUpList();

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

        if (_library == null) _library = (BadgeLibrary)target;

        var badgeGuids = AssetDatabase.FindAssets("t:BadgeSO", new[] { "Assets/ScriptableObjects/Badges" });

        if (badgeGuids.Length != _library.GetBadgesInProject.Count)
        {
            _library.ClearBadgeLibrary();
            foreach (var guid in badgeGuids)
            {
                _library.AddBadgeToLibrary(AssetDatabase.LoadAssetAtPath<BadgeSO>(AssetDatabase.GUIDToAssetPath(guid)));
                tmp = true;
            }
        }
        return tmp;
    }
}
