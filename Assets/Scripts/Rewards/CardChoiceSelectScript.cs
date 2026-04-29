using CardSystem;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardChoiceSelectScript : MonoBehaviour
{
    [SerializeField] private Transform _optionsContentParent;
    [SerializeField] private Button _confirmButton, _backButton;

    private GameObject _cardOptionContent;
    private GameObject[] _contentHighlights;

    private Action _onConfirm;

    private void Awake()
    {
        _cardOptionContent = Resources.Load<GameObject>("NewCardPrefab");

        _confirmButton?.onClick.RemoveAllListeners();
        _confirmButton?.onClick.AddListener(() =>
        {
            _onConfirm?.Invoke();
            gameObject.SetActive(false);
        });
    }

    private void InitRewardChoiceSelect()
    {
        _confirmButton.interactable = false;

        var displayScript = GetComponentInParent<RewardsDisplayScript>();
        if (displayScript == null) return;

        _backButton?.onClick.RemoveAllListeners();
        _backButton?.onClick.AddListener(() =>
        {
            displayScript?.OnSkipRewardChoice();
            gameObject.SetActive(false);
        });
    }
    private void InitUpgradeChoiceSelect()
    {
        var campNodeScript = FindAnyObjectByType<CampNodeController>(FindObjectsInactive.Include);
        if (campNodeScript == null) return;

        _backButton?.onClick.RemoveAllListeners();
        _backButton?.onClick.AddListener(() =>
        {
            campNodeScript.HideUpgradePreview();
        });
    }

    public void ShowOptions(Card[] cardOptions, RewardType rewardType)
    {
        InitRewardChoiceSelect();

        ClearContent();

        List<GameObject> contentHighlights = new();

        foreach (var card in cardOptions)
        {
            if (card == null || card.GetCardAbility == null) continue;

            GameObject content = Instantiate(_cardOptionContent, _optionsContentParent);

            Image optionHighlight = content.GetComponentInChildren<Image>(true);
            optionHighlight.gameObject.SetActive(false);
            contentHighlights.Add(optionHighlight.gameObject);

            var tmpCard = new Card(card, content.transform);

            CardPrefabSetterUpper.SetupCardPrefab(tmpCard, CardState.Rewards, OnSelectCard(rewardType, card, optionHighlight));
        }

        _contentHighlights = contentHighlights.ToArray();
    }
    public void ShowOptions(Card[] cardOptions)
    {
        InitUpgradeChoiceSelect();

        ClearContent();

        List<GameObject> contentHighlights = new();

        foreach (var card in cardOptions)
        {
            if (card == null || card.GetCardAbility == null) continue;

            GameObject content = Instantiate(_cardOptionContent, _optionsContentParent);

            Image optionHighlight = content.GetComponentInChildren<Image>(true);
            optionHighlight.gameObject.SetActive(false);
            contentHighlights.Add(optionHighlight.gameObject);

            var tmpCard = new Card(card, content.transform);

            CardPrefabSetterUpper.SetupCardPrefab(tmpCard, CardState.FreeUpgradeMenu, OnSelectCard(card, optionHighlight));
        }

        _contentHighlights = contentHighlights.ToArray();
    }
    private Action OnSelectCard(Card card, Image optionHighlight)
    {
        var campNodeScript = FindAnyObjectByType<CampNodeController>(FindObjectsInactive.Include);
        if (campNodeScript == null) return null;

        return () =>
        {
            ClearHighlights(optionHighlight?.transform);
            optionHighlight?.gameObject.SetActive(true);

            campNodeScript.ShowUpgradePreview(card);

            _onConfirm = null;
            _onConfirm = () =>
            {
                campNodeScript.OnConfirmUpgrade();
            };
        };
    }
    private Action OnSelectCard(RewardType rewardType, Card card, Image optionHighlight)
    {
        var displayScript = GetComponentInParent<RewardsDisplayScript>();
        if (displayScript == null) return null;

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
                        displayScript.OnConfirmRewardChoice(card);
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
                            displayScript.OnConfirmRewardChoice(card);
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
            Destroy(_optionsContentParent.GetChild(i).gameObject);

        _contentHighlights = new GameObject[0];
    }
}
