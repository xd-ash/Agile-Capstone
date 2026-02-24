using System.Collections.Generic;
using UnityEngine;
using static IsoMetricConversions;
using static GameObjectPool;
using System;

public static class TileHighlighter
{
    //private static List<GameObject> _lastHighlightedTiles = new List<GameObject>();
    private static Dictionary<Guid, List<GameObject>> _highlightedTilesDict = new();

    public static void ApplyHighlights(HashSet<Vector2Int> cells, Guid guid, Color highlightColor, int sortingBoost = 0)
    {
        var highlightObjectParent = MapCreator.Instance.transform.Find("HighlightObjParent");
        var highlightTilePrefab = Resources.Load<GameObject>("HighlightTile");

        ClearHighlights(guid);

        foreach (var cell in cells)
        {
            Vector3 cellLocalPos = ConvertToIsometricFromGrid(cell);
            GameObject tile = Spawn(highlightTilePrefab, cellLocalPos, Quaternion.identity, Vector3.one, highlightObjectParent);
            var sr = tile.GetComponentInChildren<SpriteRenderer>();
            sr.color = highlightColor;
            sr.sortingOrder = 1 + sortingBoost;
            //_lastHighlightedTiles.Add(tile);

            if (!_highlightedTilesDict.ContainsKey(guid))
                _highlightedTilesDict.Add(guid, new List<GameObject>() { tile });
            else
                _highlightedTilesDict[guid].Add(tile);
        }
    }

    public static void ClearHighlights(Guid guid)
    {
        if (!_highlightedTilesDict.ContainsKey(guid)) return;

        // Removes the gameobjects using the object pooling class before dict cleanup
        for (int i = 0; i < _highlightedTilesDict[guid].Count; i++)
        {
            var tile = _highlightedTilesDict[guid][i];
            if (tile == null) continue;
            Remove(tile);
        }

        _highlightedTilesDict.Remove(guid);

        /*for (int i = _lastHighlightedTiles.Count - 1; i >= 0; i--)
            if (_lastHighlightedTiles[i] != null)
                Remove(_lastHighlightedTiles[i]);

        _lastHighlightedTiles.Clear();*/
    }
    public static void ClearAllHighlights()
    {

    }
}
