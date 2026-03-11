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

    public static class FindPathAStar
    {
        private static List<MapLocation> _directions = new List<MapLocation>() { new MapLocation(1,0),
                                                                                 new MapLocation(0,1),
                                                                                 new MapLocation(-1,0),
                                                                                 new MapLocation(0,-1) };

        private static PathMarker _startNode;
        private static PathMarker _goalNode;
        private static PathMarker _lastPos;
        private static bool _isDone = true;

        private static List<PathMarker> _open = new List<PathMarker>();
        private static List<PathMarker> _closed = new List<PathMarker>();

        //Fix with editor script for bool active or dropdown
        [Header("Debug Markers")]
        [SerializeField] private static bool _placePathDebugMarkers = false;
        private static Transform _debugMarkerParent;
        private static GameObject _start = Resources.Load<GameObject>("TempAStarPathMarkers/Start");
        private static GameObject _end = Resources.Load<GameObject>("TempAStarPathMarkers/End");
        private static GameObject _pathP = Resources.Load<GameObject>("TempAStarPathMarkers/PathP");
        private static GameObject _truePMark = Resources.Load<GameObject>("TempAStarPathMarkers/TrueP");

        //Determine and return the path to tile position param. Return null if unit is unable to move,
        //if unit can move, check for reachable tiles within path and flip bool (isReachable) true and return full path.
        public static List<PathMarker> CalculatePath(Vector2Int startPos, Vector2Int endPos)
        {
            if (_isDone)
            {
                BeginSearch(startPos, endPos);
                do
                {
                    Search(_lastPos);
                } while (!_isDone);
                return GetPath();
            }

            Debug.LogError("AStar Path null due to call during calculation (!_isDone)");
            return null;
        }

        private static void BeginSearch(Vector2Int startLocation, Vector2Int endLocation)
        {
            _isDone = false;
            RemoveAllMarkers();

            // Create or grab debug marker parent
            if (_placePathDebugMarkers)
            {
                _debugMarkerParent = MapCreator.Instance.transform.Find("DebugMarkerParent");
                if (_debugMarkerParent == null)
                {
                    _debugMarkerParent = new GameObject("DebugMarkerParent").transform;
                    _debugMarkerParent.parent = MapCreator.Instance.transform;
                    _debugMarkerParent.localPosition = Vector3.zero;
                    _debugMarkerParent.localScale = Vector3.one;
                }
            }

            _startNode = new PathMarker(new MapLocation(startLocation.x, startLocation.y), 0.0f, 0.0f, 0.0f, null);
            _goalNode = new PathMarker(new MapLocation(endLocation.x, endLocation.y), 0.0f, 0.0f, 0.0f, null);

            _open.Clear();
            _closed.Clear();
            _open.Add(_startNode);
            _lastPos = _startNode;

            // Create start/end debug markers if option selected
            if (!_placePathDebugMarkers) return;
            CreateDebugMarker(_start, startLocation);
            CreateDebugMarker(_end, endLocation);
        }
        private static void Search(PathMarker thisNode)
        {
            if (thisNode == null) return;
            if (thisNode.Equals(_goalNode)) //goal has been found
            {
                _isDone = true;
                return;
            }

            Vector2 size = MapCreator.Instance.GetMapSize;
            byte[,] bMap = ByteMapController.Instance.GetByteMap;

            foreach (MapLocation dir in _directions)
            {
                MapLocation neighbour = dir + thisNode.location;
                if (neighbour.x < 0 || neighbour.x >= size.x || neighbour.y < 0 || neighbour.y >= size.y) continue; //if neighbor is out of bounds
                if (bMap[neighbour.x, neighbour.y] == 2 || bMap[neighbour.x, neighbour.y] == 5 || bMap[neighbour.x, neighbour.y] == 3) continue; // if pos is obstacle/enemy
                if (IsClosed(neighbour)) continue;

                float newG = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
                float newH = Vector2.Distance(neighbour.ToVector(), _goalNode.location.ToVector());
                float newF = newG + newH;

                if (_placePathDebugMarkers)
                    CreateDebugMarker(_pathP, new Vector2Int(neighbour.x, neighbour.y));

                if (!UpdateMarker(neighbour, newG, newH, newF, thisNode))
                    _open.Add(new PathMarker(neighbour, newG, newH, newF, thisNode));
            }

            // fully break out of search and about pathfinding
            if (_open.Count == 0)
            {
                _isDone = true;
                return;
            }
            _open = _open.OrderBy(p => p.F).ThenBy(n => n.H).ToList(); //orders by F val, and then by H val
            PathMarker pm = _open[0];
            _closed.Add(pm);
            _open.RemoveAt(0);

            _lastPos = pm;
        }

        private static List<PathMarker> GetPath()
        {
            RemoveAllMarkers();
            var truePath = new List<PathMarker>();
            PathMarker begin = _lastPos; //last pos will be goal, then work backwards using parents

            while (begin != null && !_startNode.Equals(begin))
            {
                if (_placePathDebugMarkers)
                    CreateDebugMarker(_truePMark, new Vector2Int(begin.location.x, begin.location.y));

                truePath.Add(begin);
                begin = begin.parent;
            }

            return truePath;
        }

        private static bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
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
        private static void CreateDebugMarker(GameObject marker, Vector2Int pos)
        {
            GameObject markerGO = GameObject.Instantiate(marker, Vector3.zero, Quaternion.identity, _debugMarkerParent);
            markerGO.transform.localPosition = ConvertToIsometricFromGrid(pos);
        }

        //Removes all debug marker gameobjects
        private static void RemoveAllMarkers()
        {
            if (_debugMarkerParent == null) return;

            for (int i = _debugMarkerParent.childCount - 1; i >= 0; i--)
                GameObject.Destroy(_debugMarkerParent.GetChild(i).gameObject);
        }

        public static bool IsClosed(MapLocation marker)
        {
            foreach (PathMarker p in _closed)
                if (p.location.Equals(marker)) return true;
            return false;
        }
    }
}
