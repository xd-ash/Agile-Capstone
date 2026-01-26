using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    // Class manages the deck & card hand collections and funtionalities
    // such as shuffling, adding cards, drawing cards, etc.
    public class DeckAndHandManager : MonoBehaviour
    {
        //Singleton setup
        public static DeckAndHandManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);
        }

        private int _topCardOfDeck = 0;
        //private int _nextCardInHandIndex = 0; //Removed since it was unused, kept in comments in case its needed later
        [SerializeField] private int _maxCards = 100;
        [SerializeField] public int _startingHandSize = 5; // draw this many cards at start of player turn

        [SerializeField] private Deck _deck;
        [SerializeField] private List<Card> _cardsInHand = new();
        private Card _selectedCard = null;

        // runtime deck support
        private List<CardAbilityDefinition> _runtimeDeckList = new List<CardAbilityDefinition>();
        private bool _startingHandDrawn = false;// internal guard to avoid drawing twice for the same scene load

        public Deck GetDeck => _deck;
        public Transform CardActivePos { get; private set; } // temp card position to move card to when activated (avoid cards blocking grid)
        public Card GetSelectedCard => _selectedCard;
        public List<Card> CardsInHand => _cardsInHand;
        public int GetCurrentHandSize => _cardsInHand.Count;
        public CardAbilityDefinition[] GetRuntimeDeck => _runtimeDeckList.ToArray();

        public Action OnCardAblityCancel;

        private void Start()
        {
            AbilityEvents.OnAbilityUsed += RemoveSelectedCard;

            ShuffleDeck(); // Add shuffle before any cards are drawn
            CardActivePos = transform.Find("CardActivePos");
        }

        //draws cards based on count param, which is default 1
        public void DrawCard(int count = 1)
        {
            AudioManager.instance?.PlayDrawCardSfx();
            
            if (count <= 0) return;
            for (int i = 0; i < count; i++)
            {
                if (_cardsInHand.Count >= _maxCards) return;
                if (_deck == null || _deck.GetDeck == null || _deck.GetDeck.Length == 0) return;

                if (_runtimeDeckList == null || _runtimeDeckList.Count == 0)
                {
                    // build fallback minimal runtime list from _deck if necessary
                    if (_deck == null || _deck.GetDeck == null || _deck.GetDeck.Length == 0) return;
                    _runtimeDeckList = new List<CardAbilityDefinition>(_deck.GetDeck);
                }

                // If we've exhausted the deck, reshuffle it and reset the top index
                if (_topCardOfDeck >= _runtimeDeckList.Count)
                {
                    ShuffleDeck();
                    _topCardOfDeck = 0;
                }

                /*Card newCard = null;
                if (_topCardOfDeck < _runtimeDeckList.Count)
                    newCard = new Card(_runtimeDeckList[_topCardOfDeck]); // This creates the card with SO data
                if (newCard == null) return;
                */
                _cardsInHand.Add(CreateCardAndPrefab());

                _topCardOfDeck++;
                //_nextCardInHandIndex++;

                // If we've exhausted the deck, reshuffle it and reset the top index
                if (_topCardOfDeck >= _deck.GetDeck.Length)
                {
                    ShuffleDeck();
                    _topCardOfDeck = 0;
                }
            }

            CardSplineManager.instance.ArrangeCardGOs();
        }

        // Modified: optional force parameter, and guard to avoid drawing multiple times per load
        public void DrawStartingHand(bool force = true)
        {
            if (_startingHandSize <= 0) return;
            if (force && _startingHandDrawn) return;

            int toDraw = Mathf.Min(_startingHandSize, _maxCards);
            DrawCard(toDraw);

            _startingHandDrawn = true;
        }

        public void DiscardAll()
        {
            if (_cardsInHand == null || _cardsInHand.Count == 0)
                return;

            if (_selectedCard?.GetCardTransform != null)
                Destroy(_selectedCard.GetCardTransform.gameObject);

            foreach (var card in _cardsInHand)
                if (card?.GetCardTransform != null)
                    Destroy(card.GetCardTransform.gameObject);

            _cardsInHand.Clear();
            //_nextCardInHandIndex = 0;
            _selectedCard = null;

            CardSplineManager.instance.ArrangeCardGOs();
        }

        public void SelectCard(Card card)
        {
            if (PauseMenu.isPaused || card == null) return;
            if (_selectedCard != card)
                _selectedCard = card;
        }

        public void RemoveSelectedCard(Team unitTeam = Team.Friendly)
        {
            if (unitTeam == Team.Enemy) return;

            // remove selectedCard from hand data
            _cardsInHand.Remove(_selectedCard);
            CardSplineManager.instance.RemoveSelectedCard(_selectedCard);

            _selectedCard = null;
        }

        public void RemoveCard(Card card)
        {
            _cardsInHand.Remove(card);
        }

        public void InsertCard(Card card)
        {
            if (!_cardsInHand.Contains(card))
                _cardsInHand.Insert(CalculateCardIndex(card), card);
        }

        public void ClearSelection()
        {
            InsertCard(_selectedCard);
            ReorderCard(_selectedCard, CalculateCardIndex(_selectedCard));
            _selectedCard = null;
        }

        public void ReorderCard(Card card, int newIndex)
        {
            if (card == null || _cardsInHand == null) return;
            
            int currentIndex = _cardsInHand.IndexOf(card);
            if (currentIndex == newIndex || currentIndex == -1) return;
            _cardsInHand.RemoveAt(currentIndex);
            newIndex = Mathf.Clamp(newIndex, 0, _cardsInHand.Count);
            _cardsInHand.Insert(newIndex, card);
            CardSplineManager.instance.ArrangeCardGOs();
        }

        public void AddDefinitionToRuntimeDeck(CardAbilityDefinition def)
        {
            if (def == null) return;

            PlayerCardCollection.instance.Add(def);
            ShuffleDeck();
        }

        private void ShuffleDeck()
        {
            _runtimeDeckList.Clear();

            if (_deck != null && _deck.GetDeck != null)
                _runtimeDeckList.AddRange(_deck.GetDeck);

            // include any persisted/purchased cards into the runtime deck
            if (PlayerCardCollection.instance != null)
                foreach (var def in PlayerCardCollection.instance.GetOwnedCards)
                    if (def != null)
                        _runtimeDeckList.Add(def);

            if (_runtimeDeckList == null || _runtimeDeckList.Count <= 1) return;

            // Fisher-Yates shuffle algorithm on runtime list
            for (int i = _runtimeDeckList.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                var temp = _runtimeDeckList[i];
                _runtimeDeckList[i] = _runtimeDeckList[randomIndex];
                _runtimeDeckList[randomIndex] = temp;
            }

            _topCardOfDeck = 0;
        }

        public CardAbilityDefinition[] PeekTopDefinitions(int count)
        {
            if ((_runtimeDeckList == null || _runtimeDeckList.Count == 0) && (_deck == null || _deck.GetDeck == null))
                return Array.Empty<CardAbilityDefinition>();

            var source = (_runtimeDeckList != null && _runtimeDeckList.Count > 0)
                ? _runtimeDeckList : new List<CardAbilityDefinition>(_deck.GetDeck);

            if (count <= 0) return Array.Empty<CardAbilityDefinition>();

            int available = Math.Max(0, Math.Min(count, source.Count - _topCardOfDeck));
            if (available == 0) return Array.Empty<CardAbilityDefinition>();

            CardAbilityDefinition[] result = new CardAbilityDefinition[available];
            source.CopyTo(_topCardOfDeck, result, 0, available);
            return result;
        }

        public Card CreateCardAndPrefab(/*Card card*/)
        {
            GameObject cardGO = Instantiate(Resources.Load<GameObject>("CardTestPrefab"), transform);

            Card newCard = null;
            if (_topCardOfDeck < _runtimeDeckList.Count)
                newCard = new Card(_runtimeDeckList[_topCardOfDeck], cardGO.transform); // This creates the card with SO data

            if (!cardGO.TryGetComponent(out CardSelect cs))
                cs = cardGO.AddComponent<CardSelect>();
            cs.OnPrefabCreation(newCard);

            return newCard;
        }

        public int CalculateCardIndex(Card card)
        {
            if (card == null) return 0;

            var tr = card.GetCardTransform;

            if (_cardsInHand == null || _cardsInHand.Count <= 1) return -1;

            float myX = tr.position.x;
            for (int i = 0; i < _cardsInHand.Count; i++)
            {
                if (_cardsInHand[i] == card || _cardsInHand[i]?.GetCardTransform == null) continue;
                if (_cardsInHand[i].GetCardTransform.position.x > myX) return i;
            }
            return _cardsInHand.Count - 1;
        }


        /// <summary>
        /// Debug helper: print the runtime deck to the Unity Console.
        /// Useful for quick runtime checks (call from UI button, hotkey, inspector button or code).
        /// </summary>
        public void LogRuntimeDeck()
        {
            var defs = GetRuntimeDeck;
            Debug.Log($"[CardManager] Runtime deck contains {defs.Length} definitions.");
            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];
                Debug.Log($"[CardManager] #{i}: {(d != null ? d.GetCardName : "<null>")}");
            }
        }
    }
}