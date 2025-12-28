using System.Collections.Generic;
using UnityEngine;
using CardSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering;

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

    [Header("Preview layering and background")]
    [Tooltip("Sorting layer name applied to spawned previews")]
    public string previewSortingLayerName = "Default";
    [Tooltip("Sorting order applied to spawned previews (higher = drawn on top)")]
    public int previewSortingOrder = 5000;
    [Tooltip("Optional UI blur material. If null a semi-transparent overlay will be used.")]
    public Material blurMaterial;
    [Tooltip("Tint used for fallback overlay when no blur material is provided.")]
    public Color overlayColor = new Color(0f, 0f, 0f, 0.5f);

    private GameObject _previewContainer;
    private GameObject _overlay;

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

        var defs = CardManager.instance.GetRuntimeDeck;
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

        // create overlay (blur or tinted panel) to dim/blur rest of scene
        CreateOverlay();

        _previewContainer = new GameObject("DeckPreviewContainer");
        // parent the container under spawnParent if provided, otherwise under this object
        Transform containerParent = spawnParent != null ? spawnParent : transform;
        _previewContainer.transform.SetParent(containerParent, false);
        _previewContainer.transform.localPosition = Vector3.zero;

        // Add SortingGroup to ensure previews render above other renderers
        var sortingGroup = _previewContainer.AddComponent<SortingGroup>();
        sortingGroup.sortingLayerName = previewSortingLayerName;
        sortingGroup.sortingOrder = previewSortingOrder;

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

            // Make sure all renderers/canvases on this preview are set to draw on top
            ForceSortingOnChildren(go, previewSortingLayerName, previewSortingOrder);

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

        if (_overlay != null)
        {
            Destroy(_overlay);
            _overlay = null;
        }
    }

    // Helper: apply sorting settings to renderers and canvases inside a preview
    private void ForceSortingOnChildren(GameObject root, string sortingLayer, int sortingOrder)
    {
        // Set Renderer sorting
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            // only set if renderer supports sorting (SpriteRenderer, MeshRenderer with sorting group, etc.)
            // set common properties to ensure draw order
            r.sortingLayerName = sortingLayer;
            r.sortingOrder = sortingOrder;
        }

        // Set UI Canvas sorting (if prefab uses world-space or overlay canvases)
        var canvases = root.GetComponentsInChildren<Canvas>(true);
        foreach (var c in canvases)
        {
            c.overrideSorting = true;
            c.sortingLayerName = sortingLayer;
            c.sortingOrder = sortingOrder;
        }
    }

    // Create a full-screen overlay used to blur or tint the background
    private void CreateOverlay()
    {
        // If an overlay already exists, do nothing
        if (_overlay != null) return;

        // Create a top-level UI canvas for the overlay so it sits beneath previews (previews use SortingGroup with high order)
        _overlay = new GameObject("DeckPreviewOverlay");
        var canvas = _overlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // set overlay sorting to just below previewSortingOrder
        canvas.overrideSorting = true;
        canvas.sortingOrder = Mathf.Max(0, previewSortingOrder - 1);

        _overlay.AddComponent<CanvasScaler>();
        _overlay.AddComponent<GraphicRaycaster>();

        // Add full-screen image
        GameObject imgGO = new GameObject("OverlayImage");
        imgGO.transform.SetParent(_overlay.transform, false);
        var img = imgGO.AddComponent<Image>();
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        img.rectTransform.offsetMin = Vector2.zero;
        img.rectTransform.offsetMax = Vector2.zero;

        // Apply blur material if provided, otherwise fallback to tinted panel
        if (blurMaterial != null)
        {
            img.material = blurMaterial;
            // keep a semi-transparent tint as well if desired
            img.color = overlayColor;
        }
        else
        {
            img.color = overlayColor;
        }

        // Ensure overlay blocks pointer events so background UI doesn't get clicks while deck is open
        img.raycastTarget = true;

        // Attach click catcher so overlay clicks can either forward to the world object
        // (so clicking the DeckViewer behind the overlay still toggles it) or close the overlay.
        var catcher = imgGO.AddComponent<OverlayClickCatcher>();
        catcher.owner = this;
    }
}

// Small helper component attached to the overlay image to handle clicks.
// If the click corresponds to the DeckViewer's world object the click is forwarded,
// otherwise the overlay simply closes the previews.
public class OverlayClickCatcher : MonoBehaviour, IPointerClickHandler
{
    public DeckViewer owner;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner == null) return;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector2 screenPos = eventData.position;
            Ray ray = cam.ScreenPointToRay(screenPos);

            // 3D check
            if (Physics.Raycast(ray, out RaycastHit hit3d))
            {
                Transform hitTransform = hit3d.collider?.transform;
                if (hitTransform != null && (hitTransform.gameObject == owner.gameObject || hitTransform.IsChildOf(owner.transform)))
                {
                    owner.ToggleOpen();
                    return;
                }
            }

            // 2D check (match how PointerHitsThisObject does it)
            Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            Collider2D hit2d = Physics2D.OverlapPoint(new Vector2(worldPoint.x, worldPoint.y));
            if (hit2d != null)
            {
                Transform hitTransform2d = hit2d.transform;
                if (hitTransform2d != null && (hitTransform2d.gameObject == owner.gameObject || hitTransform2d.IsChildOf(owner.transform)))
                {
                    owner.ToggleOpen();
                    return;
                }
            }
        }

        // Click was not on the DeckViewer world object — close the deck preview.
        owner.ClearPreviews();
    }
}