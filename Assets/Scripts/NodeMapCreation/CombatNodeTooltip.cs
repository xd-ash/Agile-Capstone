using System;
using TMPro;
using UnityEngine;

public class CombatNodeTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tooltipText;
    [SerializeField] private RectTransform _uiParent;
    [SerializeField] private float _yOffset = 70f;

    public static Action<NodeMapNode> OnShowTooltip;
    public static Action OnHideTooltip;

    private void Awake()
    {
        OnShowTooltip += ShowTooltip;
        OnHideTooltip += HideTooltip;
        HideTooltip();
    }

    private void OnDestroy()
    {
        OnShowTooltip -= ShowTooltip;
        OnHideTooltip -= HideTooltip;
    }

    private void HideTooltip()
    {
        gameObject.SetActive(false);
    }

    private void ShowTooltip(NodeMapNode node)
    {
        var names = node.GetEnemyDisplayNames;
        var enemies = node.GetEnemyDisplaySOs;
        if (names == null || enemies == null || names.Length == 0) return;

        transform.localPosition = node.transform.localPosition;

        float offset = node.transform.localPosition.y > 0 ? _yOffset : -_yOffset;
        _uiParent.localPosition = new Vector3(0f, offset, 0f);

        bool isBoss = node.GetIsBossNode;
        string tooltip = "";
        for (int i = 0; i < names.Length && i < enemies.Length; i++)
        {
            string typeName = OutlawNameGenerator.GetUnitTypeDisplayName(enemies[i].GetUnitType, isBoss);
            tooltip += $"{names[i]} - ({typeName})";
            if (i < names.Length - 1) tooltip += "\n";
        }

        _tooltipText.text = tooltip;
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }
}