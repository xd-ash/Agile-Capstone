using System.Collections.Generic;
using UnityEngine;

public class CombatNode : NodeMapNode, IUseCombatMapData
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

        if (_nodeIndex.x == 0)
        {
            var rngEnemies = UnitLibrary.GetRandomEnemies(1, adjustedSeed, true, UnitType.MedicEnemy);

            _combatData = new CombatMapData { maxEnemiesAllowed = 1, selectedEnemies = rngEnemies, selectedMap = so };
        }
        else
        {
            int maxEnemies = Random.Range(1, 4);
            var rngEnemies = UnitLibrary.GetRandomEnemies(maxEnemies, adjustedSeed, true, maxEnemies == 1 ? UnitType.MedicEnemy : UnitType.Player);

            _combatData = new CombatMapData() { maxEnemiesAllowed = maxEnemies, selectedEnemies = rngEnemies, selectedMap = so };
        }

        // [POSTER] generate names and populate poster visual
        _combatData.enemyNames = OutlawNameGenerator.GenerateNames(_combatData.selectedEnemies.Length, adjustedSeed);
        PopulateCombatPoster(_combatData.selectedEnemies, "WANTED", _combatData.enemyNames, false);
    }
}

//struct to store data on how many enemies to spawn based on which node is selected
[System.Serializable]
public struct CombatMapData
{
    //public int maxPlayersAllowed;
    public int maxEnemiesAllowed;
    public UnitSO[] selectedEnemies;
    public CustomTileMapSO selectedMap;
    public string[] enemyNames;
    public bool isElite;
}