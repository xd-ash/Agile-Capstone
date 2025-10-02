using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;

/*
namespace AStarPathfinding2
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
        [SerializeField] private GameObject unit;
        [SerializeField] private float _unitMoveSpeed;

        //Fix with editor script for bool active
        [SerializeField] private bool _placePathDebugMarkers;
        [SerializeField] private GameObject start;
        [SerializeField] private GameObject end;
        [SerializeField] private GameObject pathP;
        //

        private void OnEnable()
        {
            _mapCreator = GetComponent<MapCreator>();
        }

        public void OnTileClick(InputAction.CallbackContext context)
        {
            if (done && !_isMoving && _mapCreator.tileMousePos.x >= 0 && _mapCreator.tileMousePos.y >= 0 &&
                _mapCreator.GetByteMap[_mapCreator.tileMousePos.x, _mapCreator.tileMousePos.y] == 0)
            {
                BeginSearch(_mapCreator.tileMousePos);
                do
                {
                    Search(lastPos);
                } while (!done);
                GetPath();
                StartCoroutine(MoveCoro());
            }
        }
        void BeginSearch(Vector2Int endLocation)
        {
            done = false;
            _isMoving = true;
            RemoveAllMarkers();

            Vector3 startLocation = unit.transform.localPosition * _mapCreator.GetMapScale;

            Vector2Int unitPos = new Vector2Int((int)startLocation.x, (int)startLocation.y);
            startNode = new PathMarker(new MapLocation(unitPos.x, unitPos.y), 0.0f, 0.0f, 0.0f, null);

            endLocation = new Vector2Int((int)(endLocation.x * _mapCreator.GetMapScale), (int)(endLocation.y * _mapCreator.GetMapScale));
            goalNode = new PathMarker(new MapLocation((int)endLocation.x, (int)endLocation.y), 0.0f, 0.0f, 0.0f, null);

            if (_placePathDebugMarkers)
            {
                GameObject startMarkerGO = Instantiate(start, Vector3.zero, Quaternion.identity, transform);
                startMarkerGO.transform.localPosition = startLocation;

                GameObject endMarkerGO = Instantiate(end, Vector3.zero, Quaternion.identity, transform);
                endMarkerGO.transform.localPosition = new Vector3(endLocation.x, endLocation.y, 0); // - on ycomp of end location due to how the grid tool creates grid
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
                _isMoving = true;
                return;
            }

            foreach (MapLocation dir in _mapCreator.GetDirections)
            {
                MapLocation neighbour = dir + thisNode.location;
                if (neighbour.x < 0 || neighbour.x >= _mapCreator.GetMapSize.x || neighbour.y < 0 || neighbour.y >= _mapCreator.GetMapSize.y) continue; //if neighbor is out of bounds
                if (_mapCreator.GetByteMap[neighbour.x, neighbour.y] == 1) continue; // if pos is obstacle
                if (IsClosed(neighbour)) continue;

                float newG = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
                float newH = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
                float newF = newG + newH;

                if (_placePathDebugMarkers)
                {
                    GameObject pathBlock = Instantiate(pathP, Vector3.zero, Quaternion.identity, transform);
                    pathBlock.transform.localPosition = new Vector3(neighbour.x * _mapCreator.GetMapScale, neighbour.y * _mapCreator.GetMapScale, 0f);
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
                unit.transform.localPosition = new Vector3(truePath[i].location.x, truePath[i].location.y, unit.transform.localPosition.z);
                yield return new WaitForSecondsRealtime(_unitMoveSpeed);
            }
            _isMoving = false;
        }
    }
}
*/