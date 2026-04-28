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

    [SerializeField] private Sprite _currencyImage, _cardImage;
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
        _continueButton?.onClick.RemoveAllListeners();
        _continueButton?.onClick.AddListener(() =>
        {
            Action temp = () =>
            {
                RewardsController.RewardChips(_curReward.GetCurrencyReward);
                ClearContent();
                OnContinueClick();
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
            CreateChoiceRewardContent(_cardImage, () => _rewardSelectPanel.ShowRewardOptions(cardPool, _curReward.GetRewardType));
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
    private GameObject CreateChoiceRewardContent(Sprite sprite, Action onClick)
    {
        string name = _curReward.GetRewardType == RewardType.NewCard ? "New Card" : "Swap Card";

        GameObject content = Spawn(_choiceRewardPrefab, _rewardsContentParent.transform);
        content.name = name;

        var image = content.GetComponentInChildren<Image>();
        image.sprite = sprite;

        var buttons = content.GetComponentsInChildren<Button>();
        buttons[0].onClick.RemoveAllListeners();
        buttons[0].onClick.AddListener(() =>
        {
            _rewardSelectPanel.gameObject.SetActive(true);
            _pendingChoiceContent = content;
            onClick?.Invoke();
        });
        var text = buttons[0].GetComponentInChildren<TextMeshProUGUI>();
        text.text = name;

        buttons[1].onClick.RemoveAllListeners();
        buttons[1].onClick.AddListener(() =>
        {
            _pendingChoiceContent = content;
            var deckViewer = FindFirstObjectByType<DeckViewerScript>(FindObjectsInactive.Include);
            deckViewer?.gameObject.SetActive(true);
            deckViewer.InitDeckViewer((x) =>
            {
                OnConfirmRewardChoice();
            }, CardState.CardRemoval);
        });

        _pendingChoices++;
        return content;
    }
    public void OnConfirmRewardChoice(Card chosenCard = null)
    {
        GameObject newCardContent = null;
        if (chosenCard != null)
            newCardContent = CreateSingleRewardContent(_cardImage, chosenCard.GetCardName, -1);

        for (int i = _rewardsContentParent.transform.childCount - 1; i >= 0; i--)
        {
            if (_rewardsContentParent.transform.GetChild(i).gameObject == _pendingChoiceContent)
            {
                newCardContent?.transform.SetSiblingIndex(i);
                Remove(_pendingChoiceContent);
                break;
            }
        }

        _pendingChoices--;
        _pendingChoiceContent = null;
    }
    public void OnSkipRewardChoice()
    {
        _pendingChoiceContent = null;
    }
    private void ClearContent()
    {
        for (int i = _rewardsContentParent.transform.childCount - 1; i >= 0; i--)
        {
            var obj = _rewardsContentParent.transform.GetChild(i).gameObject;
            if (obj == null || !obj.activeInHierarchy) continue;
            Remove(obj);
        }

        _pendingChoices = 0;
    }
    private void OnContinueClick()
    {
        NodeMapManager.Instance.CompleteCurrentNode();
        WinLossManager.CombatNodeCompleted?.Invoke();

        IsRewarding = false;

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
