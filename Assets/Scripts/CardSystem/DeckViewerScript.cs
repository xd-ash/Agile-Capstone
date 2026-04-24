using CardSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckViewerScript : MonoBehaviour
{
    [SerializeField] private GameObject _backButton;
    [SerializeField] private Button _continueButton;

    [SerializeField] private GameObject _cardContentPrefab;
    [SerializeField] private ScrollRect _deckScrollView;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _chipsBalanceText;
    [SerializeField] private TextMeshProUGUI _allowedUpgradeCount;

    private CardState _state;

    public static DeckViewerScript Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        ToggleBackButton(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _state == CardState.DeckViewer)
            gameObject.SetActive(false);
    }

    //Create all card content in the card library scrollview
    public void BuildDeckScrollViewContent(CardState cardState = CardState.DeckViewer)
    {
        if (PlayerDataManager.Instance == null || _cardContentPrefab == null || _deckScrollView == null) return;
        if (PlayerDataManager.Instance.GetPlayerDeck == null || PlayerDataManager.Instance.GetPlayerDeck.GetCardsInDeck == null)
        {
            Debug.Log("Playerdata deck error");
            return;
        }
        _state = cardState;

        var deck = PlayerDataManager.Instance.GetPlayerDeck;
        if (deck == null) return;
        if (CardUpgradeController.Instance == null) return;

        SetWindowVisualsUp(cardState);

        Action<Transform, Card> cardSelectAction = null;
        if (cardState == CardState.UpgradeMenu)
            cardSelectAction = (t, c) =>
            {
                CardUpgradeController.Instance.ShowUpgradePreview(c);
            };
        else if (cardState == CardState.DeckEdit)
        {

        }
        
        CardScrollviewFiller.BuildScrollViewContent(_deckScrollView.content, _cardContentPrefab, deck.GetCardsInDeck.ToArray(), cardState, null, cardSelectAction);
    }
    private void SetWindowVisualsUp(CardState cardState)
    {
        string titleText = "Cards in Deck";

        switch (cardState)
        {
            case CardState.DeckViewer:
                _chipsBalanceText?.gameObject?.SetActive(false);
                break;
            case CardState.UpgradeMenu:
                titleText = "Select Card to Upgrade";
                _allowedUpgradeCount?.gameObject?.SetActive(true);
                break;
            case CardState.DeckEdit:
                titleText = "Select Card to Remove";
                break;
        }

        _titleText.text = titleText;
    }
    public void UpdateChipsBalance()
    {
        if (_chipsBalanceText == null) return;
        _chipsBalanceText.text = $"Chips: {PlayerDataManager.Instance.GetBalance}";
    }
    public void UpdateUpgradeAmountRemaining(int numRemaining)
    {
        if (_allowedUpgradeCount == null) return;
        _allowedUpgradeCount.text = $"{numRemaining}";
    }
    public void ToggleBackButton(bool isBackButtonActive)
    {
        _backButton?.SetActive(isBackButtonActive);
        _continueButton?.gameObject?.SetActive(!isBackButtonActive);
    }
}
