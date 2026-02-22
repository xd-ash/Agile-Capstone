using System.Collections.Generic;
using UnityEngine;

public class BossNode : NodeMapNode
{
    [SerializeField] private CombatMapData _combatData;

    public override void InitNode(Vector2Int index, List<NodeMapNode> prev, List<NodeMapNode> next)
    {
        base.InitNode(index, prev, next);

        // make more unique boss logic here? (expand struct?)
        _combatData = new CombatMapData() { maxEnemiesAllowed = 1, maxPlayersAllowed = 1 };
        //

        //set button image from resource load?
    }

    public override void OnClick()
    {
        PlayerDataManager.Instance.SetCurrMapNodeData(_combatData);
        EnterNodeScene();
    }
}
