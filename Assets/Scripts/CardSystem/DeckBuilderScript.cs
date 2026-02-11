using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CardSystem;
using static GameObjectPool;

public class DeckBuilderScript : MonoBehaviour
{
    private CardAndDeckLibrary _cardAndDeckLibrary;

    [SerializeField] private TMP_Dropdown _deckDropdown;
    [SerializeField] private ScrollRect _cardScrollView;
    [SerializeField] private ScrollRect _deckScrollView;

    [SerializeField] private GameObject _cardContentPrefab;
    [SerializeField] private GameObject _deckContentPrefab;

    [SerializeField] private Deck _currDeck;
    [SerializeField] private List<CardAbilityDefinition> _tempDeck = new();
    private bool _isCurrentDeckEditable => !_cardAndDeckLibrary.GetDecksInProject.Contains(_currDeck);

    public static DeckBuilderScript Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void OnEnable()
    {
        _cardAndDeckLibrary = Resources.Load<CardAndDeckLibrary>("CardAndDeckLibrary");

        Invoke(nameof(LateStartInits), 0.1f);
    }
    private void LateStartInits()
    {
        GrabAllDeckOptions();
        SwapCurrentDeck(); //initial deck grab, temp deck setup, & deck scrollview build 
        BuildCardLibraryScrollViewContent();
    }

    //Create all card content in the card library scrollview
    private void BuildCardLibraryScrollViewContent()
    {
        if (_currDeck == null || _cardContentPrefab == null || _cardScrollView == null) return;

        ClearScrollviewContent(_cardScrollView.content);

        int contentIndex = -1;
        foreach (var card in _cardAndDeckLibrary.GetCardsInProject)
        {
            if (card == null) continue;

            GameObject content = Spawn(_cardContentPrefab, Vector3.zero, Quaternion.identity, _cardScrollView.content);

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
            addCardButton?.onClick.AddListener(() => AddCardToTempDeck(card));
        }
    }
    private void SetCardLibraryContentTransform(ref RectTransform rt, int childIndex)
    {
        if (rt == null || childIndex == -1) return;

        float w = rt.rect.width;
        float h = rt.rect.height;
        float scrollViewWidth = _cardScrollView.viewport.rect.width;
        int maxNumHorizontal = (int)(scrollViewWidth / w);
        int ix = childIndex % maxNumHorizontal;
        int iy = childIndex / maxNumHorizontal;

        float x = 0.5f * w + ix * w;
        float y = 0.5f * h + iy * h;

        rt.localPosition = new Vector2(x, -y);
    }

