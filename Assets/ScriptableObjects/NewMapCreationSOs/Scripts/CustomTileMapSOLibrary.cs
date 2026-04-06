using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomTileMapSOLibrary", menuName = "Libraries/CustomTileMapSOLibrary")]
public class CustomTileMapSOLibrary : ScriptableObject
{
    [SerializeField] private List<CustomTileMapSO> _customTileMapSOsInProject = new();

    public List<CustomTileMapSO> GetSOsInProject => _customTileMapSOsInProject;

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
    public CustomTileMapSO[] GetTileMapSOsFromType(CombatMapType type)
    {
        List<CustomTileMapSO> temp = new();
        foreach (var so in _customTileMapSOsInProject)
            if (so.GetCombatMapType == type)
                temp.Add(so);
        return temp.ToArray();
    }
}
