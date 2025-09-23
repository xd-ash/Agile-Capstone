using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Analytics;
using TMPro;
namespace AStarPathfinding
{
    public class PathMarker
    {

        public MapLocation location;
        public float G, H, F;
        public GameObject marker;
        public PathMarker parent;

        public PathMarker(MapLocation l, float g, float h, float f, GameObject m, PathMarker p)
        {

            location = l;
            G = g;
            H = h;
            F = f;
            marker = m;
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
        private MapCreator mapCreator;

        public Color closedMaterial = Color.red;
        public Color openMaterial = Color.green;

        [SerializeField] private GameObject start;
        [SerializeField] private GameObject end;
        [SerializeField] private GameObject pathP;

        PathMarker startNode;
        PathMarker goalNode;
        PathMarker lastPos;
        bool done = false;

        List<PathMarker> open = new List<PathMarker>();
        List<PathMarker> closed = new List<PathMarker>();

        private System.Random rng = new System.Random(); // random for shuffle stuff

        void RemoveAllMarkers()
        {

            GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");

            foreach (GameObject m in markers) Destroy(m);
        }

        void BeginSearch()
        {

            done = false;
            RemoveAllMarkers();

            List<MapLocation> locations = new List<MapLocation>();

            for (int y = 1; y < mapCreator.GetByteMap.GetLength(1) - 1; ++y)
            {
                for (int x = 1; x < mapCreator.GetByteMap.GetLength(0) - 1; ++x)
                {

                    if (mapCreator.GetByteMap[x, y] != 1)
                    {
                        locations.Add(new MapLocation(x, y));
                    }
                }
            }
            Shuffle(locations);

            Vector3 startLocation = new Vector3(locations[0].x * maze.scale, locations[0].y * maze.scale, 0.0f);
            startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].y),
                0.0f, 0.0f, 0.0f, Instantiate(start, startLocation, Quaternion.identity), null);

            Vector3 endLocation = new Vector3(locations[1].x * maze.scale, locations[1].y * maze.scale, 0.0f);
            goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].y),
                0.0f, 0.0f, 0.0f, Instantiate(end, endLocation, Quaternion.identity), null);

            open.Clear();
            closed.Clear();
            open.Add(startNode);
            lastPos = startNode;
        }
        public void Shuffle(List<MapLocation> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                MapLocation value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public void Search(PathMarker thisNode)
        {
            if (thisNode == null) return;
            if (thisNode.Equals(goalNode)) { done = true; return; } //goal has been found

            foreach (MapLocation dir in maze.directions)
            {
                MapLocation neighbour = dir + thisNode.location;
                if (maze.map[neighbour.x, neighbour.y] == 1) continue; //if neighbor is wall 
                if (neighbour.x < 1 || neighbour.x >= maze.width || neighbour.y < 1 || neighbour.y >= maze.depth) continue; //if neighbor is out of bounds
                if (IsClosed(neighbour)) continue;

                float newG = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
                float newH = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
                float newF = newG + newH;

                GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, neighbour.y * maze.scale, 0f), Quaternion.identity);

                TextMeshPro[] values = pathBlock.GetComponentsInChildren<TextMeshPro>();
                values[0].text = $"G: {newG:f0}";
                values[1].text = $"H: {newH:f0}";
                values[2].text = $"F: {newF:f0}";

                if (!UpdateMarker(neighbour, newG, newH, newF, thisNode))
                    open.Add(new PathMarker(neighbour, newG, newH, newF, pathBlock, thisNode));
            }

            open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList(); //orders by F val, and then by H val
            PathMarker pm = (PathMarker)open.ElementAt(0);//why not open[0]?
            closed.Add(pm);

            open.RemoveAt(0);
            pm.marker.GetComponent<SpriteRenderer>().color = closedMaterial; //change marker color?

            lastPos = pm;
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

        public bool IsClosed(MapLocation marker)
        {
            foreach (PathMarker p in closed)
            {
                if (p.location.Equals(marker)) return true;
            }
            return false;
        }

        public void GetPath()
        {
            RemoveAllMarkers();
            truePath = new List<PathMarker>();
            PathMarker begin = lastPos; //last post will be goal, then work backwards using parents

            while (!start.Equals(begin) && begin != null)
            {
                //Instantiate(pathP, new Vector3(begin.location.x * maze.scale, begin.location.y * maze.scale, 0f), Quaternion.identity);
                truePath.Add(begin);
                begin = begin.parent;
            }

            //Instantiate(pathP, new Vector3(startNode.location.x * maze.scale, startNode.location.y * maze.scale, 0f), Quaternion.identity);
        }

        public bool algStarted = false;
        public List<PathMarker> truePath;
        public GameObject player;
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.P))
            {
                BeginSearch();
                algStarted = false;
                StopAllCoroutines();
            }
            if (Input.GetKeyDown(KeyCode.C) && !done && !algStarted)
            {
                algStarted = true;
                do
                {
                    Search(lastPos);
                } while (!done);
                GetPath();
                StartCoroutine(MoveCoro());
            }
            if (Input.GetKeyDown(KeyCode.M)) GetPath();
        }

        public IEnumerator MoveCoro()
        {
            for (int i = truePath.Count - 1; i >= 0; i--)
            {
                player.transform.position = new Vector3(truePath[i].location.x, truePath[i].location.y, 0f);
                yield return new WaitForSecondsRealtime(1);
            }
        }
    }
}
