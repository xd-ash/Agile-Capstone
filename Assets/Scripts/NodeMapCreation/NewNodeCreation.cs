using System.Collections.Generic;
using UnityEngine;

public class NewNodeCreation : MonoBehaviour
{
    [SerializeField] private ParticleSystem.MinMaxCurve possibleNodesPerTier = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(), new AnimationCurve());
    [SerializeField] private int _numberOfTiers = 10;

    private Dictionary<int, List<Node>> _nodeTiers = new Dictionary<int, List<Node>>();

    private void Start()
    {
        for (int i = 0; i < _numberOfTiers; i++)
        {
            List<Node> nodes = new List<Node>();

            int rng = (int)possibleNodesPerTier.Evaluate((float)i / _numberOfTiers, Random.Range(0f, 1f));
            for (int j = 0; j < rng; j++)
            {
                Node newNode = new Node()
                {
                    possiblePrev = i > 0 ? _nodeTiers[i - 1] : null,
                    nodePos = new Vector2(i, rng * 0.5f - j)
                };
                nodes.Add(newNode);

                if (i == 0) continue;
                foreach (var node in _nodeTiers[i - 1])
                    node.possibleNext.Add(newNode);
            }

            _nodeTiers.Add(i, nodes);
        }

        foreach (var nodeKVP in _nodeTiers)
        {
            ConnectNodes(nodeKVP);
        }
    }
    private void ConnectNodes(KeyValuePair<int, List<Node>> nodeKVP)
    {
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

        foreach (var node in curNodes)
        {
            //assign prev/input node randomly from possible options
            if (prevNodes != null && node.possiblePrev != null && node.possiblePrev.Count > 0 && node.prev.Count == 0)
            {
                int rng = Random.Range(0, node.possiblePrev.Count);
                node.prev.Add(prevNodes[rng]);
                prevNodes[rng].next.Add(node);
            }

            //assign next/output node randomly from possible options
            if (nextNodes != null && node.possibleNext != null && node.possibleNext.Count > 0 && node.next.Count == 0)
            {
                int rng = Random.Range(0, node.possibleNext.Count);
                node.next.Add(nextNodes[rng]);
                nextNodes[rng].prev.Add(node);
            }
            //Debug.Log($"Pos:({node.nodePos.x},{node.nodePos.y}), prevCount:{node.prev.Count}, nextCount:{node.next.Count}");
        }
        
        for (int i = 0; i < curNodes.Count; i++)
        {
            Node curNode = curNodes[i];

            for (int j = 0; j < curNodes.Count; j++)
            {
                Node comparedNode = curNodes[j];

                if (j == i) continue;

                for (int k = curNode.next.Count - 1; k >= 0; k--)
                {
                    Node curConnection = curNode.next[k];

                    for (int l = comparedNode.next.Count - 1; l >= 0; l--)
                    {
                        Node comparedConnection = comparedNode.next[l];

                        if (FindIntersection(curNode.nodePos, curConnection.nodePos, comparedNode.nodePos, comparedConnection.nodePos, out Vector2 intersectPos))
                        {
                            Debug.Log($"Intersect @ ({intersectPos.x},{intersectPos.y}. curNodePos:{curNode.nodePos}, compareNodePos:{comparedNode.nodePos})");
                            ResolveIntersection(curNode, curConnection, comparedNode, comparedConnection);
                            ConnectNodes(nodeKVP);
                            return;
                        }
                    }
                }
            }
        }
    }

    private void ResolveIntersection(Node curNode, Node curConnection, Node comparedNode, Node comparedConnection)
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

                foreach (var output in node.next)
                    Gizmos.DrawLine(node.nodePos, output.nodePos);
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
