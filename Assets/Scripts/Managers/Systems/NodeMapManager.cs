using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeMapManager : MonoBehaviour
{
    private NodeMapCreator _nodeMapCreator;
    //[SerializeField] private BountySelectPanelScript _bountySelectPanel;

    [SerializeField] private Color _completedNodeColor, _lockedNodeColor;

    private Dictionary<int, List<NodeMapNode>> _nodeMap = new();

    private int _seed;

    private bool _isNodeMapComplete = false;

    public Color GetCompletedNodeColor => _completedNodeColor;
    public Color GetLockedNodeColor => _lockedNodeColor;
    public bool GetIsNodeMapComplete => _isNodeMapComplete;

    public static Action NodeMapComplete = () => TransitionScene.Instance.StartTransition();
    public static Action RefreshNodeVisuals;

    public static NodeMapManager Instance { get; private set; }
    private void Awake()
    {
        //Basic singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //_bountySelectPanel = FindFirstObjectByType<BountySelectPanelScript>(FindObjectsInactive.Include);
    }
    private void Start()
    {
        _nodeMapCreator = GetComponent<NodeMapCreator>();

        _seed = PlayerDataManager.Instance.GetNodeMapSeed;
        _nodeMap = _nodeMapCreator.GenerateFullNodeMap(_seed);
        InitNodes();
    }
    public void InitNodes()
    {
        var pdm = PlayerDataManager.Instance;
        var completedNodes = pdm.GetCompletedNodes ?? new Vector2Int[0];
        var curNodeIndex = pdm.GetCurrentNodeIndex;
        NodeMapNode curNode = null;

        for (int i = 0; i < _nodeMap.Count; i++)
        {
            var nodes = _nodeMap[i];

            for (int j = 0; j < nodes.Count; j++)
            {
                Vector2Int index = new(i, j);
                var node = nodes[j];

                node.IsNodeCompleted = completedNodes.Contains(index);
                if (curNodeIndex == index)
                {
                    curNode = node;
                    node.IsNodeAccessible = !node.IsNodeCompleted;
                }
                else
                    node.IsNodeAccessible = false;
            }
        }
        if (curNode == null) return;
        //if (curNode.GetNodeIndex == Vector2Int.zero)
            //CompleteCurrentNode();

        // set next nodes as accessible
        for (int i = 0; i < curNode.GetNextNodes.Length; i++)
            curNode.GetNextNodes[i].IsNodeAccessible = curNode.IsNodeCompleted;

        RefreshNodeVisuals?.Invoke();
    }

    //Called when shop or a bounty is finished
    public void CompleteCurrentNode()
    {
        var pdm = PlayerDataManager.Instance;

        if (pdm.GetCurrentNodeIndex == new Vector2Int(_nodeMapCreator.GetNumberOfTiers, 0))
        {
            NodeMapComplete?.Invoke();
            return;
        }

        var curNodeIndex = pdm.GetCurrentNodeIndex;
        _nodeMap[curNodeIndex.x][curNodeIndex.y].IsNodeCompleted = true;

        List<Vector2Int> tempCompleted = new();
        if (pdm.GetCompletedNodes != null)
            tempCompleted = pdm.GetCompletedNodes.ToList();
        tempCompleted.Add(pdm.GetCurrentNodeIndex);

        pdm.UpdateNodeData(tempCompleted.ToArray());
        RefreshNodeVisuals?.Invoke();

        _isNodeMapComplete = curNodeIndex.x == _nodeMapCreator.GetNumberOfTiers - 1;
    }

    public void ReturnToMap()
    {
        PlayerDataManager.Instance.GenerateGeneralSeed(); //generate new seed for next combat/shop scene
        SaveLoadScript.SaveGame?.Invoke();
        TransitionScene.Instance.StartTransition("NodeMap");
        RefreshNodeVisuals?.Invoke();
    }
}
