using CardSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameObjectPool;

public class RewardsDisplayScript : MonoBehaviour
{
    private RewardSelectScript _rewardSelectPanel;
    private PendingRewardsPopup _pendingRewardsPopup;

    private Reward _curReward;

    [SerializeField] private Sprite _currencyImage, _cardImage, _badgeImage;
    [SerializeField] private GameObject _rewardsContentParent;

    private GameObject _singleRewardPrefab, _choiceRewardPrefab;
    private Button _continueButton;

    private int _pendingChoices;
    private GameObject _pendingChoiceContent;

    public static bool IsRewarding = false;

    private void Awake()
    {
        _rewardSelectPanel = FindAnyObjectByType<RewardSelectScript>(FindObjectsInactive.Include);
        _pendingRewardsPopup = FindAnyObjectByType<PendingRewardsPopup>(FindObjectsInactive.Include);

        _singleRewardPrefab = Resources.Load<GameObject>("Rewards/SingleRewardContent");
        _choiceRewardPrefab = Resources.Load<GameObject>("Rewards/ChoiceRewardContent");

        _continueButton = GetComponentInChildren<Button>();
        _continueButton?.onClick.AddListener(() =>
        {
            Action temp = () =>
            {
                RewardsController.RewardChips(_curReward.GetCurrencyReward);
                OnContinueClick();
                ClearContent();
                gameObject.SetActive(false);
            };

            if (_pendingChoices > 0)
            {
                _pendingRewardsPopup.gameObject.SetActive(true);
                _pendingRewardsPopup.SetContinueButtonOnClick(temp);
            }
            else
                temp?.Invoke();
        });
    }
    private void OnEnable()
    {
        IsRewarding = true;

        _curReward = PlayerDataManager.Instance.GetCurrNodeReward;

        _rewardSelectPanel.gameObject.SetActive(false);
        _pendingRewardsPopup.gameObject.SetActive(false);

        ShowRewards();
    }
    private void OnDisable()
    {
        IsRewarding = false;
    }
    public void ShowRewards()
    {
        ClearContent();

        int currency = _curReward.GetCurrencyReward;
        if (currency > 0)
            CreateSingleRewardContent(_currencyImage, "Chips", currency);

        var cardPool = _curReward.GetCardReward;
        if (cardPool != null && cardPool.Length > 0)
            CreateChoiceRewardContent(_cardImage, "Card Choices", () => _rewardSelectPanel.ShowRewardOptions(cardPool));

        var badgePool = _curReward.GetBadgeReward;
        if (badgePool != null && badgePool.Length > 0)
            CreateChoiceRewardContent(_badgeImage, "Badge Choices", () => _rewardSelectPanel.ShowRewardOptions(badgePool));
    }
    private GameObject CreateSingleRewardContent(Sprite sprite, string name, int amount)
    {
        GameObject content = Spawn(_singleRewardPrefab, _rewardsContentParent.transform);
        content.name = name;

        var image = content.GetComponentInChildren<Image>();
        image.sprite = sprite;

        var texts = content.GetComponentsInChildren<TextMeshProUGUI>();
        texts[0].text = name;
        texts[1].text = amount > 0 ? amount.ToString() : "";

        return content;
    }
    private GameObject CreateChoiceRewardContent(Sprite sprite, string name, Action onClick)
    {
        GameObject content = Spawn(_choiceRewardPrefab, _rewardsContentParent.transform);
        content.name = name;

        var image = content.GetComponentInChildren<Image>();
        image.sprite = sprite;

        var text = content.GetComponentInChildren<TextMeshProUGUI>();
        text.text = name;

        var button = content.GetComponentInChildren<Button>();
        button.onClick.AddListener(() =>
        {
            _rewardSelectPanel.gameObject.SetActive(true);
            _pendingChoiceContent = content;
            onClick?.Invoke();
        });

        _pendingChoices++;
        return content;
    }
    public void OnConfirmRewardChoice(CardAbilityDefinition chosenCard)
    {
        var newCardContent = CreateSingleRewardContent(_cardImage, chosenCard.GetCardName, -1);

        for (int i = _rewardsContentParent.transform.childCount - 1; i >= 0; i--)
        {
            if (_rewardsContentParent.transform.GetChild(i).gameObject == _pendingChoiceContent)
            {
                newCardContent.transform.SetSiblingIndex(i);
                Destroy(_pendingChoiceContent);
                break;
            }
        }

        _pendingChoices--;
        _pendingChoiceContent = null;
    }
    public void OnConfirmRewardChoice(BadgeSO chosenBadge)
    {
        var newBadgeContent = CreateSingleRewardContent(_badgeImage, chosenBadge.name, 1);

        for (int i = _rewardsContentParent.transform.childCount - 1; i >= 0; i--)
        {
            if (_rewardsContentParent.transform.GetChild(i).gameObject == _pendingChoiceContent)
            {
                newBadgeContent.transform.SetSiblingIndex(i);
                Remove(_pendingChoiceContent);
                break;
            }
        }

        _pendingChoiceContent = null;
    }
    public void OnSkipRewardChoice()
    {
        _pendingChoiceContent = null;
    }
    private void ClearContent()
    {
        for (int i = _rewardsContentParent.transform.childCount - 1; i >= 0; i--)
            Remove(_rewardsContentParent.transform.GetChild(i).gameObject);

        Debug.Log("test");

        _pendingChoices = 0;
    }
    private void OnContinueClick()
    {
        NodeMapManager.Instance.CompleteCurrentNode();
        WinLossManager.CombatNodeCompleted?.Invoke();

        if (!NodeMapManager.Instance.GetIsNodeMapComplete)
        {
            NodeMapManager.Instance.ReturnToMap();
            return;
        }

        WinLossManager.GameReset?.Invoke();
        SaveLoadScript.CreateNewGame?.Invoke();
        TransitionScene.Instance?.StartTransition();
    }
}
