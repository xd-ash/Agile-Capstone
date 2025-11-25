using System.Collections.Generic;
using UnityEngine;
using CardSystem;
using UnityEngine.EventSystems;

public class DeckViewer : MonoBehaviour
{
    [Tooltip("Where preview cards will be parented. If null, previews are parented to this object.")]
    public Transform spawnParent;

    [Tooltip("Resource path to the card prefab (same used by CardManager).")]
    public string cardPrefabPath = "CardTestPrefab";

    [Tooltip("Grid layout settings for spawned previews")]
    public int columns = 6;
    public Vector2 spacing = new Vector2(1.4f, 2.0f);

    [Tooltip("Limit to avoid huge instantiations in large decks (0 = no limit)")]
    public int maxPreviews = 0;

    [Header("Preview appearance")]
    [Tooltip("Local scale applied to each preview (editable)")]
    public Vector3 previewScale = Vector3.one * 0.5f;
    [Tooltip("Base local rotation applied to each preview (degrees)")]
    public Vector3 previewRotation = Vector3.zero;
    [Tooltip("Optional per-card Z jitter applied to rotation (degrees)")]
    public float rotationJitter = 0f;

    // Optional: require left mouse button for spawn (prevents other mouse events)
    public bool requireLeftMouseButton = true;

    private GameObject _previewContainer;

    private void Update()
    {
        // quick runtime debug: press L to log deck to console
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (CardManager.instance != null)
                CardManager.instance.LogRuntimeDeck();
            else
                Debug.LogWarning("[DeckViewer] CardManager.instance is null");
        }
    }

    // Simple world-click handler. Requires a Collider on this GameObject.
    private void OnMouseDown()
    {
        // optional left-button check
        if (requireLeftMouseButton && !Input.GetMouseButtonDown(0)) return;

        // ignore clicks over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // double-check pointer actually hit this object (robust for mixed 2D/3D setups)
        if (!PointerHitsThisObject()) return;

        ToggleOpen();
    }

    private bool PointerHitsThisObject()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Vector3 screen = Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(screen);

        // 3D raycast
        if (Physics.Raycast(ray, out RaycastHit hit3d))
        {
            if (hit3d.collider != null && (hit3d.collider.gameObject == gameObject || hit3d.collider.transform.IsChildOf(transform)))
                return true;
        }

        // 2D overlap at pointer position
        Vector3 worldPoint = cam.ScreenToWorldPoint(screen);
        Vector2 worldPoint2D = new Vector2(worldPoint.x, worldPoint.y);
        Collider2D hit2d = Physics2D.OverlapPoint(worldPoint2D);
        if (hit2d != null && (hit2d.gameObject == gameObject || hit2d.transform.IsChildOf(transform)))
            return true;

        return false;
    }

    public void ToggleOpen()
    {
        if (_previewContainer != null)
        {
            ClearPreviews();
            return;
        }

        ShowPreviews();
    }

    public void ShowPreviews()
    {
        if (CardManager.instance == null)
        {
            Debug.LogWarning("[DeckViewer] No CardManager.instance available.");
            return;
        }

        var defs = CardManager.instance.GetRuntimeDeckDefinitions();
        if (defs == null || defs.Length == 0)
        {
            Debug.Log("[DeckViewer] Deck is empty.");
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(cardPrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[DeckViewer] Could not load prefab at Resources/{cardPrefabPath}");
            return;
        }

        _previewContainer = new GameObject("DeckPreviewContainer");
        // parent the container under spawnParent if provided, otherwise under this object
        Transform containerParent = spawnParent != null ? spawnParent : transform;
        _previewContainer.transform.SetParent(containerParent, false);
        if (spawnParent == null)
            _previewContainer.transform.localPosition = Vector3.zero;
        else
            _previewContainer.transform.localPosition = Vector3.zero;

        int shown = 0;
        int limit = maxPreviews > 0 ? Mathf.Min(maxPreviews, defs.Length) : defs.Length;
        for (int i = 0; i < limit; i++)
        {
            var def = defs[i];
            if (def == null) continue;

            GameObject go = Instantiate(prefab, _previewContainer.transform);
            // position in grid
            int col = shown % columns;
            int row = shown / columns;
            Vector3 local = new Vector3(col * spacing.x, -row * spacing.y, 0f);
            go.transform.localPosition = local;

            // apply preview scale and rotation
            go.transform.localScale = previewScale;
            float jitter = rotationJitter != 0f ? (Random.Range(-rotationJitter, rotationJitter)) : 0f;
            go.transform.localEulerAngles = previewRotation + new Vector3(0f, 0f, jitter);

            // initialize card visuals via existing API
            Card card = new Card(def);
            var cs = go.GetComponent<CardSelect>();
            if (cs != null)
            {
                cs.OnPrefabCreation(card);
                // make preview non-interactive
                cs.enabled = false;
            }

            // disable physics / colliders on preview to avoid accidental interactions
            foreach (var col2D in go.GetComponentsInChildren<Collider2D>())
                col2D.enabled = false;
            foreach (var col3D in go.GetComponentsInChildren<Collider>())
                col3D.enabled = false;

            shown++;
        }

        Debug.Log($"[DeckViewer] Spawned {shown} previews.");
    }

    public void ClearPreviews()
    {
        if (_previewContainer != null)
        {
            Destroy(_previewContainer);
            _previewContainer = null;
        }
    }
}