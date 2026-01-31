using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static IsoMetricConversions;

namespace AStarPathfinding
{
    public class PathMarker
    {
        public MapLocation location;
        public float G, H, F;
        public PathMarker parent;
        public bool isReachable = false;

        public PathMarker(MapLocation l, float g, float h, float f, PathMarker p)
        {
            location = l;
            G = g;
            H = h;
            F = f;
            parent = p;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                return false;
            else
                return location.Equals(((PathMarker)obj).location);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class FindPathAStar : MonoBehaviour
    {
        private Unit _unit;
        private DirectionAnimator _dirAnimator;

        private PathMarker _startNode;
        private PathMarker _goalNode;
        private PathMarker _lastPos;
        private bool _isDone = true;
        private bool _isMoving = false;

        private List<PathMarker> _open = new List<PathMarker>();
        private List<PathMarker> _closed = new List<PathMarker>();
        private List<PathMarker> _truePath;

        [SerializeField] private float _unitMoveSpeed;
        [SerializeField] private int _moveCostPerTile = 1;

        //Fix with editor script for bool active
        [Header("Debug Markers")]
        [SerializeField] private bool _placePathDebugMarkers = false; // deserialized b/c the marker placement code isn't fixed yet
        private Transform _debugMarkerParent;
        private GameObject _start;
        private GameObject _end;
        private GameObject _pathP;
        private GameObject _truePMark;

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _dirAnimator = _unit.GetComponent<DirectionAnimator>();

            // Debug path marker grab
            _start = Resources.Load<GameObject>("TempAStarPathMarkers/Start");
            _end = Resources.Load<GameObject>("TempAStarPathMarkers/End");
            _pathP = Resources.Load<GameObject>("TempAStarPathMarkers/PathP");
            _truePMark = Resources.Load<GameObject>("TempAStarPathMarkers/TrueP");
        }

        //Determine and return the path to tile position param. Return null if unit is unable to move,
        //if unit can move, check for reachable tiles within path and flip bool (isReachable) true and return full path.
        public List<PathMarker> CalculatePath(Vector2Int tilePos)
        {
            if (_isDone && !_isMoving && PauseMenu.isPaused != true)
            {
                //only allow movement on this unit's turn
                if (TurnManager.GetCurrentUnit != _unit) return null; 

                BeginSearch(tilePos);
                do
                {
                    Search(_lastPos);
                } while (!_isDone);
                GetPath();

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

                // return full path to target position
                return _truePath;
            }

            return null;
        }

        //Start unit's movement towards determined goal
        public void OnStartUnitMove(Action onFinished = null)
        {
            if (_isDone && !_isMoving && PauseMenu.isPaused != true)
                StartCoroutine(MoveCoro(onFinished));
        }

        void BeginSearch(Vector2Int endLocation)
        {
            _isDone = false;
            RemoveAllMarkers();

            // Create or grab debug marker parent
            if (_placePathDebugMarkers)
            {
                _debugMarkerParent = MapCreator.instance.transform.Find("DebugMarkerParent");
                if (_debugMarkerParent == null)
                {
                    _debugMarkerParent = new GameObject("DebugMarkerParent").transform;
                    _debugMarkerParent.parent = MapCreator.instance.transform;
                    _debugMarkerParent.localPosition = Vector3.zero;
                    _debugMarkerParent.localScale = Vector3.one;
                }
            }

            Vector2Int unitPos = ConvertToGridFromIsometric(_unit.transform.localPosition);
            _startNode = new PathMarker(new MapLocation(unitPos.x, unitPos.y), 0.0f, 0.0f, 0.0f, null);
            _goalNode = new PathMarker(new MapLocation(endLocation.x, endLocation.y), 0.0f, 0.0f, 0.0f, null);

            _open.Clear();
            _closed.Clear();
            _open.Add(_startNode);
            _lastPos = _startNode;

            // Create start/end debug markers if option selected
            if (!_placePathDebugMarkers) return;
            CreateDebugMarker(_start, unitPos);
            CreateDebugMarker(_end, endLocation);
        }
        public void Search(PathMarker thisNode)
        {
            if (thisNode == null) return;
            if (thisNode.Equals(_goalNode)) //goal has been found
            {
                _isDone = true;
                return;
            }

            Vector2 size = MapCreator.instance.GetMapSize;
            byte[,] bMap = MapCreator.instance.GetByteMap;

            foreach (MapLocation dir in MapCreator.instance.GetDirections)
            {
                MapLocation neighbour = dir + thisNode.location;
                if (neighbour.x < 0 || neighbour.x >= size.x || neighbour.y < 0 || neighbour.y >= size.y) continue; //if neighbor is out of bounds
                if (bMap[neighbour.x, neighbour.y] == 1 || bMap[neighbour.x, neighbour.y] == 2) continue; // if pos is obstacle/enemy
                if (IsClosed(neighbour)) continue;

                float newG = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
                float newH = Vector2.Distance(neighbour.ToVector(), _goalNode.location.ToVector());
                float newF = newG + newH;

                if (_placePathDebugMarkers)
                    CreateDebugMarker(_pathP, new Vector2Int(neighbour.x, neighbour.y));

                if (!UpdateMarker(neighbour, newG, newH, newF, thisNode))
                    _open.Add(new PathMarker(neighbour, newG, newH, newF, thisNode));
            }

            _open = _open.OrderBy(p => p.F).ThenBy(n => n.H).ToList(); //orders by F val, and then by H val
            PathMarker pm = _open[0];
            _closed.Add(pm);
            _open.RemoveAt(0);

            _lastPos = pm;
        }

        public void GetPath()
        {
            RemoveAllMarkers();
            _truePath = new List<PathMarker>();
            PathMarker begin = _lastPos; //last pos will be goal, then work backwards using parents

            while (!_startNode.Equals(begin) && begin != null)
            {
                if (_placePathDebugMarkers)
                    CreateDebugMarker(_truePMark, new Vector2Int(begin.location.x, begin.location.y));

                _truePath.Add(begin);
                begin = begin.parent;
            }
        }

        public bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
        {
            foreach (PathMarker p in _open)
            {
                if (p.location.Equals(pos))
                {
                    p.G = g;
                    p.H = h;
                    p.F = f;
                    p.parent = prt;
                    return true;
                }
            }
            return false;
        }

        // Create debug marker
        private void CreateDebugMarker(GameObject marker, Vector2Int pos)
        {
            Vector3 isoPos = ConvertToIsometricFromGrid(new Vector3(pos.x, pos.y));
            GameObject markerGO = Instantiate(marker, Vector3.zero, Quaternion.identity, _debugMarkerParent);
            markerGO.transform.localPosition = ConvertToIsometricFromGrid(pos);
        }

        //Removes all debug marker gameobjects
        void RemoveAllMarkers()
        {
            if (_debugMarkerParent == null) return;

            for (int i = _debugMarkerParent.childCount - 1; i >= 0; i--)
                Destroy(_debugMarkerParent.GetChild(i).gameObject);
        }

        public bool IsClosed(MapLocation marker)
        {
            foreach (PathMarker p in _closed)
                if (p.location.Equals(marker)) return true;
            return false;
        }

        public IEnumerator MoveCoro(Action onFinished = null)
        {
            if (_truePath.Count == 0 || _truePath == null) yield break;

            _isMoving = true;

            //Convert unit local position to grid position
            Vector2Int prev = ConvertToGridFromIsometric(_unit.transform.localPosition);
            Vector2Int tempStart = prev;
            Vector2Int tempEnd = Vector2Int.zero;

            _dirAnimator?.SetMoving(true);

            for (int i = _truePath.Count - 1; i >= 0; i--)
            {
                if (!_unit.CanSpend(1) || !_truePath[i].isReachable)
                    break;

                Vector2Int next = new Vector2Int(_truePath[i].location.x, _truePath[i].location.y);
                tempEnd = next;

                // Anim direction set
                Vector2Int delta = next - prev;
                _dirAnimator?.SetDirectionFromDelta(delta);

                Vector3 startPos = _unit.transform.localPosition;
                Vector3 endPos = ConvertToIsometricFromGrid(next, startPos.y * .01f);// z pos adjusted with y value to allow for easy layering of sprites
                                                                                     // (.01f holds no significance, just used to keep value small)
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

                _unit.SpendAP(_moveCostPerTile);
                //TurnManager.instance.UpdateApText();
                GameUIManager.instance.UpdateApText();

                prev = next;
            }

            _dirAnimator?.SetMoving(false);
            _isMoving = false;

            MapCreator.instance.UpdateUnitPositionByteMap(tempStart, tempEnd, _unit);

            // do onfinished action/method call after movement finishes (used in GOAP unit movement & action completion)
            if (onFinished != null)
                onFinished();

            // rebuild highlights for player right after movement is fully done
            if (_unit.GetTeam == Team.Friendly)
                MovementRangeHighlighter.instance.RebuildForCurrentUnit();
        }
    }
}
