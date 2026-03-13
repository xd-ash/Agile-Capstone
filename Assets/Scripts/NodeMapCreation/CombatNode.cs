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
        PlayerDataManager.Instance.SetCurrMapNodeData(_combatData);
        EnterNodeScene();
    }

    public void SetCombatData(CustomTileMapSO[] mapPool)
    {
        //filter map pool by type?
        Random.InitState(PlayerDataManager.Instance.GetGeneralSeed);
        int rngMap = Random.Range(0, mapPool.Length);
        var so = mapPool[rngMap];
        if (so == null)
        {
            Debug.LogError("tileMap SO Null");
            return;
        }

        if (_nodeIndex == Vector2Int.zero)
        {
            if (OptionsSettings.ShouldRunTutorial)
            {
                var library = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTileMapSOLibrary");
                so = library.GetTileMapSOsFromType(CombatMapType.Tutorial)[0]; //change to be random if multiple?
            }

            _combatData = new CombatMapData { maxEnemiesAllowed = 3, maxPlayersAllowed = 1, selectedMap = so };
        }
        else
        {
            Random.InitState(PlayerDataManager.Instance.GetNodeMapSeed + (int)transform.localPosition.x + (int)transform.localPosition.y); // adding variation in seed based on node position
            _combatData = new CombatMapData() { maxEnemiesAllowed = Random.Range(1, 4), maxPlayersAllowed = 1, selectedMap = so };
        }

        _background.sprite = Resources.Load<Sprite>($"TempNodeMap/Nodeicons/Bounty{_combatData.maxEnemiesAllowed}");
    }
}

//struct to store data on how many enemies/players to spawn based on which node is selected
[System.Serializable]
public struct CombatMapData
{
    public int maxPlayersAllowed;
    public int maxEnemiesAllowed;
    public CustomTileMapSO selectedMap;
}
