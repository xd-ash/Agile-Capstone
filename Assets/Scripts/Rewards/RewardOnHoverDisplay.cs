using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RewardOnHoverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _chipsIcon, _cardAddIcon, _cardSwapIcon;
    //[SerializeField] Transform _topPos, _bottomPos, _uiParent;

    public static Action<NodeMapNode> OnRewardNodeHover;
    public static Action OnClearRewardDisplay;

    private void Awake()
    {
        OnRewardNodeHover += ShowRewardDisplay;
        OnClearRewardDisplay += HideRewardHoverDisplay;

        //_uiParent.position = _bottomPos.position;
        HideRewardHoverDisplay();
    }
    private void OnDestroy()
    {
        OnRewardNodeHover -= ShowRewardDisplay;
        OnClearRewardDisplay -= HideRewardHoverDisplay;
    }
    private void HideRewardHoverDisplay()
    {
        _chipsIcon?.SetActive(false);
        _cardAddIcon?.SetActive(false);
        _cardSwapIcon?.SetActive(false);

        gameObject.SetActive(false);
    }
    private void ShowRewardDisplay(NodeMapNode rewardNode)
    {        
        //transform.parent.SetAsLastSibling();
        transform.localPosition = rewardNode.transform.localPosition;
        if (transform.localPosition.y < -230)
            transform.localPosition = new(transform.localPosition.x, -230);
        else if (transform.localPosition.y > 267)
            transform.localPosition = new(transform.localPosition.x, 267);

        //_uiParent.position = transform.localPosition.y > 0 ? _bottomPos.position : _topPos.position;

        _chipsIcon?.SetActive(true);

        switch (rewardNode.GetNodeRewards.GetRewardType)
        {
            case RewardType.NewCard:
                _cardAddIcon?.SetActive(true);
                break;
            case RewardType.SwapCard:
                _cardSwapIcon?.SetActive(true);
                break;
            case RewardType.All:
                _cardAddIcon?.SetActive(true);
                _cardSwapIcon?.SetActive(true);
                break;
        }

        gameObject.SetActive(true);
    }
}
