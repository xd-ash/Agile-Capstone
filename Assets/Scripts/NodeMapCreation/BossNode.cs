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

        var rngBoss = UnitLibrary.GetRandomEnemies(1, adjustedSeed, false);

        _combatData = new CombatMapData() { maxEnemiesAllowed = 1, selectedEnemies = rngBoss, selectedMap = so };

        _combatData.enemyNames = OutlawNameGenerator.GenerateNames(1, adjustedSeed);
        PopulateCombatPoster(_combatData.selectedEnemies, "DEAD OR ALIVE", _combatData.enemyNames, true);
        SetNodeTypeBadge("TempNodeMap/Nodeicons/BossBadge", new Color(0.8f, 0.15f, 0.15f)); // red
    }
}