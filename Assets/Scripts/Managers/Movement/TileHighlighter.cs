using System.Collections.Generic;
using UnityEngine;
using static IsoMetricConversions;
using static GameObjectPool;

public static class TileHighlighter
{
    private static List<GameObject> _lastHighlightedTiles = new List<GameObject>();

    public static void ApplyHighlights(HashSet<Vector2Int> cells, Color highlightColor)
    {
        var highlightObjectParent = MapCreator.Instance.transform.Find("HighlightObjParent");
        var highlightTilePrefab = Resources.Load<GameObject>("HighlightTile");

        ClearHighlights();

        foreach (var cell in cells)
        {
            Vector3 cellLocalPos = ConvertToIsometricFromGrid(cell);
            GameObject tile = Spawn(highlightTilePrefab, cellLocalPos, Quaternion.identity, Vector3.one, highlightObjectParent);
            tile.GetComponentInChildren<SpriteRenderer>().color = highlightColor;
            _lastHighlightedTiles.Add(tile);
        }
    }

    public static void ClearHighlights()
    {
        for (int i = _lastHighlightedTiles.Count - 1; i >= 0; i--)
            if (_lastHighlightedTiles[i] != null)
                Remove(_lastHighlightedTiles[i]);

        _lastHighlightedTiles.Clear();
    }
}
