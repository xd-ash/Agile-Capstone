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
        // Filter out tutorial maps from the normal combat pool
        var filtered = System.Array.FindAll(mapPool, m => m.GetCombatMapType != CombatMapType.Tutorial);
        if (filtered.Length == 0)
        {
            Debug.LogError("CombatNode: No non-tutorial maps available in pool.");
            return;
        }

        Random.InitState(PlayerDataManager.Instance.GetGeneralSeed);
        int rngMap = Random.Range(0, filtered.Length);
        var so = filtered[rngMap];
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
                so = library.GetTileMapSOsFromType(CombatMapType.Tutorial)[0];
            }

            _combatData = new CombatMapData { maxEnemiesAllowed = 1, maxPlayersAllowed = 1, selectedMap = so };
        }
        else
        {
            Random.InitState(PlayerDataManager.Instance.GetNodeMapSeed + (int)transform.localPosition.x + (int)transform.localPosition.y);
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
