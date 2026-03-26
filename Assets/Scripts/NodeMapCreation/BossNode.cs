using System.Collections.Generic;
using UnityEngine;

public class BossNode : NodeMapNode, IUseCombatMapData
{
    [SerializeField] private CombatMapData _combatData;

    public override void InitNode(Vector2Int index, List<NodeMapNode> prev, List<NodeMapNode> next)
    {
        base.InitNode(index, prev, next);
    }

    public override void OnClick()
    {
        PlayerDataManager.Instance.SetCurrMapNodeData(_combatData);
        EnterNodeScene();
    }

    public void SetCombatData(CustomTileMapSO[] mapPool)
    {
        // make more unique boss logic here? (expand struct?)

        //filter map pool by type?
        Random.InitState(PlayerDataManager.Instance.GetGeneralSeed);
        int rngMap = Random.Range(0, mapPool.Length);
        var so = mapPool[rngMap];
        if (so == null)
        {
            Debug.LogError("tileMap SO Null");
            return;
        }

        _combatData = new CombatMapData() { maxEnemiesAllowed = 1, maxPlayersAllowed = 1, selectedMap = so };

        //set button image from resource load?
    }
}
