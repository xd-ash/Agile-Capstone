using System.Collections.Generic;
using UnityEngine;

public class CampNode : NodeMapNode
{
    [SerializeField] private CampNodeController _campNodeController;
    [SerializeField] private CardUpgradeController _cardUpgradeController;

    public override void InitNode(Vector2Int index, List<NodeMapNode> prev, List<NodeMapNode> next)
    {
        base.InitNode(index, prev, next);

        _campNodeController = FindFirstObjectByType<CampNodeController>(FindObjectsInactive.Include);
    }
    public override void OnClick()
    {
        _campNodeController.gameObject.SetActive(true);
        _campNodeController.InitCampNode(() => CompleteCampNode());
    }
    private void CompleteCampNode()
    {
        PlayerDataManager.Instance.UpdateNodeData(_nodeIndex);
        NodeMapManager.Instance.CompleteCurrentNode();
        NodeMapManager.Instance.InitNodes();
    }
}