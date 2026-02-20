using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class NewNodeCreation : MonoBehaviour
{
    [SerializeField] private ParticleSystem.MinMaxCurve possibleNodesPerTier = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(), new AnimationCurve());
    [SerializeField] private int _numberOfTiers = 10;

    private Dictionary<int, List<Node>> _nodeTiers = new Dictionary<int, List<Node>>();
    [SerializeField] private int _connectAttempts = 0;

    [SerializeField] private int _randomSeed;
    [Space(10), SerializeField] private GameObject _interectempty;

    public void GenerateNewSeed()
    {
        _randomSeed = UnityEngine.Random.Range(0, int.MaxValue);
    }
    private void Start()
    {
        UnityEngine.Random.InitState(_randomSeed);

        for (int i = 0; i < _numberOfTiers; i++)
        {
            List<Node> nodes = new List<Node>();

            int rng = (int)possibleNodesPerTier.Evaluate((float)i / _numberOfTiers, UnityEngine.Random.Range(0f, 1f));
            for (int j = 0; j < rng; j++)
            {
                Node newNode = new Node()
                {
                    possiblePrev = i > 0 ? new(_nodeTiers[i - 1]) : null,
                    nodePos = new Vector2(i, rng * 0.5f - j)
                };
                nodes.Add(newNode);

                if (i == 0) continue;
                foreach (var node in _nodeTiers[i - 1])
                    node.possibleNext.Add(newNode);
            }

            _nodeTiers.Add(i, nodes);
        }

        StartCoroutine(NodeCoro());
        /*for (int i = 0; i < _nodeTiers.Count; i++)
        {
            var nodeKVP = _nodeTiers.ElementAt(i);

            //Debug.Log($"Tier: {nodeKVP.Key} Start.");

            int c = -1;
            while (!CheckNodesPrevAndNext(nodeKVP) && c < _connectAttempts)
            {
                ConnectNodes(ref nodeKVP);
                c++;
                //DebugNodesSoundOff(nodeKVP);
            }
            if (c >= _connectAttempts)
                Debug.Log($"Tier:{nodeKVP.Key} while loop fail exit.");

            //Debug.Log($"Tier: {nodeKVP.Key} Complete.");
        }*/
    }
    public IEnumerator NodeCoro()
    {
        for (int i = 0; i < _nodeTiers.Count; i++)
        {
            var nodeKVP = _nodeTiers.ElementAt(i);

            //Debug.Log($"Tier: {nodeKVP.Key} Start.");

            int c = -1;
            do
            {
                ConnectNodes(ref nodeKVP);
                c++;
                //DebugNodesSoundOff(nodeKVP);
                yield return new WaitForSecondsRealtime(1f);
                //yield return null;
            } while (!CheckNodesPrevAndNext(ref nodeKVP) && c < _connectAttempts);
            if (c >= _connectAttempts)
                Debug.Log($"Tier:{nodeKVP.Key} while loop fail exit.");

            //Debug.Log($"Tier: {nodeKVP.Key} Complete.");
            //yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            //Debug.Log($"Tier:{nodeKVP.Key}, Count:{nodeKVP.Value.Count}");
        }
    }

    private void ConnectNodes(ref KeyValuePair<int, List<Node>> nodeKVP)
    {
        UnityEngine.Random.InitState(_randomSeed);

        //reset intersect debug markers
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        int curTier = nodeKVP.Key;
        var curNodes = nodeKVP.Value;

        List<Node> prevNodes = null;
        List<Node> nextNodes = null;

        int prevTier = Mathf.Max(nodeKVP.Key - 1, 0);
        if (prevTier != curTier)
            prevNodes = _nodeTiers[prevTier];

        int nextTier = Mathf.Min(nodeKVP.Key + 1, _numberOfTiers - 1);
        if (nextTier != curTier)
            nextNodes = _nodeTiers[nextTier];

        for (int i = 0; i < curNodes.Count; i++)
        {
            var node = curNodes[i];
            //assign prev/input node randomly from possible options
            if (prevNodes != null && node.possiblePrev != null && node.possiblePrev.Count > 0 && node.prev.Count == 0)
            {
                int rng = UnityEngine.Random.Range(0, node.possiblePrev.Count);
                node.prev.Add(node.possiblePrev[rng]);
                prevNodes[rng].next.Add(node);
            }

            //assign next/output node randomly from possible options
            if (nextNodes != null && node.possibleNext != null && node.possibleNext.Count > 0 && node.next.Count == 0)
            {
                int rng = UnityEngine.Random.Range(0, node.possibleNext.Count);
               // Debug.Log($"Node ({node.nodePos.x},{node.nodePos.y}), rng: {rng}");
                node.next.Add(node.possibleNext[rng]);
                nextNodes[rng].prev.Add(node);
            }
            //Debug.Log($"Pos:({node.nodePos.x},{node.nodePos.y}), prevCount:{node.prev.Count}, nextCount:{node.next.Count}");
        }
        /*foreach (var node in curNodes)
        {
            //assign prev/input node randomly from possible options
            if (prevNodes != null && node.possiblePrev != null && node.possiblePrev.Count > 0 && node.prev.Count == 0)
            {
                int rng = UnityEngine.Random.Range(0, node.possiblePrev.Count);
                node.prev.Add(prevNodes[rng]);
                prevNodes[rng].next.Add(node);
            }

            //assign next/output node randomly from possible options
            if (nextNodes != null && node.possibleNext != null && node.possibleNext.Count > 0 && node.next.Count == 0)
            {
                Debug.Log($"Node ({node.nodePos.x},{node.nodePos.y}), possibleNextCount: {node.possibleNext.Count}");
                int rng = UnityEngine.Random.Range(0, node.possibleNext.Count);
                node.next.Add(nextNodes[rng]);
                nextNodes[rng].prev.Add(node);
            }
            //Debug.Log($"Pos:({node.nodePos.x},{node.nodePos.y}), prevCount:{node.prev.Count}, nextCount:{node.next.Count}");
        }*/

        for (int i = 0; i < curNodes.Count; i++)
        {
            Node curNode = curNodes[i];

            for (int j = 0; j < curNodes.Count; j++)
            {
                Node comparedNode = curNodes[j];

                if (curNode == comparedNode) continue;

                //CompareNodesNext(ref curNode, ref comparedNode);
                CompareNodesPrev(ref curNode, ref comparedNode);
            }
        }
    }
    private void DebugNodesSoundOff(KeyValuePair<int, List<Node>> nodeKvp)
    {
        for (int i = 0; i < nodeKvp.Value.Count; i++)
        {
            var node = nodeKvp.Value[i];
            //Debug.Log($"Node ({node.nodePos.x},{node.nodePos.y}), PossPrevCount: {(node.possiblePrev == null ? "null" : node.possiblePrev.Count)}, PossNextCount: {(node.possibleNext == null ? "null" : node.possibleNext.Count)}");
            foreach (var possibleNext in node.possibleNext)
                Debug.Log($"Node ({node.nodePos.x},{node.nodePos.y}), PossibleNextNodePos: ({possibleNext.nodePos.x},{possibleNext.nodePos.y})");
        }
    }
    private void CompareNodesNext(ref Node curNode, ref Node comparedNode)
    {
        for (int i = curNode.next.Count - 1; i >= 0; i--)
        {
            Node curConnection = curNode.next[i];

            for (int j = comparedNode.next.Count - 1; j >= 0; j--)
            {
                Node comparedConnection = comparedNode.next[j];

                if (curConnection == comparedConnection) continue;

                if (FindIntersection(curNode.nodePos, curConnection.nodePos, comparedNode.nodePos, comparedConnection.nodePos, out Vector2 intersectPos))
                {
                    //Debug.Log($"Intersect @ ({intersectPos.x},{intersectPos.y}. curNodePos:{curNode.nodePos}, compareNodePos:{comparedNode.nodePos})");
                    var go = GameObject.Instantiate(_interectempty, Vector3.zero, Quaternion.identity, transform);
                    go.transform.localPosition = intersectPos;
                    go.name = $"({intersectPos.x.ToString("n1")},{intersectPos.y.ToString("n1")}) Intersect";
                    ResolveIntersectionNext(ref curNode, ref curConnection,ref comparedNode,ref comparedConnection);
                }
            }
        }
    }
    private void CompareNodesPrev(ref Node curNode, ref Node comparedNode)
    {
        for (int i = curNode.prev.Count - 1; i >= 0; i--)
        {
            Node curConnection = curNode.prev[i];

            for (int j = comparedNode.prev.Count - 1; j >= 0; j--)
            {
                Node comparedConnection = comparedNode.prev[j];

                if (curConnection.nodePos == comparedConnection.nodePos) continue;

                if (FindIntersection(curNode.nodePos, curConnection.nodePos, comparedNode.nodePos, comparedConnection.nodePos, out Vector2 intersectPos))
                {
                    //Debug.Log($"Intersect @ ({intersectPos.x},{intersectPos.y}. curNodePos:{curNode.nodePos}, compareNodePos:{comparedNode.nodePos})");
                    var go = GameObject.Instantiate(_interectempty, Vector3.zero, Quaternion.identity, transform);
                    go.transform.localPosition = intersectPos;
                    go.name = $"({intersectPos.x.ToString("n1")},{intersectPos.y.ToString("n1")}) Intersect";
                    ResolveIntersectionPrev(ref curNode, ref curConnection,ref comparedNode,ref comparedConnection);
                }
            }
        }
    }

    //return false if any node in current tier has no prev or next nodes. Return false if any node in previous teir has not next node
    //otherwise return true
    private bool CheckNodesPrevAndNext(ref KeyValuePair<int, List<Node>> nodeKVP)
    {
        //check current tier prev/next
        foreach (var node in nodeKVP.Value)
            if (node.prev.Count == 0 && nodeKVP.Key != 0 ||
                node.next.Count == 0 && nodeKVP.Key != _numberOfTiers - 1)
            {
                //Debug.Log($"Node({node.nodePos.x},{node.nodePos.y}), nextCount: {node.next.Count}, prevCount: {node.prev.Count}");
                return false;
            }

        //check prev tier next
        int prevTier = nodeKVP.Key - 1;
        if (prevTier >= 0)
            foreach (var prevNode in _nodeTiers[nodeKVP.Key - 1])
                if (prevNode.next.Count == 0)
                    return false;

        return true;
    }

    private void ResolveIntersectionNext(ref Node curNode,ref Node curConnection,ref Node comparedNode,ref Node comparedConnection)
    {
        float dist1 = Vector2.Distance(curNode.nodePos, curConnection.nodePos);
        float dist2 = Vector2.Distance(comparedNode.nodePos, comparedConnection.nodePos);
        bool curNodeCloser = dist1 <= dist2;

        if (curNodeCloser)
        {
            comparedNode.next.Remove(comparedConnection);
            comparedNode.possibleNext.Remove(comparedConnection);

            comparedConnection.prev.Remove(comparedNode);
            comparedConnection.possiblePrev.Remove(comparedNode);
        }
        else
        {
            curNode.next.Remove(curConnection);
            curNode.possibleNext.Remove(curConnection);

            curConnection.prev.Remove(curNode);
            curConnection.possiblePrev.Remove(curNode);
        }
    }
    private void ResolveIntersectionPrev(ref Node curNode,ref Node curConnection,ref Node comparedNode,ref Node comparedConnection)
    {
        float dist1 = Vector2.Distance(curNode.nodePos, curConnection.nodePos);
        float dist2 = Vector2.Distance(comparedNode.nodePos, comparedConnection.nodePos);
        bool curNodeCloser = dist1 <= dist2;

        if (curNodeCloser)
        {
            comparedNode.prev.Remove(comparedConnection);
            comparedNode.possiblePrev.Remove(comparedConnection);

            comparedConnection.next.Remove(comparedNode);
            comparedConnection.possibleNext.Remove(comparedNode);
        }
        else
        {
            curNode.prev.Remove(curConnection);
            curNode.possiblePrev.Remove(curConnection);

            curConnection.next.Remove(curNode);
            curConnection.possibleNext.Remove(curNode);
        }
    }

    public bool FindIntersection(Vector2 p1_start, Vector2 p1_end, Vector2 p2_start, Vector2 p2_end, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.zero;

        // Direction vectors of the lines
        Vector2 direction1 = p1_end - p1_start;
        Vector2 direction2 = p2_end - p2_start;

        float denominator = (direction2.y * direction1.x) - (direction2.x * direction1.y);

        // If the denominator is zero, the lines are parallel (or collinear)
        if (Mathf.Abs(denominator) < 0.0001f)
            return false;

        float u_a = ((direction2.x * (p1_start.y - p2_start.y)) - (direction2.y * (p1_start.x - p2_start.x))) / denominator;
        float u_b = ((direction1.x * (p1_start.y - p2_start.y)) - (direction1.y * (p1_start.x - p2_start.x))) / denominator;

        // Check if the intersection point is within the bounds of both line segments (0 to 1)
        if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
        {
            // Calculate the intersection point
            intersectionPoint = p1_start + u_a * direction1;
            return true;
        }

        // Otherwise, the lines do not intersect within their segments
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.khaki;

        foreach (var kvp in _nodeTiers)
            foreach (var node in kvp.Value)
            {
                Gizmos.DrawSphere(node.nodePos, 0.2f);

                foreach (var nextNode in node.next)
                    Gizmos.DrawLine(node.nodePos, nextNode.nodePos);
                foreach (var prevNode in node.prev)
                    Gizmos.DrawLine(node.nodePos, prevNode.nodePos);
            }
    }

    public class Node
    {
        public List<Node> prev = new();
        public List<Node> next = new();
        public List<Node> possiblePrev = new();
        public List<Node> possibleNext = new();

        public Vector2 nodePos;
    }
}
[CustomEditor(typeof(NewNodeCreation))]
public class NodeCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate New Seed"))
        {
            var t = (NewNodeCreation)target;
            t.GenerateNewSeed();
        }
    }
}
