using System.Collections.Generic;
using UnityEngine;

public class EliteNode : NodeMapNode, IUseCombatMapData
{
    [SerializeField] private CombatMapData _combatData;

    public override void InitNode(Vector2Int index, List<NodeMapNode> prev, List<NodeMapNode> next)
    {
        base.InitNode(index, prev, next);
    }

    public override void OnClick()
    {
        base.OnClick();

        PlayerDataManager.Instance.SetCurrMapNodeData(_combatData);
        EnterNodeScene();
    }

    public void SetCombatData(CustomTileMapSO[] mapPool)
    {
        var adjustedSeed = PlayerDataManager.Instance.GetGeneralSeed - int.Parse($"{_nodeIndex.x}{_nodeIndex.y}");// adding variation in seed based on node position
        Random.InitState(adjustedSeed);

        int rngMap = Random.Range(0, mapPool.Length);
        var so = mapPool[rngMap];
        if (so == null)
        {
            Debug.LogError("tileMap SO Null");
            return;
        }

        var rngEnemies = UnitLibrary.GetRandomEnemies(3, adjustedSeed, true);

        _combatData = new CombatMapData() { maxEnemiesAllowed = 3, selectedEnemies = rngEnemies, selectedMap = so };
    }
}