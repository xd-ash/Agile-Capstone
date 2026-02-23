using System.Collections.Generic;
using UnityEngine;

public class BountyBoardNode : NodeMapNode
{
    [SerializeField] private BountySelectPanelScript _bountySelectPanel;

    [SerializeField] private List<CombatMapData> _combatData = new();

    public override void InitNode(Vector2Int index, List<NodeMapNode> prev, List<NodeMapNode> next)
    {
        base.InitNode(index, prev, next);

        _bountySelectPanel = FindFirstObjectByType<BountySelectPanelScript>(FindObjectsInactive.Include);

        Random.InitState(PlayerDataManager.Instance.GetNodeMapSeed);
        int numBounties = Random.Range(2, 4);
        _combatData.Clear();
        for (int i = 0; i < numBounties; i++)
            _combatData.Add(new CombatMapData() { maxEnemiesAllowed = Random.Range(1, 4), maxPlayersAllowed = 1 });
    }

    public override void OnClick()
    {
        _bountySelectPanel?.gameObject.SetActive(true);
        _bountySelectPanel?.InitBountyBoard(_combatData.ToArray(), _nodeIndex);
    }
}
