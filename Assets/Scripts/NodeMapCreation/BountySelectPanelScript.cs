using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BountySelectPanelScript : MonoBehaviour
{
    private RectTransform _panelTransform;

    [SerializeField] private List<CombatMapData> _bountyOptions = new();
    private Button[] _bountyButtons;

    public void InitBountyBoard(CombatMapData[] bountyOptions, Vector2Int nodeIndex)
    {
        _bountyOptions = new(bountyOptions);
        _bountyButtons = new Button[bountyOptions.Length];
        SpawnBountyButtons();

        PlayerDataManager.Instance.UpdateNodeData(nodeIndex);
    }

    private void SpawnBountyButtons()
    {
        if (_bountyButtons == null || _bountyButtons.Length == 0)
        {
            Debug.LogError($"Bounty button init failure due to button array length/null.");
            return;
        }

        //buttons positions controlled by horizontal layout group
        for (int i = 0; i < _bountyOptions.Count; i++)
        {
            GameObject buttonGO = Instantiate(Resources.Load<GameObject>("TempNodeMap/BountySelectButton"), transform);
            buttonGO.name = $"BountyOption ({i})";
            var option = _bountyOptions[i];

            var image = buttonGO.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>($"TempNodeMap/Nodeicons/Bounty{option.maxEnemiesAllowed}");

            var button = buttonGO.GetComponent<Button>();
            button?.onClick.AddListener(() =>
            {
                PlayerDataManager.Instance.SetCurrMapNodeData(option);
                TransitionScene.Instance.StartTransition("Combat");//make better/dynamic?
            });
        }
    }
}
