using System.Collections.Generic;
using UnityEngine;
using CardSystem;

public class DeckPreviewOverlay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private DeckPreviewRow rowPrefab;

    private readonly List<DeckPreviewRow> _spawnedRows = new();

    private void OnEnable()
    {
        Build();
    }

    public void Build()
    {
        Clear();

        var deckManager = DeckAndHandManager.instance;
        if (deckManager == null)
        {
            Debug.LogError("[DeckPreviewOverlay] No DeckAndHandManager found");
            return;
        }

        var runtimeDeck = deckManager.GetRuntimeDeck;
        if (runtimeDeck == null || runtimeDeck.Length == 0)
        {
            Debug.LogWarning("[DeckPreviewOverlay] Runtime deck is empty");
            return;
        }

        // Count duplicates
        Dictionary<CardAbilityDefinition, int> counts = new();

        foreach (var def in runtimeDeck)
        {
            if (def == null) continue;

            if (!counts.ContainsKey(def))
                counts[def] = 0;

            counts[def]++;
        }

        foreach (var pair in counts)
        {
            var row = Instantiate(rowPrefab, contentRoot);
            row.Bind(pair.Key, pair.Value);
            _spawnedRows.Add(row);
        }
    }

    private void Clear()
    {
        foreach (var row in _spawnedRows)
            Destroy(row.gameObject);

        _spawnedRows.Clear();
    }
}
