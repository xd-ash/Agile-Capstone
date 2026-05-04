using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public abstract class NodeMapNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected LineRenderer _lineRenderer;
    protected Button _button;
    protected Image _background;

    protected Vector2Int _nodeIndex;

    [SerializeField] protected List<NodeMapNode> _prev = new();
    [SerializeField] protected List<NodeMapNode> _next = new();

    [SerializeField] protected bool _isNodeCompleted;
    [SerializeField] protected bool _isNodeAccessible;

    [SerializeField] protected Reward _nodeRewards;

    protected GameObject _posterContent;
    protected TextMeshProUGUI _wantedLabel;
    protected Transform _silhouetteContainer;
    protected Image _nodeTypeOverlay;
    
    protected string[] _enemyDisplayNames;
    protected UnitSO[] _enemyDisplaySOs;
    protected bool _isBossNode;
    
    public NodeMapNode[] GetPrevNodes => _prev.ToArray();
    public NodeMapNode[] GetNextNodes => _next.ToArray();
    public Vector2Int GetNodeIndex => _nodeIndex;

    public bool IsNodeCompleted {  get { return _isNodeCompleted; } set { _isNodeCompleted = value; } }
    public bool IsNodeAccessible { get { return _isNodeAccessible; } set { _isNodeAccessible = value; } }

    public Reward GetNodeRewards => _nodeRewards;
    
    public string[] GetEnemyDisplayNames => _enemyDisplayNames;
    public UnitSO[] GetEnemyDisplaySOs => _enemyDisplaySOs;
    public bool GetIsBossNode => _isBossNode;

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
        _button?.onClick.RemoveAllListeners();
        _button?.onClick.AddListener(OnClick);
        _background = GetComponent<Image>();
        
        var posterContentTransform = transform.Find("PosterContent");
        if (posterContentTransform != null)
        {
            _posterContent = posterContentTransform.gameObject;
            _wantedLabel = posterContentTransform.Find("WantedLabel")?.GetComponent<TextMeshProUGUI>();
            _silhouetteContainer = posterContentTransform.Find("SilhouetteContainer");
            _posterContent.SetActive(false); // disabled until a combat node activates it
        }
        var overlayTransform = transform.Find("NodeTypeOverlay");
        if (overlayTransform != null)
        {
            _nodeTypeOverlay = overlayTransform.GetComponent<Image>();
            _nodeTypeOverlay.gameObject.SetActive(false);
        }
        
        SetButtonIconFromType();
        SetNodeRewards();
        NodeMapManager.RefreshNodeVisuals += RefreshNodeVisual;
    }

    private void SetNodeRewards()
    {
        if (this is ShopNode || this is CampNode || 
            NodeMapCreator.Instance != null && _nodeIndex.x == NodeMapCreator.Instance.GetNumberOfTiers - 1) 
            return;

        _nodeRewards = RewardsController.DetermineRewards(_nodeIndex, this is EliteNode);
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
            case EliteNode:
                return "Combat";
            case CampNode:
            default:
                return string.Empty;
        }
    }
    protected virtual void SetButtonIconFromType()
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
            case EliteNode:
                _background.sprite = Resources.Load<Sprite>("TempNodeMap/Nodeicons/EliteBounty");
                break;
            case CampNode:
                _background.sprite = Resources.Load<Sprite>("TempNodeMap/Nodeicons/CampfireNodeIcon");
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

    public virtual void OnClick()
    {
        PlayerDataManager.Instance.SetCurrNodeReward(_nodeRewards);
        RewardOnHoverDisplay.OnClearRewardDisplay?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (this is CombatNode || this is EliteNode /*|| this is CombatNode ||*/ )
            RewardOnHoverDisplay.OnRewardNodeHover?.Invoke(this);
        
        if (_enemyDisplayNames != null && _enemyDisplayNames.Length > 0)
            CombatNodeTooltip.OnShowTooltip?.Invoke(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        RewardOnHoverDisplay.OnClearRewardDisplay?.Invoke();
        CombatNodeTooltip.OnHideTooltip?.Invoke();
    }
    
    protected void PopulateCombatPoster(UnitSO[] enemies, string posterTitle, string[] names, bool isBoss)
    {
        _enemyDisplayNames = names;
        _enemyDisplaySOs = enemies;
        _isBossNode = isBoss;

        if (_posterContent == null) return;
        _posterContent.SetActive(true);

        if (_wantedLabel != null)
            _wantedLabel.text = posterTitle;

        _background.sprite = Resources.Load<Sprite>("TempNodeMap/Nodeicons/WantedPoster");

        if (_silhouetteContainer != null)
        {
            for (int i = _silhouetteContainer.childCount - 1; i >= 0; i--)
                Destroy(_silhouetteContainer.GetChild(i).gameObject);

            //spawn one silhouette per enemy
            foreach (var enemy in enemies)
            {
                var silGO = new GameObject($"Sil_{enemy.GetUnitType}", typeof(RectTransform), typeof(Image));
                silGO.transform.SetParent(_silhouetteContainer, false);
                var img = silGO.GetComponent<Image>();
                string silPath = isBoss
                    ? $"TempNodeMap/Nodeicons/Silhouette_{enemy.GetUnitType}_Boss"
                    : $"TempNodeMap/Nodeicons/Silhouette_{enemy.GetUnitType}";
                img.sprite = Resources.Load<Sprite>(silPath);
                img.preserveAspect = true;
            }
        }
    }

    protected void SetNodeTypeBadge(string badgeSpritePath, Color badgeColor)
    {
        if (_nodeTypeOverlay == null) return;
        _nodeTypeOverlay.gameObject.SetActive(true);
        _nodeTypeOverlay.sprite = Resources.Load<Sprite>(badgeSpritePath);
        _nodeTypeOverlay.color = badgeColor;
    }
}