using System.Collections.Generic;
using UnityEngine;

public class BountyBoardNode : NodeMapNode, IUseCombatMapData
{
    [SerializeField] private BountySelectPanelScript _bountySelectPanel;
    [SerializeField] private List<CombatMapData> _combatData = new();

    public override void InitNode(Vector2Int index, List<NodeMapNode> prev, List<NodeMapNode> next)
    {
        base.InitNode(index, prev, next);

        _bountySelectPanel = FindFirstObjectByType<BountySelectPanelScript>(FindObjectsInactive.Include);
    }

    public override void OnClick()
    {
        _bountySelectPanel?.transform?.parent?.gameObject?.SetActive(true);
        _bountySelectPanel?.InitBountyBoard(_combatData.ToArray(), _nodeIndex);
    }

    public void SetCombatData(CustomTileMapSO[] mapPool)
    {
        var adjustedSeed = PlayerDataManager.Instance.GetGeneralSeed - int.Parse($"{_nodeIndex.x}{_nodeIndex.y}");// adding variation in seed based on node position
        Random.InitState(adjustedSeed);

        int numBounties = Random.Range(2, 4);
        _combatData.Clear();

        for (int i = 0; i < numBounties; i++)
        {
            Random.InitState(adjustedSeed - i);

            int rngMap = Random.Range(0, mapPool.Length);
            var so = mapPool[rngMap];
            if (so == null)
            {
                Debug.LogError("tileMap SO Null");
                return;
            }

            int maxEnemies = Random.Range(1, 4);
            var rngEnemies = UnitLibrary.GetRandomEnemies(maxEnemies, adjustedSeed, true, maxEnemies == 1 ? UnitType.MedicEnemy : UnitType.Player);

            _combatData.Add(new CombatMapData() { maxEnemiesAllowed = maxEnemies, selectedEnemies = rngEnemies, selectedMap = so });
        }
    }
}
