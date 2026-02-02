using System.Collections.Generic;
using UnityEngine;
using CardSystem;

public class DeckPreviewOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DeckAndHandManager deckManager;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRoot;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.8f, 0.8f, 0.8f);
    [SerializeField] private Vector3 visibleScale = Vector3.one;

    private bool isVisible = false;


    private void Awake()
    {
        if (deckManager == null)
            deckManager = DeckAndHandManager.instance;
    }
    public void Toggle()
    {
        if (isVisible) Hide();
        else Show();
    }

    public void Show()
    {
        if (!deckManager)
        {
            Debug.LogError("DeckPreviewOverlay: deckManager reference is missing!");
            return;
        }

        Build();

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        panelRoot.localScale = visibleScale;
        isVisible = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        panelRoot.localScale = hiddenScale;
        Clear();
        isVisible = false;
    }

    private void Build()
    {
        Clear();

        if (rowPrefab == null)
        {
            Debug.LogError("DeckPreviewOverlay: rowPrefab is not assigned!");
            return;
        }

        if (contentRoot == null)
        {
            Debug.LogError("DeckPreviewOverlay: contentRoot is not assigned!");
            return;
        }

        var runtimeDeck = deckManager.GetRuntimeDeck;

        if (runtimeDeck == null || runtimeDeck.Length == 0)
        {
            Debug.LogWarning("DeckPreviewOverlay: runtime deck is empty!");
            return;
        }

        // Count duplicates safely
        Dictionary<CardAbilityDefinition, int> counts = new Dictionary<CardAbilityDefinition, int>();
        foreach (var def in runtimeDeck)
        {
            if (def == null)
            {
                Debug.LogWarning("DeckPreviewOverlay: null CardAbilityDefinition found in runtime deck! Skipping.");
                continue;
            }

            if (!counts.ContainsKey(def))
                counts[def] = 0;

            counts[def]++;
        }

        if (counts.Count == 0)
        {
            Debug.LogWarning("DeckPreviewOverlay: no valid cards to display in overlay.");
            return;
        }

        // Spawn rows
        foreach (var pair in counts)
        {
            var rowGO = Instantiate(rowPrefab, contentRoot);
            var row = rowGO.GetComponent<DeckPreviewRow>();

            if (row == null)
            {
                Debug.LogError("DeckPreviewOverlay: rowPrefab missing DeckPreviewRow component!");
                Destroy(rowGO);
                continue;
            }

            // Bind only if valid
            if (pair.Key != null)
            {
                row.Bind(pair.Key, pair.Value);
                Debug.Log($"DeckPreviewOverlay: spawned row for {pair.Key.GetCardName} x{pair.Value}");
            }
        }
    }

    private void Clear()
    {
        foreach (Transform t in contentRoot)
            Destroy(t.gameObject);
    }
}