    //Create all deck content in the deck scrollview
    private void BuildDeckScrollViewContent()
    {
        if (_currDeck == null || _deckContentPrefab == null || _deckScrollView == null) return;

        ClearScrollviewContent(_deckScrollView.content);

        int contentIndex = -1;
        foreach (var card in _tempDeck)
        {
            if (card == null) continue;
            
            GameObject content = GameObject.Instantiate(_deckContentPrefab, Vector3.zero, Quaternion.identity, _deckScrollView.content);
            content.name = card.name;

            var texts = content.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts == null || texts.Length < 2) continue;
            texts[0].text = card.name;
            //texts[1].text = GetCardAmountInCurrentDeck(card).ToString();

            contentIndex++;
            var rt = content.GetComponent<RectTransform>();
            //SetDeckCardContentTransform(ref rt, contentIndex);

            var removeCardButton = content.GetComponentInChildren<Button>();
            if (removeCardButton == null) continue;

            removeCardButton?.onClick.AddListener(() => RemoveCardFromTempDeck(card));
            removeCardButton.gameObject.SetActive(_isCurrentDeckEditable);
        }
    }
    private void SetDeckCardContentTransform(ref RectTransform rt, int childIndex)
    {
        if (rt == null || childIndex == -1) return;

        float w = rt.rect.width;
        float h = rt.rect.height;
        float scrollViewHeight = _cardScrollView.viewport.rect.height;

        float x = 0.5f * w;
        float y = 0.5f * h + childIndex * h;

        rt.localPosition = new Vector2(x, -y);
    }

    //Add card to pending/temp deck list
    private void AddCardToTempDeck(CardAbilityDefinition card)
    {
        if (card == null || _currDeck == null ||!_isCurrentDeckEditable) return;
        if (_tempDeck == null) _tempDeck = new();
        _tempDeck.Add(card);

        BuildDeckScrollViewContent();
    }

    //remove card from pending/temp deck list
    private void RemoveCardFromTempDeck(CardAbilityDefinition card)
    {
        if (card == null) return;
        if (_tempDeck == null) _tempDeck = new();
        if (_tempDeck.Contains(card))
            _tempDeck.Remove(card);
        
        BuildDeckScrollViewContent();
    }

    //add pending/temp cards to current deck & update player data decks
    public void ConfirmDeckAdditions()
    {
        _currDeck.ClearDeck(_tempDeck); 
        PlayerDataManager.Instance?.CreateOrAdjustDeck(_currDeck);
        SaveLoadScript.SaveGame?.Invoke();
    }

    //create new deck to edit
    public void CreateNewDeck(string deckName)
    {
        _currDeck = new Deck(deckName);
        AddDeckOption(_currDeck);
        SetDropdownValueFromDeckName(deckName);

        PlayerDataManager.Instance.SetActiveDeck(_currDeck);
        _tempDeck = new(_currDeck.GetCardsInDeck);
        _deckDropdown.captionText.text = _currDeck.GetDeckName;

        //SwapCurrentDeck();
        PlayerDataManager.Instance?.CreateOrAdjustDeck(_currDeck);
    }
    //delete deck from options & player data
    public void DeleteDeck()
    {
        if (_currDeck == null || !_isCurrentDeckEditable)
        {
            Debug.Log("Cannot delete starter decks. Replace this message with a popup or other system at some point");
            return;
        }

        PlayerDataManager.Instance?.DeleteDeck(_currDeck);
        RemoveDeckOption(_currDeck);
        _deckDropdown.value = 0; //reset to first value
        SwapCurrentDeck();
        SaveLoadScript.SaveGame?.Invoke();
    }

    //Return the amount of the given card in the current deck & in the pending/temp deck additions
    private int GetCardAmountInCurrentDeck(CardAbilityDefinition cardToCount)
    {
        if (_currDeck == null || cardToCount == null) return -1;

        int cardAmount = 0;

        foreach (var card in _currDeck.GetCardsInDeck)
            if (card == cardToCount)
                cardAmount++;

        foreach (var card in _tempDeck)
            if (card == cardToCount)
                cardAmount++;

        return cardAmount;
    }

    //Swap current deck
    public void SwapCurrentDeck()
    {
        ClearScrollviewContent(_deckScrollView.content);

        var temp = GetCurrentDeckFromDropdown();
        if (temp == null) return;
        _currDeck = temp;
        PlayerDataManager.Instance.SetActiveDeck(_currDeck);
        _tempDeck = new(_currDeck.GetCardsInDeck);
        _deckDropdown.captionText.text = _currDeck.GetDeckName;

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
    //Add deck dropdown option
    private void AddDeckOption(Deck deck)
    {
        if (_deckDropdown == null) return;
        foreach (var option in _deckDropdown.options)
            if (option != null && option.text == deck.GetDeckName)
                return; // if deckname exists in options, just return

        _deckDropdown.options.Add(new(deck.GetDeckName));
    }
    //Remove deck dropdown option
    private void RemoveDeckOption(Deck deck)
    {
        if (_deckDropdown == null) return;

        for (int i = _deckDropdown.options.Count - 1; i >= 0; i--)
            if (_deckDropdown.options[i].text == deck.GetDeckName)
            {
                _deckDropdown.options.RemoveAt(i);
                break;
            }
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
    private void ClearScrollviewContent(RectTransform contentTransform)
    {
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
            Destroy(contentTransform.GetChild(i).gameObject);
    }
}
