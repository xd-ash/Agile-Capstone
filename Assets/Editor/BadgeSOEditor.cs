using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BadgeSO)), CanEditMultipleObjects]
public class BadgeSOEditor : Editor
{
    BadgeLibrary _library;

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
        BadgeSO badge = (BadgeSO)target;
        if (_library == null)
            _library = Resources.Load<BadgeLibrary>("Libraries/BadgeLibrary");
        if (_library != null && !_library.GetBadgesInProject.Contains(badge))
            _library.AddBadgeToLibrary(badge);
        if (_library == null)
            Debug.Log("library null");
    }
}
