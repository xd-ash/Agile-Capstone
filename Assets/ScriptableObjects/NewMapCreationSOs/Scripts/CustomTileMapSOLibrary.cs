using CardSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "CustomTileMapSOLibrary", menuName = "Scriptable Objects/CustomTileMapSOLibrary")]
public class CustomTileMapSOLibrary : ScriptableObject
{
    [SerializeField] private List<CustomTileMapSO> _customTileMapSOsInProject = new();

    public List<CustomTileMapSO> GetSOsInProject => _customTileMapSOsInProject;

    public static Action GrabAssets;
    public void AddTileMapSOToLibrary(CustomTileMapSO tileMapSO)
    {
        if (tileMapSO == null) return;

        if (!_customTileMapSOsInProject.Contains(tileMapSO))
            _customTileMapSOsInProject.Add(tileMapSO);
    }
    public void CleanUpList()
    {
        for (int i = _customTileMapSOsInProject.Count - 1; i >= 0; i--)
            if (_customTileMapSOsInProject[i] == null)
                _customTileMapSOsInProject.RemoveAt(i);
    }
    public void ClearList()
    {
        _customTileMapSOsInProject.Clear();
    }
    public CustomTileMapSO GetTileMapSOFromName(string soName)
    {
        foreach (var so in _customTileMapSOsInProject)
            if (so.name == soName)
                return so;
        return null;
    }
}
