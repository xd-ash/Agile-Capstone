using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeMapCreator : MonoBehaviour
{
    public enum NodeTypes { Combat, BountyBoard, Boss, Shop, Camp, Elite }

    private GameObject _nodePrefab;
    private CustomTileMapSOLibrary _tilemapSOLibrary;

    private List<List<NodePlaceholder>> _tiers = new();

    [SerializeField] private ParticleSystem.MinMaxCurve _possibleNodesPerTier;
    [SerializeField] private int _numberOfTiers = 10;

    [Header("Tier Node Limits")]
    [SerializeField] private int minNodesPerTier = 3;
    [SerializeField] private int maxNodesPerTier = 5;

    [SerializeField] private int maxCampsPerMap = 2;
    [SerializeField] private int maxEliteNodesPerMap = 3;

    [Header("Minimum Required Counts")]
    [SerializeField] private int minCampsPerMap = 1;
    [SerializeField] private int minShopsPerMap = 1;
    [SerializeField] private int minElitePerMap = 1;
    [SerializeField] private int minBountyPerMap = 1;

    [Header("Spacing Constraints (tiers apart)")]
    [SerializeField] private int shopMinSpacing = 3;
    [SerializeField] private int campMinSpacing = 2;
    [SerializeField] private int eliteMinSpacing = 2;

    [SerializeField] private float tierSpacingX = 3f;
    [SerializeField] private float nodeSpacingY = 1.2f;

    [SerializeField] private AnimationCurve combatWeight;
    [SerializeField] private AnimationCurve shopWeight;
    [SerializeField] private AnimationCurve campWeight;
    [SerializeField] private AnimationCurve bountyWeight;
    [SerializeField] private AnimationCurve eliteWeight;

    private int _eliteCount;
    private int _campCount;
    private int _shopCount;
    private int _bountyCount;

    private int _lastShopTier = -999;
    private int _lastCampTier = -999;
    private int _lastEliteTier = -999;

    public static NodeMapCreator Instance { get; private set; }
    public int GetNumberOfTiers { get; internal set; }

    private void Awake()
    {
        Instance = this;
        _nodePrefab = Resources.Load<GameObject>("TempNodeMap/NodePrefab");
        _tilemapSOLibrary = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTileMapSOLibrary");
    }

    public Dictionary<int, List<NodeMapNode>> GenerateFullNodeMap(int seed)
    {
        UnityEngine.Random.InitState(seed);

        _eliteCount = 0;
        _campCount = 0;
        _shopCount = 0;
        _bountyCount = 0;

        _lastShopTier = -999;
        _lastCampTier = -999;
        _lastEliteTier = -999;

        BuildTiers();
        AssignTypes();
        ConnectTiers();

        return BuildVisualMap();
    }

    private void BuildTiers()
    {
        _tiers.Clear();

        for (int i = 0; i < _numberOfTiers; i++)
        {
            int count;

            if (i == 0 || i == _numberOfTiers - 1)
                count = 1;
            else
            {
                count = (int)_possibleNodesPerTier.Evaluate((float)i / _numberOfTiers, UnityEngine.Random.value);
                count = Mathf.Clamp(count, minNodesPerTier, maxNodesPerTier);
            }

            var tier = new List<NodePlaceholder>();

            for (int j = 0; j < count; j++)
            {
                tier.Add(new NodePlaceholder
                {
                    dictIndex = new Vector2Int(i, j)
                });
            }

            LayoutTier(tier, i);
            _tiers.Add(tier);
        }
    }

    private void AssignTypes()
    {
        var flat = _tiers.SelectMany(t => t).ToList();
        Shuffle(flat);

        foreach (var node in flat)
        {
            int tier = node.dictIndex.x;
            float t = (float)tier / (_numberOfTiers - 1);

            if (tier == 0)
            {
                node.nodeType = NodeTypes.Combat;
                continue;
            }

            if (tier == _numberOfTiers - 1)
            {
                node.nodeType = NodeTypes.Boss;
                continue;
            }

            NodeTypes forced = GetForcedType();

            if (forced != NodeTypes.Combat && IsAllowedSpacing(forced, tier))
            {
                node.nodeType = forced;
                RegisterPlacement(forced, tier);
                Increment(forced);
                continue;
            }

            bool allowElite = _eliteCount < maxEliteNodesPerMap && t > 0.3f && t < 0.85f;

            NodeTypes type = RollType(t, allowElite);

            if (!IsAllowedSpacing(type, tier))
                type = NodeTypes.Combat;

            node.nodeType = type;

            RegisterPlacement(type, tier);
            Increment(type);
        }
    }

    private NodeTypes GetForcedType()
    {
        if (_campCount < minCampsPerMap) return NodeTypes.Camp;
        if (_shopCount < minShopsPerMap) return NodeTypes.Shop;
        if (_eliteCount < minElitePerMap) return NodeTypes.Elite;
        if (_bountyCount < minBountyPerMap) return NodeTypes.BountyBoard;
        return NodeTypes.Combat;
    }

    private bool IsAllowedSpacing(NodeTypes type, int tier)
    {
        if (type == NodeTypes.Shop)
            return (tier - _lastShopTier) >= shopMinSpacing;

        if (type == NodeTypes.Camp)
            return (tier - _lastCampTier) >= campMinSpacing;

        if (type == NodeTypes.Elite)
            return (tier - _lastEliteTier) >= eliteMinSpacing;

        return true;
    }

    private void RegisterPlacement(NodeTypes type, int tier)
    {
        if (type == NodeTypes.Shop) _lastShopTier = tier;
        if (type == NodeTypes.Camp) _lastCampTier = tier;
        if (type == NodeTypes.Elite) _lastEliteTier = tier;
    }

    private void Increment(NodeTypes type)
    {
        if (type == NodeTypes.Camp) _campCount++;
        if (type == NodeTypes.Shop) _shopCount++;
        if (type == NodeTypes.Elite) _eliteCount++;
        if (type == NodeTypes.BountyBoard) _bountyCount++;
    }

    private void ConnectTiers()
    {
        for (int i = 0; i < _tiers.Count - 1; i++)
        {
            var current = _tiers[i];
            var next = _tiers[i + 1];

            foreach (var n in next)
            {
                var parent = current[UnityEngine.Random.Range(0, current.Count)];
                n.prev.Add(parent);
                parent.next.Add(n);
            }

            foreach (var n in current)
            {
                if (n.next.Count == 0)
                {
                    var child = next[UnityEngine.Random.Range(0, next.Count)];
                    n.next.Add(child);
                    child.prev.Add(n);
                }
            }
        }
    }

    private Dictionary<int, List<NodeMapNode>> BuildVisualMap()
    {
        var map = new Dictionary<int, List<NodeMapNode>>();

        for (int i = 0; i < _tiers.Count; i++)
        {
            var list = new List<NodeMapNode>();

            for (int j = 0; j < _tiers[i].Count; j++)
            {
                var p = _tiers[i][j];

                GameObject go = Instantiate(_nodePrefab, transform);
                NodeMapNode node = GetNode(p, go);

                RectTransform rect = go.GetComponent<RectTransform>();
                float padding = (rect.rect.width + rect.rect.height) * 0.5f * 1.35f;

                rect.anchoredPosition = p.nodePos * padding;

                list.Add(node);
            }

            map.Add(i, list);
        }

        for (int i = 0; i < _tiers.Count; i++)
        {
            for (int j = 0; j < _tiers[i].Count; j++)
            {
                var p = _tiers[i][j];

                List<NodeMapNode> prev = new();
                List<NodeMapNode> next = new();

                foreach (var n in p.prev)
                    prev.Add(map[n.dictIndex.x][n.dictIndex.y]);

                foreach (var n in p.next)
                    next.Add(map[n.dictIndex.x][n.dictIndex.y]);

                map[i][j].InitNode(new Vector2Int(i, j), prev, next);
            }
        }

        return map;
    }

    private void LayoutTier(List<NodePlaceholder> nodes, int tierIndex)
    {
        float x = tierIndex * tierSpacingX;
        float startY = -(nodes.Count - 1) * nodeSpacingY * 0.5f;

        for (int i = 0; i < nodes.Count; i++)
            nodes[i].nodePos = new Vector2(x, startY + i * nodeSpacingY);
    }

    private NodeTypes RollType(float t, bool allowElite)
    {
        List<(NodeTypes type, float w)> pool = new()
        {
            (NodeTypes.Combat, combatWeight.Evaluate(t)),
            (NodeTypes.Shop, shopWeight.Evaluate(t)),
            (NodeTypes.Camp, campWeight.Evaluate(t)),
            (NodeTypes.BountyBoard, bountyWeight.Evaluate(t))
        };

        if (allowElite)
            pool.Add((NodeTypes.Elite, eliteWeight.Evaluate(t)));

        float total = pool.Sum(p => p.w);
        float r = UnityEngine.Random.value * total;

        float sum = 0;

        foreach (var p in pool)
        {
            sum += p.w;
            if (r <= sum) return p.type;
        }

        return NodeTypes.Combat;
    }

    private NodeMapNode GetNode(NodePlaceholder node, GameObject go)
    {
        switch (node.nodeType)
        {
            case NodeTypes.Combat: return go.AddComponent<CombatNode>();
            case NodeTypes.BountyBoard: return go.AddComponent<BountyBoardNode>();
            case NodeTypes.Boss: return go.AddComponent<BossNode>();
            case NodeTypes.Shop: return go.AddComponent<ShopNode>();
            case NodeTypes.Elite: return go.AddComponent<EliteNode>();
            default: return go.AddComponent<CampNode>();
        }
    }

    private void Shuffle(List<NodePlaceholder> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public class NodePlaceholder
    {
        public Vector2 nodePos;
        public NodeTypes nodeType;
        public Vector2Int dictIndex;

        public List<NodePlaceholder> prev = new();
        public List<NodePlaceholder> next = new();
    }
}