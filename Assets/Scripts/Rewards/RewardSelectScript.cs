using CardSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameObjectPool;
using System.Collections.Generic;

public class RewardSelectScript : MonoBehaviour
{
    private RewardsDisplayScript _displayScript;

    [SerializeField] private Transform _optionsContentParent;
    [SerializeField] private Button _confirmButton, _skipButton;

    private GameObject _cardOptionContent, _badgeOptionContent;
    private GameObject[] _contentHighlights;

    private Action _onConfirm;

    private void Awake()
    {
        _displayScript = GetComponentInParent<RewardsDisplayScript>();

        _cardOptionContent = Resources.Load<GameObject>("Rewards/CardOptionContent");
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
    }
    private void OnEnable()
    {
        _confirmButton.interactable = false;
        _skipButton.interactable = true;
    }

    public void ShowRewardOptions(CardAbilityDefinition[] cardOptions)
    {
        ClearContent();

        List<GameObject> contentHighlights = new();

        foreach (var card in cardOptions)
        {
            if (card == null) continue;

            GameObject content = Spawn(_cardOptionContent, _optionsContentParent);

            TextMeshProUGUI[] cardTextFields = content.GetComponentsInChildren<TextMeshProUGUI>();
            // Update text content
            cardTextFields[0].text = card.GetCardName;
            cardTextFields[1].text = card.GetDescription;
            cardTextFields[2].text = card.GetApCost.ToString();

            Image optionHighlight = content.GetComponentInChildren<Image>(true);
            optionHighlight.gameObject.SetActive(false);
            contentHighlights.Add(optionHighlight.gameObject);

            Button contentButton = content.GetComponentInChildren<Button>(true);
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
            });
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
                ClearHighlights();
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
    private void ClearHighlights()
    {
        foreach (var highlight in _contentHighlights)
            highlight.SetActive(false);
    }
    private void ClearContent()
    {
        for (int i = _optionsContentParent.childCount - 1; i >= 0; i--)
            Remove(_optionsContentParent.GetChild(i).gameObject);

        _contentHighlights = new GameObject[0];
    }
}
