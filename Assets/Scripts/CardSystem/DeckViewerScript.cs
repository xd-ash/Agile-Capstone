using CardSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class DeckViewerScript : MonoBehaviour
{
    private CardAndDeckLibrary _cardAndDeckLibrary;
    private Deck _currentShownDeck;
    [SerializeField] private TMP_Dropdown _deckDropdown;
    [SerializeField] private GameObject _cardContentPrefab;
    [SerializeField] private ScrollRect _deckScrollView;
    [SerializeField] private TextMeshProUGUI _deckTitleText;

    public static DeckViewerScript Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        _cardAndDeckLibrary = Resources.Load<CardAndDeckLibrary>("Libraries/CardAndDeckLibrary");

        GrabAllDeckOptions();
        SwapCurrentDeck(true);
        BuildDeckScrollViewContent();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            this.gameObject.SetActive(false);
    }
    //Create all card content in the card library scrollview
    private void BuildDeckScrollViewContent()
    {
        if (PlayerDataManager.Instance == null || _cardContentPrefab == null || _deckScrollView == null) return;
        if (PlayerDataManager.Instance.GetActiveDeck == null || PlayerDataManager.Instance.GetActiveDeck.GetCardsInDeck == null)
        {
            Debug.Log("Playerdata deck error");
            return;
        }

        var deck = _currentShownDeck; //PlayerDataManager.Instance.GetActiveDeck;            
        if (deck == null) return;

        if (_deckTitleText != null)
            _deckTitleText.text = $"All Cards in {deck.GetDeckName}";

        ClearScrollviewContent(_deckScrollView.content);

        var cardsToShow = new List<CardAbilityDefinition>(deck.GetCardsInDeck);
        if (deck == PlayerDataManager.Instance.GetActiveDeck && PlayerDataManager.Instance.GetOwnedCards != null)
            foreach (var card in PlayerDataManager.Instance.GetOwnedCards)
                cardsToShow.Add(card);

        int contentIndex = -1;
        foreach (var card in cardsToShow)
        {
            if (card == null) continue;

            GameObject content = Instantiate(_cardContentPrefab, Vector3.zero, Quaternion.identity, _deckScrollView.content);

            TextMeshProUGUI[] cardTextFieldsUI = content.GetComponentsInChildren<TextMeshProUGUI>();
            // Update text content
            cardTextFieldsUI[0].text = card.GetCardName;
            cardTextFieldsUI[1].text = card.GetDescription;
            cardTextFieldsUI[2].text = card.GetApCost.ToString();

            contentIndex++;
            var rt = content.GetComponent<RectTransform>();
            //SetCardLibraryContentTransform(ref rt, contentIndex);

            var addCardButton = content.GetComponentInChildren<Button>();
            if (addCardButton == null) continue;
            addCardButton?.gameObject.SetActive(false);
        }
    }
    private void ClearScrollviewContent(RectTransform contentTransform)
    {
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
            Destroy(contentTransform.GetChild(i).gameObject);
    }

    //Swap current deck
    public void SwapCurrentDeck(bool onEnable = false)
    {
        ClearScrollviewContent(_deckScrollView.content);

        var temp = onEnable ? PlayerDataManager.Instance.GetActiveDeck : GetCurrentDeckFromDropdown();
        if (temp == null) return;
        _currentShownDeck = temp;
        _deckDropdown.captionText.text = _currentShownDeck.GetDeckName;
        SetDropdownValueFromDeckName(_currentShownDeck.GetDeckName);

        BuildDeckScrollViewContent();
    }
    //Grab correct deck from dropdown value
    private Deck GetCurrentDeckFromDropdown()
    {
        if (_deckDropdown == null || _deckDropdown.options.Count == 0 || _cardAndDeckLibrary == null) return null;

        int entryIndex = _deckDropdown.value;

        string selectedDeckName = _deckDropdown.options[entryIndex].text;
        var deck = _cardAndDeckLibrary.GetDeckFromName(selectedDeckName, false);
        if (deck == null)
            if (PlayerDataManager.Instance != null)
                foreach (var playerDeck in PlayerDataManager.Instance.GetAllPlayerDecks)
                    if (playerDeck != null && playerDeck.GetDeckName == selectedDeckName)
                        deck = playerDeck;

        return deck;
    }
    //Grab all deck options from player data manager
    private void GrabAllDeckOptions()
    {
        if (_deckDropdown == null) return;
        _deckDropdown.options.Clear();

        // put default decks into dropdown options
        foreach (var deck in _cardAndDeckLibrary.GetDecksInProject)
            if (deck != null)
                _deckDropdown.options.Add(new(deck.GetDeckName));

        // put player decks into dropdown options
        foreach (var deck in PlayerDataManager.Instance.GetAllPlayerDecks)
            if (deck != null)
                _deckDropdown.options.Add(new(deck.GetDeckName));
    }
    //Set dropdown value/deck from given deck name
    private bool SetDropdownValueFromDeckName(string deckName)
    {
        if (_deckDropdown == null) return false;

        foreach (var option in _deckDropdown.options)
            if (option != null && option.text == deckName)
            {
                _deckDropdown.value = _deckDropdown.options.IndexOf(option);
                break;
            }
        return true;
    }
}
