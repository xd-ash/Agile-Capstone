using CardSystem;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameObjectPool;

public class RewardSelectScript : MonoBehaviour
{
    private RewardsDisplayScript _displayScript;
    private TextMeshProUGUI _titleText;

    [SerializeField] private Transform _optionsContentParent;
    [SerializeField] private Button _confirmButton, _skipButton;

    private GameObject _cardOptionContent, _badgeOptionContent;
    private GameObject[] _contentHighlights;

    private Action _onConfirm;

    public static bool IsRewarding { get; private set; }

    private void Awake()
    {
        _displayScript = GetComponentInParent<RewardsDisplayScript>();
        _titleText = GetComponentInChildren<TextMeshProUGUI>();

        _cardOptionContent = Resources.Load<GameObject>("NewCardPrefab");
        _badgeOptionContent = Resources.Load<GameObject>("Rewards/BadgeOptionContent");

        _confirmButton?.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(() =>
        {
            _onConfirm?.Invoke();
            gameObject.SetActive(false);
        });

        _skipButton?.onClick.RemoveAllListeners();
        _skipButton.onClick.AddListener(() =>
        {
            _displayScript.OnSkipRewardChoice();
            gameObject.SetActive(false);
        });

        IsRewarding = true;
    }
    private void OnEnable()
    {
        _confirmButton.interactable = false;
        _skipButton.interactable = true;
    }

    public void ShowRewardOptions(Card[] cardOptions, RewardType rewardType)
    {
        ClearContent();

        List<GameObject> contentHighlights = new();

        foreach (var card in cardOptions)
        {
            if (card == null || card.GetCardAbility == null) continue;

            GameObject content = Spawn(_cardOptionContent, _optionsContentParent);

            Image optionHighlight = content.GetComponentInChildren<Image>(true);
            optionHighlight.gameObject.SetActive(false);
            contentHighlights.Add(optionHighlight.gameObject);

            var tmpCard = new Card(card, content.transform);

            CardPrefabSetterUpper.SetupCardPrefab(tmpCard, CardState.Rewards, OnSelectCard(rewardType, card, optionHighlight));
        }

        _contentHighlights = contentHighlights.ToArray();
    }
    private Action OnSelectCard(RewardType rewardType, Card card, Image optionHighlight)
    {
        switch (rewardType)
        {
            case RewardType.NewCard:
                return () =>
                {
                    _confirmButton.interactable = true;
                    ClearHighlights(optionHighlight?.transform);
                    optionHighlight?.gameObject.SetActive(true);

                    _onConfirm = null;
                    _onConfirm = () =>
                    {
                        RewardsController.RewardCard(card);
                        _displayScript.OnConfirmRewardChoice(card);
                    };
                };
            case RewardType.SwapCard:
                return () =>
                {
                    _confirmButton.interactable = true;
                    ClearHighlights(optionHighlight?.transform);
                    optionHighlight?.gameObject.SetActive(true);

                    var deckViewer = FindFirstObjectByType<DeckViewerScript>(FindObjectsInactive.Include);
                    var deckEditingController = FindFirstObjectByType<DeckEditingController>(FindObjectsInactive.Include);

                    _onConfirm = null;
                    _onConfirm = () =>
                    {
                        deckViewer?.gameObject?.SetActive(true);
                        deckViewer?.InitDeckViewer((x) =>
                        {
                            RewardsController.RewardCard(card);
                            _displayScript.OnConfirmRewardChoice(card);
                        }, CardState.CardSwap);
                        deckEditingController?.SetCardSwap(card);
                    };
                };
            default:
                return null;
        }
    }
    private void ClearHighlights(Transform thisCard)
    {
        if (_contentHighlights == null || thisCard == null) return;
        foreach (var highlight in _contentHighlights)
        {
            if (thisCard.gameObject == highlight) continue;
            highlight.SetActive(false);
            var cfs = highlight.GetComponentInParent<CardFunctionScript>();
            var cs = highlight.GetComponentInParent<CardSelect>();
            cs?.ToggleHighlightAndScale(false);
            cfs?.ClearSelection(0f);
        }
    }
    private void ClearContent()
    {
        for (int i = _optionsContentParent.childCount - 1; i >= 0; i--)
            Remove(_optionsContentParent.GetChild(i).gameObject);

        _contentHighlights = new GameObject[0];
    }
}
