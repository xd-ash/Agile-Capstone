using AStarPathfinding;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AbilityEvents;

public class MouseFunctionManager : MonoBehaviour
{
    public static MouseFunctionManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        _tilemap = FindAnyObjectByType<Tilemap>();
    }

    private Tilemap _tilemap;
    private Vector3Int _tilePos;
    private Vector3 _mousePos;
    private TileBase _currTile;
    private List<PathMarker> _currHighlightedPath;
    private Vector3Int _lastTilePos = new Vector3Int(-1, -1, -1);

    [SerializeField] private Color _mouseTileColor,
                                   _reachablePathColor,
                                   _unreachablePathColor;

    void Update()
    {
        TrackMouse();

        //if no tile is moused over, clear the current path (if any)
        //and set _lastTilePos to value outside the range of the play map
        //(_currtile will never be this value, unless the tilemap is changed).
        if (_currTile == null)
        {
            if (_currHighlightedPath != null)
            {
                //probably dont need this check as methods do null checking where needed
                if (IsTargeting)
                    ClearTileColor(_lastTilePos);
                else
                    ClearTileColor(_currHighlightedPath);
            }
            _lastTilePos = new Vector3Int(-1, -1, -1);
            return;
        }

        if (_lastTilePos != _tilePos)
        {
            //Clear any highlighted tiles once a new tile is selected
            ClearTileColor(_lastTilePos);
            if (_currHighlightedPath != null)
                ClearTileColor(_currHighlightedPath);
        }

        //Do relevant tasks for on mouseover & click
        if (IsTargeting)
            DoTargetingStuff();
        else
            FindMovementPath();
    }
    private void FindMovementPath()
    {
        if (MapCreator.instance.GetByteMap[_tilePos.x, _tilePos.y] != 0) return; // return if mouseover tile pos is not walkable

        // find path to mouseover position and set tile color
        var tempPath = FindPathAStar.instance.OnTileHover((Vector2Int)_tilePos);
        if (tempPath != null)
            SetTileColor(tempPath);

        _currHighlightedPath = tempPath;

        _lastTilePos = _tilePos;

        // Swap me to new input system at some point
        if (Input.GetMouseButtonDown(0))
            FindPathAStar.instance.OnTileClick();
    }
    private void DoTargetingStuff()
    {
        SetTileColor(_tilePos);
        _lastTilePos = _tilePos;

        Debug.LogWarning("DoTargetingStuff not implemented in mouseManager");
    }

    //Tracks mouse position and sets tile position and currtile
    private void TrackMouse()
    {
        Vector3 trueMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos = new Vector3(trueMousePos.x, trueMousePos.y, 0f);

        _tilePos = _tilemap.WorldToCell(_mousePos);
        _currTile = _tilemap.GetTile(_tilePos);
    }

    //Methods to set and clear tile colors, with overloads for affecting only
    //the mouseover tile or the entire current path
    private void SetTileColor(Vector3Int tilePos)
    {
        _tilemap.SetColor(tilePos, _mouseTileColor);
    }
    private void SetTileColor(List<PathMarker> path)
    {
        if (path == null || path.Count == 0) return;

        foreach (PathMarker pm in path)
            if (pm != null)
                _tilemap.SetColor(new Vector3Int(pm.location.x, pm.location.y), pm.isReachable ? _reachablePathColor : _unreachablePathColor);
    }
    private void ClearTileColor(Vector3Int tilePos)
    {
        _tilemap.SetColor(tilePos, Color.white);
    }
    private void ClearTileColor(List<PathMarker> path)
    {
        if (path == null || path.Count == 0) return;

        foreach (PathMarker pm in path)
            if (pm != null)
                ClearTileColor(new Vector3Int(pm.location.x, pm.location.y));
    }
}
