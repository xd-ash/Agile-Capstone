using System.Collections.Generic;
using UnityEngine;

public class CombatNode : NodeMapNode, IUseCombatMapData
{
    [SerializeField] private CombatMapData _combatData;

    private static Dictionary<int, Sprite> _combatSpriteCache = new Dictionary<int, Sprite>();

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

        if (_nodeIndex == Vector2Int.zero)
        {
            if (OptionsSettings.ShouldRunTutorial)
            {
                var library = Resources.Load<CustomTileMapSOLibrary>("Libraries/CustomTileMapSOLibrary");
                so = library.GetTileMapSOsFromType(CombatMapType.Tutorial)[0];
            }

            _combatData = new CombatMapData
            {
                maxEnemiesAllowed = 1,
                maxPlayersAllowed = 1,
                selectedMap = so
            };
        }
        else
        {
            Random.InitState(PlayerDataManager.Instance.GetNodeMapSeed + (int)transform.localPosition.x + (int)transform.localPosition.y);

            _combatData = new CombatMapData
            {
                maxEnemiesAllowed = Random.Range(1, 4),
                maxPlayersAllowed = 1,
                selectedMap = so
            };
        }

        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        int enemyCount = _combatData.maxEnemiesAllowed;

        if (!_combatSpriteCache.TryGetValue(enemyCount, out Sprite sprite))
        {
            sprite = Resources.Load<Sprite>($"TempNodeMap/Nodeicons/Bounty{enemyCount}");
            _combatSpriteCache[enemyCount] = sprite;
        }

        if (_background != null)
            _background.sprite = sprite;
    }
}

[System.Serializable]
public struct CombatMapData
{
    public int maxPlayersAllowed;
    public int maxEnemiesAllowed;
    public CustomTileMapSO selectedMap;
}