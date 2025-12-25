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
    private Transform _highlightObjectParent;
    private GameObject _highlightTile;
    private Vector3Int _tilePos;
    private TileBase _currTile;

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
        if (_highlightObjectParent == null)
            _highlightObjectParent = FindAnyObjectByType<Grid>().transform.Find("HighlightObjParent");

        InitializeTileHighlight();
    }

    private void InitializeTileHighlight()
    {
        _highlightTile = Instantiate(Resources.Load<GameObject>("HighlightTile"), _highlightObjectParent);
        _highlightTile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _highlightTile.transform.localScale = Vector3.one;
        var sr = _highlightTile.GetComponentInChildren<SpriteRenderer>();
        sr.color = _mouseTileColor;
        sr.sortingOrder = 2;
        _highlightTile.SetActive(false);
    }

    private void Update()
    {
        // Quick fix for right clicking to cancel activated attack card/ability
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

        //Left click 
        if (IsTargeting || !MovementLine.instance.DrawMovementPath())
            MovementLine.instance.ClearLine();
    }

    // return true if mouse crosses tile border
    private bool TrackMouse()
    {
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldMouse.z = 0f;

        _tilePos = _tilemap.WorldToCell(worldMouse);
        var tempTile = _tilemap.GetTile(_tilePos);

        if (tempTile == null)
        {
            _highlightTile.SetActive(false);
            _currTile = null;
            MovementLine.instance.ClearLine();
            return false;
        }

        _currTile = tempTile;
        return true;
    }
}
