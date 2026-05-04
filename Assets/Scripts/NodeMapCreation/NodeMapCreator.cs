using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeMapCreator : MonoBehaviour
{
    public enum NodeTypes { Combat,/* BountyBoard,*/ Camp, EliteCombat, Shop, Boss }

    private GameObject _nodePrefab;
    private CustomTileMapSOLibrary _tilemapSOLibrary;

    private Dictionary<int, List<NodePlaceholder>> _nodeTiers = new();

    [SerializeField] private ParticleSystem.MinMaxCurve _possibleNodesPerTier = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(), new AnimationCurve());
    [SerializeField] private int _numberOfTiers = 10;

    [SerializeField, Space(5)] private int _maxShopsInPath = 1;
    [SerializeField] private Vector2Int _minMaxTotalShops = new(2, 3);
    private int _totalShops = 0;

    [SerializeField, Space(5)] private int _maxCampsInPath = 2;
    [SerializeField] private Vector2Int _minMaxTotalCamps = new(2, 5);
    private int _totalCamps = 0;

    [SerializeField, Space(5)] private int _maxElitesInPath = 2;
    [SerializeField] private Vector2Int _minMaxTotalElites = new(0, 3);
    private int _totalEliteNodes = 0;

    //[SerializeField, Space(5)] private int _maxBountyBoardInPath = 3;
    //[SerializeField] private Vector2Int _minMaxTotalBountyBoard = new(0, 8);

    private int _curSeed;

    public int GetNumberOfTiers => _numberOfTiers;

    public static NodeMapCreator Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _nodePrefab = Resources.Load<GameObject>("TempNodeMap/NodePrefab");
        _tilemapSOLibrary = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTileMapSOLibrary");
    }

    public Dictionary<int, List<NodeMapNode>> GenerateFullNodeMap(int seed)
    {
        _curSeed = seed;

        Dictionary<int, List<NodeMapNode>> finalNodemap = new();

        int failCounter = 0;
        do
        {
            if (failCounter > 0)
                _curSeed = PlayerDataManager.Instance.GenerateNodeMapSeed();

            GeneratePlaceholderNodeMap();
            failCounter++;
        } while (!CheckNodeMapPaths() && failCounter < 10000);
        if (failCounter >= 10000)
            Debug.LogWarning($"Excessive nodemap regen fails");

        return PopulateNodeMap();
    }

    private void GeneratePlaceholderNodeMap()
    {
        UnityEngine.Random.InitState(_curSeed);

        _nodeTiers.Clear();
        _totalShops = 0;
        _totalEliteNodes = 0;
        _totalCamps = 0;

        for (int i = 0; i < _numberOfTiers; i++)
        {
            List<NodePlaceholder> nodes = new();

            int rng = (int)_possibleNodesPerTier.Evaluate((float)i / _numberOfTiers, UnityEngine.Random.Range(0f, 1f));
            for (int j = 0; j < rng; j++)
            {
                var newNodePlaceholder = new NodePlaceholder()
                {
                    nodePos = new Vector2(i, rng * 0.5f - j - 0.5f),
                    possiblePrev = i > 0 ? new(_nodeTiers[i - 1]) : null,
                    dictIndex = new(i, j)
                };

                nodes.Add(newNodePlaceholder);

                if (i == 0) continue;
                foreach (var node in _nodeTiers[i - 1])
                    node.possibleNext.Add(newNodePlaceholder);
            }

            _nodeTiers.Add(i, nodes);
        }

        for (int i = 0; i < _numberOfTiers; i++)
        {
            var nodeKVP = _nodeTiers.ElementAt(i);

            int c = -1; // fail counter
            do
            {
                ConnectNodes(nodeKVP);
                c++;
            } while (!CheckNodesForConnections(nodeKVP) && c < 100); // arbitrary fail count to avoid engine crashing
            if (c >= 100)
                Debug.Log($"Tier:{nodeKVP.Key} while loop fail exit.");
        }

        //set node types after all connections are complete
        for (int i = 0; i < _nodeTiers.Count; i++)
        {
            for (int j = 0; j < _nodeTiers[i].Count; j++)
            {
                var node = _nodeTiers[i][j];
                ChooseNodeType(ref node);
            }
        }
    }
    private Dictionary<int, List<NodeMapNode>> PopulateNodeMap()
    {
        Dictionary<int, List<NodeMapNode>> trueNodeMap = new();

        for (int i = 0; i < _nodeTiers.Count; i++)
        {
            List<NodeMapNode> nodes = new();

            for (int j = 0; j < _nodeTiers[i].Count; j++)
            {
                var node = _nodeTiers[i][j];
                //ChooseNodeType(ref node);

                GameObject nodeGO = Instantiate(_nodePrefab, transform);
                NodeMapNode nodeMono = GetNodeStrategy(node, ref nodeGO);

                if (nodeGO == null)
                {
                    Debug.Log($"NodeGo null @ ({node.nodePos})");
                    continue;
                }
                var nodeRect = nodeGO.GetComponent<RectTransform>().rect;
                float padding = (nodeRect.width + nodeRect.height) * 0.5f * 1.35f;
                nodeGO.transform.localPosition = node.nodePos * padding;
                nodeGO.name = $"{i}-{nodeGO.transform.localPosition}-{node.nodeType}";

                nodes.Add(nodeMono);
            }

            trueNodeMap.Add(i, nodes);
        }

        for (int i = 0; i < _numberOfTiers; i++)
            for (int j = 0; j < _nodeTiers[i].Count; j++)
                SetNextAndPrevLists(new(i, j), ref trueNodeMap);

        return trueNodeMap;
    }
    private NodeMapNode GetNodeStrategy(NodePlaceholder node, ref GameObject nodeGO)
    {
        switch (node.nodeType)
        {
            case NodeTypes.Combat:
                return nodeGO.AddComponent<CombatNode>();
            //case NodeTypes.BountyBoard:
                //return nodeGO.AddComponent<BountyBoardNode>();
            case NodeTypes.Boss:
                return nodeGO.AddComponent<BossNode>();
            case NodeTypes.Shop:
                return nodeGO.AddComponent<ShopNode>();
            case NodeTypes.EliteCombat:
                return nodeGO.AddComponent<EliteNode>();
            default:
                return nodeGO.AddComponent<CampNode>();
        }
    }
    private void SetNextAndPrevLists(Vector2Int dictIndex, ref Dictionary<int, List<NodeMapNode>> trueNodeDict)
    {
        List<NodeMapNode> tempPrev = new();
        List<NodeMapNode> tempNext = new();
        var nodePlaceholder = _nodeTiers[dictIndex.x][dictIndex.y];

        if (nodePlaceholder.prev != null && nodePlaceholder.prev.Count > 0)
            foreach (var prevNode in nodePlaceholder.prev)
                tempPrev.Add(trueNodeDict[prevNode.dictIndex.x][prevNode.dictIndex.y]);

        if (nodePlaceholder.next != null && nodePlaceholder.next.Count > 0)
            foreach (var nextNode in nodePlaceholder.next)
                tempNext.Add(trueNodeDict[nextNode.dictIndex.x][nextNode.dictIndex.y]);

        var node = trueNodeDict[dictIndex.x][dictIndex.y];
        node.InitNode(dictIndex, tempPrev, tempNext);
        (node as IUseCombatMapData)?.SetCombatData(_tilemapSOLibrary.GetTileMapSOsFromType(CombatMapType.NormalCombat));
    }
    private void ChooseNodeType(ref NodePlaceholder node)
    {
        NodeTypes rngType = NodeTypes.Combat;

        //temp setup for initial node to be an easier combat
        if (node.dictIndex.x == 0)
        {
            node.nodeType = NodeTypes.Combat;
            return;
        }
        else if (node.dictIndex.x == _numberOfTiers - 1) // final node
        {
            node.nodeType = NodeTypes.Boss;
            return;
        }

        int c = -1;
        do
        {
            rngType = GetWeightedNodeTypeFromVal();
            c++;
        } while ((!CheckNodeNeighbourContents(node, rngType) || !CheckNodeByPosition(node, rngType)) && c < 100);
        if (c >= 100)
            Debug.Log($"Node type choose while loop failure. ({node.nodePos})");

        node.nodeType = rngType;

        switch (rngType)
        {
            case NodeTypes.Shop:
                _totalShops++;
                break;
            case NodeTypes.EliteCombat:
                _totalEliteNodes++;
                break;
            case NodeTypes.Camp:
                _totalCamps++;
                break;
            //case NodeTypes.BountyBoard:
                //break;
        }
    }
    private NodeTypes GetWeightedNodeTypeFromVal()
    {
        int rngVal = UnityEngine.Random.Range(0, 100);

        switch (rngVal)
        {
            case >= 90: return NodeTypes.Shop;
            case >= 65: return NodeTypes.Camp;
            case >= 55: return NodeTypes.EliteCombat;
            //case >= 40: return NodeTypes.BountyBoard;
            default: return NodeTypes.Combat;
        }
    }
    private bool CheckNodeByPosition(NodePlaceholder node, NodeTypes type)
    {
        switch (type)
        {
            case NodeTypes.Shop:
                return node.dictIndex.x > (_numberOfTiers - 1) / 2; // no shops int 1st half of nodemap
            case NodeTypes.Camp:
                return node.dictIndex.x > 1; // no camps directly after first combat
            default:
                return true;
        }
    }
    private bool CheckNodeNeighbourContents(NodePlaceholder node, NodeTypes type)
    {
        // check nodes directly above and below from same node type
        NodePlaceholder nodeAbove = null;
        NodePlaceholder nodeBelow = null;

        if (node.dictIndex.y < _nodeTiers[node.dictIndex.x].Count - 1)
            nodeBelow = _nodeTiers[node.dictIndex.x][node.dictIndex.y + 1];
        if (node.dictIndex.y > 0)
            nodeAbove = _nodeTiers[node.dictIndex.x][node.dictIndex.y - 1];

        if (type == NodeTypes.Combat)
            return true;

        if (nodeBelow != null && nodeBelow.nodeType == type ||
            nodeAbove != null && nodeAbove.nodeType == type)
            return false;

        if (node.next != null)
            foreach (var nextNode in node.next)
                if (nextNode.nodeType == type)
                    return false;

        if (node.prev != null)
            foreach (var prevNode in node.prev)
                if (prevNode.nodeType == type ||
                    type == NodeTypes.Camp && prevNode.nodeType == NodeTypes.Shop ||
                    type == NodeTypes.Shop && prevNode.nodeType == NodeTypes.Camp)
                    return false;

        return true;
    }

    private void ConnectNodes(KeyValuePair<int, List<NodePlaceholder>> nodeKVP)
    {
        UnityEngine.Random.InitState(_curSeed);

        int curTier = nodeKVP.Key;
        var curNodes = nodeKVP.Value;

        List<NodePlaceholder> prevNodes = null;
        List<NodePlaceholder> nextNodes = null;

        int prevTier = Mathf.Max(nodeKVP.Key - 1, 0);
        if (prevTier != curTier)
            prevNodes = _nodeTiers[prevTier];

        int nextTier = Mathf.Min(nodeKVP.Key + 1, _numberOfTiers - 1);
        if (nextTier != curTier)
            nextNodes = _nodeTiers[nextTier];

        for (int i = 0; i < curNodes.Count; i++)
        {
            var node = curNodes[i];

            //assign prev node randomly from possible options
            if (prevNodes != null && node.possiblePrev != null && node.possiblePrev.Count > 0 && node.prev.Count == 0)
            {
                int rng = UnityEngine.Random.Range(0, node.possiblePrev.Count);
                var rngNode = node.possiblePrev[rng];
                node.prev.Add(rngNode);
                rngNode.next.Add(node);
            }

            //assign next node randomly from possible options
            if (nextNodes != null && node.possibleNext != null && node.possibleNext.Count > 0 && node.next.Count == 0)
            {
                int rng = UnityEngine.Random.Range(0, node.possibleNext.Count);
                var rngNode = node.possibleNext[rng];
                node.next.Add(rngNode);
                rngNode.prev.Add(node);
            }
        }

        for (int i = 0; i < curNodes.Count; i++)
        {
            NodePlaceholder curNode = curNodes[i];

            for (int j = i + 1; j < curNodes.Count; j++)
            {
                NodePlaceholder comparedNode = curNodes[j];

                if (curNode == comparedNode) continue;

                CompareNodesPrev(curNode, comparedNode);
            }
        }
    }

    private void CompareNodesPrev(NodePlaceholder curNode, NodePlaceholder comparedNode)
    {
        for (int i = curNode.prev.Count - 1; i >= 0; i--)
        {
            NodePlaceholder curConnection = curNode.prev[i];

            for (int j = comparedNode.prev.Count - 1; j >= 0; j--)
            {
                NodePlaceholder comparedConnection = comparedNode.prev[j];

                if (curConnection.nodePos == comparedConnection.nodePos) continue;

                if (FindIntersection(curNode.nodePos, curConnection.nodePos,
                        comparedNode.nodePos, comparedConnection.nodePos, out Vector2 intersectPos))
                    ResolveIntersectionPrev(curNode, curConnection, comparedNode, comparedConnection);
            }
        }
    }

    private void ResolveIntersectionPrev(NodePlaceholder curNode, NodePlaceholder curConnection, NodePlaceholder comparedNode, NodePlaceholder comparedConnection)
    {
        float dist1 = Vector2.Distance(curNode.nodePos, curConnection.nodePos);
        float dist2 = Vector2.Distance(comparedNode.nodePos, comparedConnection.nodePos);
        bool curNodeCloser = dist1 <= dist2;

        if (curNodeCloser)
            RemoveConnection(comparedNode, comparedConnection);
        else
            RemoveConnection(curNode, curConnection);
    }

    private void RemoveConnection(NodePlaceholder startNode, NodePlaceholder connectedNode)
    {
        List<bool> temp = new()
        {
            startNode.prev.Remove(connectedNode),
            startNode.possiblePrev.Remove(connectedNode),

            connectedNode.next.Remove(startNode),
            connectedNode.possibleNext.Remove(startNode),
        };

        /*if (temp.Contains(false))
            Debug.Log($"Connection removal fail. StartNode:({startNode.nodePos}), ConnectedNode:({connectedNode.nodePos})");*/
    }

    //return false if any node in current tier has no prev or next nodes. Return false if any node in previous teir has not next node
    //otherwise return true
    private bool CheckNodesForConnections(KeyValuePair<int, List<NodePlaceholder>> nodeKVP)
    {
        //check current tier prev/next
        foreach (var node in nodeKVP.Value)
            if (node.prev.Count == 0 && nodeKVP.Key != 0 || node.next.Count == 0 && nodeKVP.Key != _numberOfTiers - 1)
                return false;

        //check prev tier next
        int prevTier = nodeKVP.Key - 1;
        if (prevTier >= 0)
            foreach (var prevNode in _nodeTiers[prevTier])
                if (prevNode.next.Count == 0)
                {
                    int rng = UnityEngine.Random.Range(0, prevNode.possibleNext.Count);
                    var rngNode = prevNode.possibleNext[rng];
                    prevNode.next.Add(rngNode);
                    rngNode.prev.Add(prevNode);
                    return false;
                }

        return true;
    }
    private bool FindIntersection(Vector2 p1Start, Vector2 p1End, Vector2 p2Start, Vector2 p2End, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.zero;

        // Direction vectors of the lines
        Vector2 direction1 = p1End - p1Start;
        Vector2 direction2 = p2End - p2Start;

        float denominator = (direction2.y * direction1.x) - (direction2.x * direction1.y);

        // If the denominator is zero, the lines are parallel (or collinear)
        if (Mathf.Abs(denominator) < 0.0001f)
            return false;

        float u_a = ((direction2.x * (p1Start.y - p2Start.y)) - (direction2.y * (p1Start.x - p2Start.x))) / denominator;
        float u_b = ((direction1.x * (p1Start.y - p2Start.y)) - (direction1.y * (p1Start.x - p2Start.x))) / denominator;

        // Check if the intersection point is within the bounds of both line segments (0 to 1)
        if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
        {
            // Calculate the intersection point
            intersectionPoint = p1Start + u_a * direction1;
            return true;
        }

        // Otherwise, the lines do not intersect within their segments
        return false;
    }
    private bool CheckNodeMapPaths()
    {
        var startNode = _nodeTiers[0][0];
        if (startNode == null)
        {
            Debug.Log($"Start node null in nodemap creation?");
            return true;
        }

        Dictionary<int, List<NodePlaceholder>> paths = new();

        WalkPath(startNode, 0, ref paths);

        bool isSuccess = true;

        foreach (var path in paths.Values)
        {
            if (!isSuccess) break;

            int pathShopCounter = 0;
            int pathCampCounter = 0;
            int pathEliteCounter = 0;
            //int pathBountyBoardCounter = 0;

            foreach (var node in path)
            {
                switch (node.nodeType)
                {
                    case NodeTypes.Shop:
                        pathShopCounter++;
                        break;
                    case NodeTypes.Camp:
                        pathCampCounter++;
                        break;
                    case NodeTypes.EliteCombat:
                        pathEliteCounter++;
                        break;
                    //case NodeTypes.BountyBoard:
                        //pathBountyBoardCounter++;
                        //break;
                }

                if (pathShopCounter > _maxShopsInPath || pathCampCounter > _maxCampsInPath || pathEliteCounter > _maxElitesInPath /*|| pathBountyBoardCounter > _maxBountyBoardInPath*/)
                {
                    isSuccess = false;
                    //Debug.Log($"path max failure");
                    break;
                }
            }
        }

        if (isSuccess && (_totalShops < _minMaxTotalShops.x || _totalCamps < _minMaxTotalCamps.x || _totalEliteNodes < _minMaxTotalElites.x ||
                          _totalShops > _minMaxTotalShops.y || _totalCamps > _minMaxTotalCamps.y || _totalEliteNodes > _minMaxTotalElites.y))
        {
            isSuccess = false;
            //Debug.Log($"total minmax failure");
        }

        if (isSuccess)
            foreach (var path in paths.Values)
            {
                string message = $"Path count: {path.Count} - ";
                foreach (var node in path)
                    message += $"{node.nodeType}, ";
                //Debug.Log(message);
            }

        return isSuccess;
    }
    private void WalkPath(NodePlaceholder currNode, int pathIndex, ref Dictionary<int, List<NodePlaceholder>> paths)
    {
        if (!paths.ContainsKey(pathIndex))
            paths.Add(pathIndex, new() { currNode });
        else
            paths[pathIndex].Add(currNode);

        List<NodePlaceholder> curPath = new(paths[pathIndex]);

        for (int i = 0; i < currNode.next.Count; i++)
        {
            var nextNode = currNode.next[i];

            if (i == 0)
            {
                WalkPath(nextNode, pathIndex, ref paths);
                continue;
            }

            int newPathIndex = paths.Count + 1;
            paths.Add(newPathIndex, new(curPath));

            WalkPath(nextNode, newPathIndex, ref paths);
        }
    }
    public class NodePlaceholder
    {
        public Vector2 nodePos;
        public NodeTypes nodeType;
        public Vector2Int dictIndex;

        public List<NodePlaceholder> prev = new();
        public List<NodePlaceholder> next = new();
        public List<NodePlaceholder> possiblePrev = new();
        public List<NodePlaceholder> possibleNext = new();
    }
}
