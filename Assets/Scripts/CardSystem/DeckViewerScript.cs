using CardSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckViewerScript : MonoBehaviour
{
    private GameObject _backButton;
    [SerializeField] private GameObject _cardContentPrefab;
    [SerializeField] private ScrollRect _deckScrollView;

    private CardState _state;

    public static DeckViewerScript Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        _backButton = transform.Find("CloseWindowButton").gameObject;
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

        CardScrollviewFiller.BuildScrollViewContent(_deckScrollView.content, _cardContentPrefab, deck.GetCardsInDeck.ToArray(), cardState, null, cardSelectAction);
    }
    private void SetWindowVisualsUp(CardState cardState)
    {
        if (TryGetComponent(out TextMeshProUGUI titleText))
            titleText.text = cardState == CardState.DeckViewer ? "Cards in Deck" : "Select Card to Upgrade";

        GameObject chipsBalanceUI = transform.Find("CurrencyBalance").gameObject;
        GameObject upgradeCounterUI = transform.Find("AllowUpgradesCounter").gameObject;
        chipsBalanceUI?.SetActive(cardState == CardState.UpgradeMenu);
        upgradeCounterUI?.SetActive(cardState == CardState.UpgradeMenu);
    }
    public void ToggleBackButton(bool isActive)
    {
        _backButton?.SetActive(isActive);
    }
}
