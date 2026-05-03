using CardSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DeckEditingController))]
public class DeckViewerScript : MonoBehaviour
{
    //private DeckEditingController _deckEditingController;

    [Header("Card Spawning")]
    [SerializeField] private GameObject _cardContentPrefab;
    [SerializeField] private ScrollRect _deckScrollView;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _chipsBalanceText;
    [SerializeField] private TextMeshProUGUI _allowedEditsText;

    [Header("Other UI Elements")]
    [SerializeField] private GameObject _backButton;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private GameObject _chipsBalanceGO;
    [SerializeField] private GameObject _allowedEditsGO;

    public CardState ViewerState { get; private set; } = CardState.DeckViewer;

    public static DeckViewerScript Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        if (_backButton.TryGetComponent(out Button bb))
            bb.onClick.AddListener(() => CardShopManager.Instance?.ToggleShopCardBC(true));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && ViewerState == CardState.DeckViewer)
            gameObject.SetActive(false);
    }

    public void InitDeckViewer(Action<int> onComplete = null, CardState cardState = CardState.DeckViewer)
    {
        ViewerState = cardState;

        ToggleEditUIElements(false);
        bool isCardRemoveReward = NodeMapManager.Instance == null || !NodeMapManager.Instance.gameObject.activeInHierarchy;
        ToggleBackButton(cardState == CardState.DeckViewer || cardState == CardState.CardSwap || isCardRemoveReward);

        if (cardState == CardState.UpgradeMenu || cardState == CardState.FreeUpgradeMenu || 
            cardState == CardState.CardRemoval || cardState == CardState.FreeCardRemoval || cardState == CardState.CardSwap)
            DeckEditingController.Instance.InitEditingController(onComplete);

        BuildDeckScrollViewContent();
    }

    //Create all card content in the card library scrollview
    public void BuildDeckScrollViewContent()
    {
        if (PlayerDataManager.Instance == null || _cardContentPrefab == null || _deckScrollView == null) return;
        if (PlayerDataManager.Instance.GetPlayerDeck == null || PlayerDataManager.Instance.GetPlayerDeck.GetCardsInDeck == null)
        {
            Debug.Log("Playerdata deck error");
            return;
        }

        var deck = PlayerDataManager.Instance.GetPlayerDeck;
        if (deck == null) return;
        if (DeckEditingController.Instance == null) return;

        SetWindowVisualsUp(ViewerState);

        Action<Transform, Card> cardSelectAction = null;
        if (ViewerState == CardState.UpgradeMenu || ViewerState == CardState.CardRemoval || 
            ViewerState == CardState.FreeUpgradeMenu || ViewerState == CardState.FreeCardRemoval || ViewerState == CardState.CardSwap)
            cardSelectAction = (t, c) =>
            {
                DeckEditingController.Instance.ShowPreview(c);
            };
        
        CardScrollviewFiller.BuildScrollViewContent(_deckScrollView.content, _cardContentPrefab, deck.GetCardsInDeck.ToArray(), ViewerState, null, cardSelectAction);
    }
    private void SetWindowVisualsUp(CardState cardState)
    {
        string titleText = "Cards in Deck";

        switch (cardState)
        {
            case CardState.DeckViewer:
                break;
            case CardState.UpgradeMenu:
            case CardState.FreeUpgradeMenu:
                titleText = "Select Card to Upgrade";
                ToggleEditUIElements(true);
                break;
            case CardState.CardRemoval:
            case CardState.FreeCardRemoval:
                titleText = "Select Card to Remove";
                ToggleEditUIElements(true);
                break;
            case CardState.CardSwap:
                titleText = "Select Card to Swap Out";
                break;
        }

        _titleText.text = titleText;
    }
    public void UpdateChipsBalance()
    {
        if (_chipsBalanceText == null) return;
        _chipsBalanceText.text = $"Chips: {PlayerDataManager.Instance.GetBalance}";
    }
    public void UpdateEditAmountRemaining(int numRemaining)
    {
        if (_allowedEditsText == null) return;
        _allowedEditsText.text = ViewerState == CardState.UpgradeMenu || ViewerState == CardState.FreeUpgradeMenu ? $"Remaining Upgrades: {numRemaining}" : $"Remaining Removals: {numRemaining}";
    }
    public void ToggleBackButton(bool isBackButtonActive)
    {
        _backButton?.SetActive(isBackButtonActive);
        _continueButton?.SetActive(!isBackButtonActive);
    }
    public void ToggleEditUIElements(bool isEditing)
    {
        _allowedEditsGO?.SetActive(isEditing && CardShopManager.Instance == null);
        _chipsBalanceGO?.SetActive(isEditing);
    }
}
