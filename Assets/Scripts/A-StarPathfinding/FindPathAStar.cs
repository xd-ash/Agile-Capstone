using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace AStarPathfinding
{
    public class PathMarker
    {
        public MapLocation location;
        public float G, H, F;
        public PathMarker parent;

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
        private MapCreator _mapCreator;

        private PathMarker startNode;
        private PathMarker goalNode;
        private PathMarker lastPos;
        private bool done = true;
        private bool _isMoving = false;

        private List<PathMarker> open = new List<PathMarker>();
        private List<PathMarker> closed = new List<PathMarker>();
        private List<PathMarker> truePath;
        [SerializeField] private Unit _unit; //have some sort of collection of units once enemies/player/NPCs are implemented?
                                             //cycle through based on turn order
        [SerializeField] private float _unitMoveSpeed;


        /*/Fix with editor script for bool active
        [SerializeField] private bool _placePathDebugMarkers;
        [SerializeField] private GameObject start;
        [SerializeField] private GameObject end;
        [SerializeField] private GameObject pathP;
        [SerializeField] private GameObject truePMark;
        /*/

        private void Start()
        {
            _mapCreator = MapCreator.instance;
        }

        /*/Movement Showcase Purposes //
        public IEnumerator MovementPauseCoro()
        {
            yield return new WaitForSeconds(0.1f);
            Search(lastPos);
            if (done)
            {
                Debug.Log("done");
                yield return new WaitForSeconds(0.5f);
                GetPath();
            }
            else
            {
                StartCoroutine(MovementPauseCoro());
            }
        }
        /*/

        public void OnTileClick(InputAction.CallbackContext context)
        {
            if (done && !_isMoving && _mapCreator.tileMousePos.x >= 0 && _mapCreator.tileMousePos.y >= 0 &&
                _mapCreator.GetByteMap[_mapCreator.tileMousePos.x, _mapCreator.tileMousePos.y] == 0)
            {
                if (!TurnManager.IsPlayerTurn) return; // only let the player move on player turn

                BeginSearch(_mapCreator.tileMousePos);
                do
                {
                Search(lastPos);
                } while (!done);
                GetPath();
                int steps = truePath != null ? truePath.Count : 0;
                if (steps > _unit.ap)
                {
                int keep = Mathf.Max(0, _unit.ap);
                if (keep == 0) { _isMoving = false; return; } // no AP to move
                truePath = truePath.GetRange(truePath.Count - keep, keep);
                }

                StartCoroutine(MoveCoro());
            }
        }
        void BeginSearch(Vector2Int endLocation)
        {
            done = false;
            _isMoving = true;
            RemoveAllMarkers();

            Vector3 startLocation = _unit.transform.localPosition * _mapCreator.GetMapScale;

            Vector2Int unitPos = new Vector2Int((int)startLocation.x, (int)startLocation.y);
            startNode = new PathMarker(new MapLocation(unitPos.x, unitPos.y), 0.0f, 0.0f, 0.0f, null);

            endLocation = new Vector2Int((int)(endLocation.x * _mapCreator.GetMapScale), (int)(endLocation.y * _mapCreator.GetMapScale));
            goalNode = new PathMarker(new MapLocation((int)endLocation.x, (int)endLocation.y), 0.0f, 0.0f, 0.0f, null);

            /*
            if (_placePathDebugMarkers)
            {
                GameObject startMarkerGO = Instantiate(start, Vector3.zero, Quaternion.identity, transform.Find("UnitMoveEmpty"));
                startMarkerGO.transform.localPosition = startLocation;

                GameObject endMarkerGO = Instantiate(end, Vector3.zero, Quaternion.identity, transform.Find("UnitMoveEmpty"));
                endMarkerGO.transform.localPosition = new Vector3(endLocation.x, endLocation.y, 0); // - on ycomp of end location due to how the grid tool creates grid
            }
            */

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
                _isMoving = true;
                return;
            }

            foreach (MapLocation dir in _mapCreator.GetDirections)
            {
                MapLocation neighbour = dir + thisNode.location;
                if (neighbour.x < 0 || neighbour.x >= _mapCreator.GetMapSize.x || neighbour.y < 0 || neighbour.y >= _mapCreator.GetMapSize.y) continue; //if neighbor is out of bounds
                if (_mapCreator.GetByteMap[neighbour.x, neighbour.y] == 1 || _mapCreator.GetByteMap[neighbour.x, neighbour.y] == 2) continue; // if pos is obstacle/enemy
                if (IsClosed(neighbour)) continue;

                float newG = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
                float newH = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
                float newF = newG + newH;

                /*
                if (_placePathDebugMarkers)
                {
                    GameObject pathBlock = Instantiate(pathP, Vector3.zero, Quaternion.identity, transform.Find("UnitMoveEmpty"));
                    pathBlock.transform.localPosition = new Vector3(neighbour.x * _mapCreator.GetMapScale, neighbour.y * _mapCreator.GetMapScale, 0f);
                }
                */

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

            /*
            if (_placePathDebugMarkers)
            {
                foreach (var tp in truePath)
                {
                    GameObject truePathGO = Instantiate(truePMark, Vector3.zero, Quaternion.identity, transform.Find("UnitMoveEmpty"));
                    truePathGO.transform.localPosition = new Vector3(tp.location.x * _mapCreator.GetMapScale, tp.location.y * _mapCreator.GetMapScale, 0f);
                }
            }*/
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

        public IEnumerator MoveCoro()
        {
            for (int i = truePath.Count - 1; i >= 0; i--)
            {
                if (!_unit.CanSpend(1))
                {
                    break;
                }
                _unit.transform.localPosition = new Vector3(truePath[i].location.x, truePath[i].location.y, _unit.transform.localPosition.z);
                _unit.SpendAP(1);
                TurnManager.instance.UpdateApText();

                yield return new WaitForSecondsRealtime(_unitMoveSpeed);
            }
            _isMoving = false;
        }
    }
}
