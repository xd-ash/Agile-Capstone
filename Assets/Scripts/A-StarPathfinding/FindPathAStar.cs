using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
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
        //Singleton setup removed
        /*public static FindPathAStar instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);
        }
        */

        private MapCreator _mapCreator;
        private Unit _unit;

        private PathMarker startNode;
        private PathMarker goalNode;
        private PathMarker lastPos;
        private bool done = true;
        private bool _isMoving = false;

        private List<PathMarker> open = new List<PathMarker>();
        private List<PathMarker> closed = new List<PathMarker>();
        private List<PathMarker> truePath;

        [SerializeField] private float _unitMoveSpeed;

        //Fix with editor script for bool active
        [Header("Debug Markers")]
        [SerializeField] private bool _placePathDebugMarkers;
        private GameObject start;
        private GameObject end;
        private GameObject pathP;
        private GameObject truePMark;

        private void Start()
        {
            _unit = GetComponent<Unit>();
            _mapCreator = MapCreator.instance;

            if (_placePathDebugMarkers)
            {
                start = Resources.Load<GameObject>("TempAStarPathMarkers/Start");
                end = Resources.Load<GameObject>("TempAStarPathMarkers/End");
                pathP = Resources.Load<GameObject>("TempAStarPathMarkers/PathP");
                truePMark = Resources.Load<GameObject>("TempAStarPathMarkers/TrueP");
            }
        }

        //Determine and return the path to tile position param. Return null if unit is unable to move,
        //if unit can move, check for reachable tiles within path and flip bool (isReachable) true and return full path.
        public List<PathMarker> CalculatePath(Vector2Int tilePos)
        {
            if (done && !_isMoving && PauseMenu.isPaused != true)
            {
                //only allow movement on this unit's turn
                if (TurnManager.GetCurrentUnit != _unit) return null; 

                BeginSearch(tilePos);
                do
                {
                    Search(lastPos);
                } while (!done);
                GetPath();

                List<PathMarker> tempTrue = truePath;

                int steps = truePath != null ? truePath.Count : 0;
                if (steps > _unit.ap)
                {
                    int keep = Mathf.Max(0, _unit.ap);
                    tempTrue = truePath.GetRange(truePath.Count - keep, keep);
                }

                //Flip bool in pathmarker to indicate which tiles can be moved to and get colored appropriately 
                foreach (PathMarker pm in tempTrue)
                    pm.isReachable = true;

                return truePath;
            }

            return null;
        }

        //Start unit's movement towards determined goal
        public void OnStartUnitMove(Action onFinished = null)
        {
            if (done && !_isMoving && PauseMenu.isPaused != true)
                StartCoroutine(MoveCoro(onFinished));
        }

        void BeginSearch(Vector2Int endLocation)
        {
            done = false;
            RemoveAllMarkers();

            Vector2Int unitPos = ConvertToGridFromIsometric(_unit.transform.localPosition);
            startNode = new PathMarker(new MapLocation(unitPos.x, unitPos.y), 0.0f, 0.0f, 0.0f, null);

            goalNode = new PathMarker(new MapLocation(endLocation.x, endLocation.y), 0.0f, 0.0f, 0.0f, null);

            if (_placePathDebugMarkers)
            {
                Debug.Log("Fix debug marker code in A*");
                /*GameObject startMarkerGO = Instantiate(start, Vector3.zero, Quaternion.identity, transform.Find("UnitMoveEmpty"));
                startMarkerGO.transform.localPosition = startLocation;

                GameObject endMarkerGO = Instantiate(end, Vector3.zero, Quaternion.identity, transform.Find("UnitMoveEmpty"));
                endMarkerGO.transform.localPosition = new Vector3(endLocation.x, endLocation.y, 0); // - on ycomp of end location due to how the grid tool creates grid*/
            }

            open.Clear();
            closed.Clear();
            open.Add(startNode);
            lastPos = startNode;
        }
        public void Search(PathMarker thisNode)
        {
            if (thisNode == null) return;
            if (thisNode.Equals(goalNode)) //goal has been found
            {
                done = true;
                return;
            }

            foreach (MapLocation dir in _mapCreator.GetDirections)
            {
                MapLocation neighbour = dir + thisNode.location;
                if (neighbour.x < 0 || neighbour.x >= _mapCreator.GetMapSize.x || neighbour.y < 0 || neighbour.y >= _mapCreator.GetMapSize.y) continue; //if neighbor is out of bounds
                if (_mapCreator.GetByteMap[neighbour.x, neighbour.y] == 1 || _mapCreator.GetByteMap[neighbour.x, neighbour.y] == 2) continue; // if pos is obstacle/enemy
                //if (_mapCreator.GetByteMap[neighbour.x, neighbour.y] != 0) continue;
                if (IsClosed(neighbour)) continue;

                float newG = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
                float newH = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
                float newF = newG + newH;


                if (_placePathDebugMarkers)
                {
                    Debug.Log("Fix debug marker code in A*");
                    //GameObject pathBlock = Instantiate(pathP, Vector3.zero, Quaternion.identity, transform.Find("UnitMoveEmpty"));
                    //pathBlock.transform.localPosition = new Vector3(neighbour.x * _mapCreator.GetMapScale, neighbour.y * _mapCreator.GetMapScale, 0f);
                }

                if (!UpdateMarker(neighbour, newG, newH, newF, thisNode))
                    open.Add(new PathMarker(neighbour, newG, newH, newF, thisNode));
            }

            open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList(); //orders by F val, and then by H val
            PathMarker pm = open[0];
            closed.Add(pm);

            open.RemoveAt(0);

            lastPos = pm;
        }

        public void GetPath()
        {
            RemoveAllMarkers();
            truePath = new List<PathMarker>();
            PathMarker begin = lastPos; //last post will be goal, then work backwards using parents

            while (!startNode.Equals(begin) && begin != null)
            {
                truePath.Add(begin);
                begin = begin.parent;
            }

            if (_placePathDebugMarkers)
            {
                Debug.Log("Fix debug marker code in A*");
                /*foreach (var tp in truePath)
                {
                    GameObject truePathGO = Instantiate(truePMark, Vector3.zero, Quaternion.identity, transform.Find("UnitMoveEmpty"));
                    truePathGO.transform.localPosition = new Vector3(tp.location.x * _mapCreator.GetMapScale, tp.location.y * _mapCreator.GetMapScale, 0f);
                }*/
            }
        }

        public bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
        {
            foreach (PathMarker p in open)
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
        void RemoveAllMarkers()
        {
            GameObject[] markers = GameObject.FindGameObjectsWithTag("MarkersAStar");
            foreach (GameObject m in markers) Destroy(m);
        }

        public bool IsClosed(MapLocation marker)
        {
            foreach (PathMarker p in closed)
            {
                if (p.location.Equals(marker)) return true;
            }
            return false;
        }

        public IEnumerator MoveCoro(Action onFinished = null)
        {
            if (truePath.Count == 0 || truePath == null) yield break;
            //if (_mapCreator.GetByteMap[truePath[0].location.x, truePath[0].location.y] != 0) //bandaid fix for pathing issues of enemies moving to player
                //truePath.RemoveAt(0);
            
            _isMoving = true;

            var _dirAnimator = _unit.GetComponent<DirectionAnimator>();

            //Convert unit local position to grid position
            Vector2Int prev = ConvertToGridFromIsometric(_unit.transform.localPosition);
            Vector2Int tempStart = prev;
            Vector2Int tempEnd = Vector2Int.zero;

            _dirAnimator.SetMoving(true);

            for (int i = truePath.Count - 1; i >= 0; i--)
            {
                if (!_unit.CanSpend(1) || !truePath[i].isReachable)
                    break;

                Vector2Int next = new Vector2Int(truePath[i].location.x, truePath[i].location.y);
                tempEnd = next;
                Vector2Int delta = next - prev;

                _dirAnimator.SetDirectionFromDelta(delta);

                Vector3 startPos = _unit.transform.localPosition;

                //Convert unit grid position to local position 
                Vector3 endPos = ConvertToIsometricFromGrid(next, startPos.y * .01f);// z pos adjusted with y value to allow for easy layering of sprites
                                                                                     // (.01f holds no significance, just used to keep value small)
                float duration = _unitMoveSpeed;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    _unit.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }

                _unit.SpendAP(1);
                TurnManager.instance.UpdateApText();

                prev = next;
            }

            _dirAnimator.SetMoving(false);
            _isMoving = false;

            _mapCreator.UpdateUnitPositionByteMap(tempStart, tempEnd, _unit);

            // do onfinished action/method call after movement finishes (used in GOAP unit movement & action completion)
            if (onFinished != null)
                onFinished();

            // right after movement is fully done
            if (_unit.team == Team.Friendly)
                MovementRangeHighlighter.instance.RebuildForCurrentUnit();
        }

        /*Movement Showcase Purposes
        public IEnumerator MovementPauseCoro()
        {
            yield return new WaitForSeconds(0.1f);
            Search(lastPos);
            if (done)
            {
                //Debug.Log("done");
                yield return new WaitForSeconds(0.5f);
                GetPath();
            }
            else
            {
                StartCoroutine(MovementPauseCoro());
            }
        } */
    }
}
