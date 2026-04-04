using CardSystem;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameObjectPool;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class RewardSelectScript : MonoBehaviour
{
    private RewardsDisplayScript _displayScript;

    [SerializeField] private Transform _optionsContentParent;
    [SerializeField] private Button _confirmButton, _skipButton;

    private GameObject _cardOptionContent, _badgeOptionContent;
    private GameObject[] _contentHighlights;

    private Action _onConfirm;

    public static bool IsRewarding { get; private set; }

    private void Awake()
    {
        _displayScript = GetComponentInParent<RewardsDisplayScript>();

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

    public void ShowRewardOptions(Card[] cardOptions)
    {
        ClearContent();

        List<GameObject> contentHighlights = new();

        foreach (var card in cardOptions)
        {
            if (card == null || card.GetCardAbility == null) continue;

            GameObject content = Spawn(_cardOptionContent, _optionsContentParent);

            var tmpCard = new Card(card, content.transform);

            CardPrefabSetterUpper.SetupCardPrefab(tmpCard, CardState.Rewards);

            Image optionHighlight = content.GetComponentInChildren<Image>(true);
            optionHighlight.gameObject.SetActive(false);
            contentHighlights.Add(optionHighlight.gameObject);

            var cs = content.GetComponent<CardSelect>();
            cs.InitCardSelect(CardState.Rewards);
            cs.OnPrefabCreation(tmpCard);
            var cfs = content.GetComponent<CardFunctionScript>();
            cfs.SetOnMouseDown(CardState.Rewards, () =>
            {
                _confirmButton.interactable = true;
                ClearHighlights(optionHighlight.transform);
                optionHighlight.gameObject.SetActive(true);

                _onConfirm = null;
                _onConfirm = () =>
                {
                    RewardsController.RewardCard(card);
                    _displayScript.OnConfirmRewardChoice(card);
                };
            });

            /*Button contentButton = content.GetComponentInChildren<Button>(true);
            contentButton.onClick.RemoveAllListeners();
            contentButton.onClick.AddListener(() =>
            {
                _confirmButton.interactable = true;
                ClearHighlights();
                optionHighlight.gameObject.SetActive(true);

                _onConfirm = null;
                _onConfirm = () =>
                {
                    RewardsController.RewardCard(card);
                    _displayScript.OnConfirmRewardChoice(card);
                };
            });*/
        }

        _contentHighlights = contentHighlights.ToArray();
    }
    public void ShowRewardOptions(BadgeSO[] badgeOptions)
    {
        ClearContent();

        List<GameObject> contentHighlights = new();

        foreach (var badge in badgeOptions)
        {
            if (badge == null) continue;

            GameObject content = Spawn(_badgeOptionContent, _optionsContentParent);

            TextMeshProUGUI[] badgeTextFields = content.GetComponentsInChildren<TextMeshProUGUI>();
            // Update text content
            badgeTextFields[0].text = badge.name;
            badgeTextFields[1].text = badge.GetDescription;

            Image optionHighlight = content.GetComponentInChildren<Image>(true);
            optionHighlight.gameObject.SetActive(false);
            contentHighlights.Add(optionHighlight.gameObject);

            Button contentButton = content.GetComponentInChildren<Button>(true);
            contentButton.onClick.RemoveAllListeners();
            contentButton.onClick.AddListener(() =>
            {
                _confirmButton.interactable = true;
                ClearHighlights(optionHighlight.transform);
                optionHighlight.gameObject.SetActive(true);

                _onConfirm = null;
                _onConfirm = () => 
                {
                    RewardsController.RewardBadge(badge);
                    Debug.LogError("Badge rewarding not fully implemented yet"); 
                };
            });
        }

        _contentHighlights = contentHighlights.ToArray();
    }
    private void ClearHighlights(Transform thisCard)
    {
        if (_contentHighlights == null) return;
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
