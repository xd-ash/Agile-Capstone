using System.Collections.Generic;
using UnityEngine;

public class CombatNode : NodeMapNode
{
    [SerializeField] private CombatMapData _combatData;

    public override void InitNode(Vector2Int index, List<NodeMapNode> prev, List<NodeMapNode> next)
    {
        base.InitNode(index, prev, next);

        if (_nodeIndex == Vector2Int.zero)
            _combatData = new CombatMapData { maxEnemiesAllowed = 1, maxPlayersAllowed = 1 };
        else
        {
            Random.InitState(PlayerDataManager.Instance.GetNodeMapSeed + (int)transform.localPosition.x + (int)transform.localPosition.y); // adding variation in seed based on node position
            _combatData = new CombatMapData() { maxEnemiesAllowed = Random.Range(1, 4), maxPlayersAllowed = 1 };
        }

        _background.sprite = Resources.Load<Sprite>($"TempNodeMap/Nodeicons/Bounty{_combatData.maxEnemiesAllowed}");
    }
    
    public override void OnClick()
    {
        PlayerDataManager.Instance.SetCurrMapNodeData(_combatData);
        EnterNodeScene();
    }
}

//struct to store data on how many enemies/players to spawn based on which node is selected
[System.Serializable]
public struct CombatMapData
{
    public int maxPlayersAllowed;
    public int maxEnemiesAllowed;
}