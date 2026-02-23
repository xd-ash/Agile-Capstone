using TMPro;
using UnityEngine;

public class DeckSelector : MonoBehaviour
{
    private CardAndDeckLibrary _cardAndDeckLibrary;

    [SerializeField] private TMP_Dropdown _deckDropdown;

    private void OnEnable()
    {
        _cardAndDeckLibrary = Resources.Load<CardAndDeckLibrary>("Libraries/CardAndDeckLibrary");

        GrabAllDeckOptions();
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
    //Swap current deck
    public void SwapCurrentDeck()
    {
        var temp = GetCurrentDeckFromDropdown();
        if (temp == null) return;
        PlayerDataManager.Instance.SetActiveDeck(temp);
        _deckDropdown.captionText.text = temp.GetDeckName;
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
}
