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
        Random.InitState(PlayerDataManager.Instance.GetGeneralSeed);

        int rngMap = Random.Range(0, mapPool.Length);
        var so = mapPool[rngMap];

        if (so == null)
        {
            Debug.LogError("tileMap SO Null");
            return;
        }

        int enemyCount;

        // tutorial override (same logic as before)
        if (_nodeIndex == Vector2Int.zero)
        {
            if (OptionsSettings.ShouldRunTutorial)
            {
                var library = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTileMapSOLibrary");
                so = library.GetTileMapSOsFromType(CombatMapType.Tutorial)[0];
            }

            enemyCount = 2; // still elite but controlled
        }
        else
        {
            Random.InitState(PlayerDataManager.Instance.GetNodeMapSeed +
                             (int)transform.localPosition.x +
                             (int)transform.localPosition.y);

            enemyCount = Random.Range(3, 4); 
        }

        _combatData = new CombatMapData
        {
            maxEnemiesAllowed = enemyCount,
            maxPlayersAllowed = 1,

            selectedMap = so
        };

        _background.sprite =
            Resources.Load<Sprite>($"TempNodeMap/Nodeicons/Elite{enemyCount}");
    }
}