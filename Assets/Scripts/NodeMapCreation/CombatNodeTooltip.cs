using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CombatNodeTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _name1;
    [SerializeField] private TextMeshProUGUI _tooltip1;

    [SerializeField] private TextMeshProUGUI _name2;
    [SerializeField] private TextMeshProUGUI _tooltip2;

    [SerializeField] private TextMeshProUGUI _name3;
    [SerializeField] private TextMeshProUGUI _tooltip3;

    //[SerializeField] private RectTransform _uiParent;
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

        //transform.localPosition = node.transform.localPosition;

        float offset = node.transform.localPosition.y > 0 ? _yOffset : -_yOffset;
        //_uiParent.localPosition = new Vector3(0f, offset, 0f);

        bool isBoss = node.GetIsBossNode;

        _name1.text = string.Empty;
        _tooltip1.text = string.Empty;
        _name2.text = string.Empty;
        _tooltip2.text = string.Empty;
        _name3.text = string.Empty;
        _tooltip3.text = string.Empty;

        //string tooltip = "";
        for (int i = 0; i < names.Length && i < enemies.Length; i++)
        {
            string typeName = OutlawNameGenerator.GetUnitTypeDisplayName(enemies[i].GetUnitType, isBoss);

            switch (i)
            {
                case 0:
                    _name1.text = names[i];
                    _tooltip1.text = typeName;
                    break;
                case 1:
                    _name2.text = names[i];
                    _tooltip2.text = typeName;
                    break;
                case 2:
                    _name3.text = names[i];
                    _tooltip3.text = typeName;
                    break;
            }
            /*tooltip += $"{names[i]} - ({typeName})";
            if (i < names.Length - 1) tooltip += "\n";*/
        }

        //_tooltipText.text = tooltip;
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }
}