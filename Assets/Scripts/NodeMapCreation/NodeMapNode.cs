using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class NodeMapNode : MonoBehaviour
{
    protected LineRenderer _lineRenderer;
    protected Button _button;
    protected Image _background;

    protected Vector2Int _nodeIndex;

    [SerializeField] protected List<NodeMapNode> _prev = new();
    [SerializeField] protected List<NodeMapNode> _next = new();

    [SerializeField] protected bool _isNodeCompleted;
    [SerializeField] protected bool _isNodeAccessible;

    public NodeMapNode[] GetPrevNodes => _prev.ToArray();
    public NodeMapNode[] GetNextNodes => _next.ToArray();
    public Vector2Int GetNodeIndex => _nodeIndex;

    public bool IsNodeCompleted {  get { return _isNodeCompleted; } set { _isNodeCompleted = value; } }
    public bool IsNodeAccessible { get { return _isNodeAccessible; } set { _isNodeAccessible = value; } }

    public virtual string GetTargetScene => GrabTargetSceneFromType();

    public virtual void InitNode(Vector2Int index, List<NodeMapNode> prev, List<NodeMapNode> next)
    {
        _nodeIndex = index;
        _prev = prev;
        _next = next;

        _lineRenderer = GetComponentInChildren<LineRenderer>();
        var positions = GetLineRendererPositions();
        _lineRenderer.positionCount = positions.Length;
        _lineRenderer.SetPositions(positions);

        _button = GetComponent<Button>();
        _button?.onClick.AddListener(OnClick);
        _background = GetComponent<Image>();

        SetButtonIconFromType();


        NodeMapManager.RefreshNodeVisuals += RefreshNodeVisual;
    }
    protected void OnDestroy()
    {
        NodeMapManager.RefreshNodeVisuals -= RefreshNodeVisual;
    }

    // Add connection positions to line renderer, with this node's position every other entry.
    // may need to just split into different line renderers
    protected virtual Vector3[] GetLineRendererPositions()
    {
        List<Vector3> positions = new() { transform.position };

        foreach(var node in _prev)
        {
            positions.Add(node.transform.position);
            positions.Add(transform.position);
        }
        foreach(var node in _next)
        {
            positions.Add(node.transform.position);
            positions.Add(transform.position);
        }

        return positions.ToArray();
    }
    
    protected virtual string GrabTargetSceneFromType()
    {
        switch (this)
        {
            case CombatNode:
                return "Combat";
            case BountyBoardNode:
                return "Combat";
            case BossNode:
                return "Combat";
            case ShopNode:
                return "Shop";
            case OtherNode:
            default:
                return string.Empty;
        }
    }
    protected void SetButtonIconFromType()
    {
        switch (this)
        {
            case CombatNode: //comabt node sets icons based on combat data loaded later in script
                break;
            case BountyBoardNode:
                _background.sprite = Resources.Load<Sprite>("TempNodeMap/Nodeicons/BountyBoard");
                break;
            case BossNode:
                _background.sprite = Resources.Load<Sprite>("TempNodeMap/Nodeicons/BossBounty");
                break;
            case ShopNode:
                _background.sprite = Resources.Load<Sprite>("TempNodeMap/Nodeicons/ShopIcon");
                break;
            case OtherNode:
                _background.sprite = Resources.Load<Sprite>("TempNodeMap/Nodeicons/MoneyBag1");
                break;
            default:
                break;
        }
    }
    protected virtual void EnterNodeScene()
    {
        var scene = GrabTargetSceneFromType();

        PlayerDataManager.Instance.UpdateNodeData(_nodeIndex);

        SaveLoadScript.SaveGame?.Invoke(); //save on node selection/enter
        TransitionScene.Instance.StartTransition(scene);
    }

    protected virtual void RefreshNodeVisual()
    {
        if (_button == null || NodeMapManager.Instance == null) return;
        
        // interactable only if accessible and not already completed
        _button.interactable = _isNodeAccessible && !_isNodeCompleted;
        var nmm = NodeMapManager.Instance;

        if (_background == null) return;

        Color c;
        if (_isNodeCompleted)
            c = nmm.GetCompletedNodeColor;
        else if (!_isNodeAccessible)
            c = nmm.GetLockedNodeColor;
        else
            c = Color.white;

        _background.color = c;
    }

    public abstract void OnClick();
}