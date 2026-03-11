using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AStarPathfinding;
using static IsoMetricConversions;

public class UnitMovementController : MonoBehaviour
{
    private Unit _unit;
    private DirectionAnimator _dirAnimator;

    private List<PathMarker> _truePath;
    private bool _isMoving = false;
    public bool GetIsMoving => _isMoving;
    public bool _isKnockback = false;

    [SerializeField] private float _unitMoveSpeed;
    [SerializeField] private int _moveCostPerTile = 1;

    //temp? variable to track last position during movement coro to be used for trap knockbacks
    public Vector2Int PrevPosOnMove { get; private set; }

    private Action _onMoveFinish;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _dirAnimator = GetComponent<DirectionAnimator>();
    }

    public void OnKnockback(Vector2Int targetPos)
    {
        StopAllCoroutines();
        _onMoveFinish?.Invoke();

        _isKnockback = true;
        _isMoving = false;
        _dirAnimator?.SetMoving(false);

        CalculatePath(targetPos);
        if (_truePath == null)
        {
            Debug.Log("truepath null");
            return;
        }
        OnStartUnitMove(() => _isKnockback = false);
    }

    //if unit can move, check for reachable tiles within path and flip bool (isReachable) true and return full path.
    public List<PathMarker> CalculatePath(Vector2Int tilePos)
    {
        if (!_isMoving && PauseMenu.isPaused != true)
        {
            //only allow movement on this unit's turn
            if (TurnManager.GetCurrentUnit != _unit && !_isKnockback) return null;

            var position = ConvertToGridFromIsometric(transform.localPosition);
            _truePath = FindPathAStar.CalculatePath(position, tilePos);
                    
            if (!_isKnockback)
            {
                //Flip bool in pathmarker to indicate which tiles are within movement range
                List<PathMarker> tempTrue = _truePath;
                int steps = _truePath != null ? _truePath.Count : 0;
                if (steps > _unit.GetAP)
                {
                    int keep = Mathf.Max(0, _unit.GetAP);
                    tempTrue = _truePath.GetRange(_truePath.Count - keep, keep);
                }
                foreach (PathMarker pm in tempTrue)
                    pm.isReachable = true;
            }

            // return full path to target position
            return _truePath;
        }

        return null;
    }
    //Start unit's movement towards determined goal
    public void OnStartUnitMove(Action onFinished = null)
    {
        if (!_isMoving && PauseMenu.isPaused != true)
            StartCoroutine(MoveCoro(onFinished));
    }

    public IEnumerator MoveCoro(Action onFinished = null)
    {
        // bandaid fix
        _onMoveFinish = onFinished;
        //

        if (_truePath.Count == 0 || _truePath == null) yield break;

        _isMoving = true;

        //Convert unit local position to grid position
        Vector2Int prev = ConvertToGridFromIsometric(_unit.transform.localPosition);

        _dirAnimator?.SetMoving(true);

        for (int i = _truePath.Count - 1; i >= 0; i--)
        {
            //Debug.Log($"truepath count: {_truePath.Count}, index: {i}");
            if (!_isKnockback)
            {
                if (!_unit.CanSpend(1) || !_truePath[i].isReachable)
                    break;
            }
            if (!_unit.GetCanMove)
            {
                _dirAnimator?.SetMoving(false);
                break;
            }

            Vector2Int next = new Vector2Int(_truePath[i].location.x, _truePath[i].location.y);

            // Anim direction set
            Vector2Int delta = next - prev;

            _dirAnimator?.SetDirectionFromDelta(_isKnockback ? -delta : delta);

            Vector3 startPos = _unit.transform.localPosition;
            Vector3 endPos = ConvertToIsometricFromGrid(next);

            float elapsed = 0f;
            while (elapsed < _unitMoveSpeed)
            {
                // If the game is paused, just wait here without progressing the move
                if (PauseMenu.isPaused)
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / _unitMoveSpeed);
                _unit.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            if (!_isKnockback)
            {
                _unit.SpendAP(_moveCostPerTile);
                GameUIManager.instance.UpdateApText();
            }

            ByteMapController.Instance.UpdateUnitPositionByteMap(_unit, prev, next);

            // tile enter event for trap check (make this better?)
            PrevPosOnMove = prev;
            ByteMapController.TileEntered?.Invoke(next, _unit);

            prev = next;
        }

        _dirAnimator?.SetMoving(false);
        _isMoving = false;

        // do onfinished action/method call after movement finishes (used in GOAP unit movement & action completion)
        onFinished?.Invoke();

        // rebuild highlights for player right after movement is fully done
        //if (_unit.GetTeam == Team.Friendly)
            //MovementRangeCalculator.Instance.RebuildForCurrentUnit();
    }
}
