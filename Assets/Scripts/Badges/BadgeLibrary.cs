using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BadgeLibrary", menuName = "Rewards/New Badge Library")]
public class BadgeLibrary : ScriptableObject
{
    [SerializeField] private List<BadgeSO> _badgesInProject = new();

    public List<BadgeSO> GetBadgesInProject => _badgesInProject;

    public static Action GrabAssets;

    public void AddBadgeToLibrary(BadgeSO badge)
    {
        if (badge == null) return;
        
        if (!_badgesInProject.Contains(badge))
            _badgesInProject.Add(badge);
    }

    public void CleanUpList()
    {
        for (int i = _badgesInProject.Count - 1; i >= 0; i--)
            if (_badgesInProject[i] == null)
                _badgesInProject.RemoveAt(i);
    }
    public void ClearBadgeLibrary()
    {
        _badgesInProject.Clear();
    }
    public BadgeSO GetBadgeFromName(string badgeName)
    {
        foreach (var badge in _badgesInProject)
            if (badge.name == badgeName)
                return badge;

        Debug.LogWarning($"No matching badge found in library for \"{badgeName}\"");
        return null;
    }
}
