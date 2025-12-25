using AStarPathfinding;
using CardSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AbilityEvents;
using static IsoMetricConversions;

public class MouseFunctionManager : MonoBehaviour
{
    public static MouseFunctionManager instance;

    private Tilemap _tilemap;

    [Header("Tile")]
    [SerializeField] private Color _mouseTileColor = Color.yellow;
    private GameObject _highlightTile;
    private Vector3Int _tilePos;
    private TileBase _currTile;
    private bool _shouldMove;
    public Vector3Int GetCurrTilePosition => _tilePos;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_tilemap == null)
            _tilemap = FindAnyObjectByType<Tilemap>();

        InitializeTileHighlight();
    }

    private void InitializeTileHighlight()
    {
        var highlightObjectParent = FindAnyObjectByType<Grid>().transform.Find("HighlightObjParent");
        _highlightTile = Instantiate(Resources.Load<GameObject>("HighlightTile"), highlightObjectParent);
        _highlightTile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _highlightTile.transform.localScale = Vector3.one;
        var sr = _highlightTile.GetComponentInChildren<SpriteRenderer>();
        sr.color = _mouseTileColor;
        sr.sortingOrder = 2;
        _highlightTile.SetActive(false);
    }

    private void Update()
    {
        // Right click to cancel activated attack card/ability
        if (Input.GetMouseButtonDown(1))
            if (IsTargeting && !PauseMenu.isPaused)
                if (CardSystem.CardManager.instance.selectedCard != null &&
                    CardSystem.CardManager.instance.selectedCard.CardTransform.TryGetComponent<CardSelect>(out CardSelect card))
                {
                    AbilityEvents.TargetingStopped();
                    card.ReturnCardToHand();
                    CardManager.instance.OnCardAblityCancel?.Invoke();
                }

        if (PauseMenu.isPaused || !TrackMouse()) return;

        _highlightTile.SetActive(true);
        _highlightTile.transform.localPosition = ConvertToIsometricFromGrid((Vector2Int)_tilePos);

        // If target selection active or tile hover is out of range then clear line, else left mouse click to move to tile
        if (IsTargeting || !MovementLine.instance.DrawMovementPath(out _shouldMove))
            MovementLine.instance.ClearLine();
        else
        {
            if (Input.GetMouseButtonDown(0) && _shouldMove)
            {
                var unitAStar = TurnManager.GetCurrentUnit.GetComponent<FindPathAStar>();
                unitAStar?.OnStartUnitMove();
            }
        }
    }

    // return true if mouse is over valid tile
    private bool TrackMouse()
    {
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldMouse.z = 0f;

        _tilePos = _tilemap.WorldToCell(worldMouse);
        _currTile = _tilemap.GetTile(_tilePos);

        if (_currTile == null)
        {
            _highlightTile.SetActive(false);
            MovementLine.instance.ClearLine();
            return false;
        }

        return true;
    }
}
