using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
//using UnityEngine.SceneManagement;

namespace CardSystem
{
    public class CardManager : MonoBehaviour
    {
        //Singleton setup
        public static CardManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);

            OnSceneLoaded();
        }

        private int _topCardOfDeck = 0;
        private int _nextCardInHandIndex = 0;
        [SerializeField] private int _maxCards = 100;
        /**/public int _currentHandSize = 0;

        [SerializeField] private Deck _deck;
        [SerializeField] public List<Card> _cardsInHand = new();
        public Card selectedCard = null;

        // runtime deck support
        public List<CardAbilityDefinition> runtimeAddedDefinitions = new List<CardAbilityDefinition>();
        private List<CardAbilityDefinition> _runtimeDeckList = new List<CardAbilityDefinition>();

        public Transform cardActivePos;// temp card position to move card to when activated (avoid cards blocking grid)

        public Action OnCardAblityCancel; //placeholder event for properly cancelling unit coroutines on card ability cancel

        [Header("Auto-starting hand")]
        [Tooltip("Fallback starting hand size used if TurnManager isn't available at draw time.")]
        [SerializeField] private int defaultStartingHandSize = 5;

        private bool _startingHandDrawn = false;// internal guard to avoid drawing twice for the same scene load

        public int GetCurrentHandSize => _currentHandSize;
        public List<Card> GetCardsInHand => _cardsInHand;
        public CardAbilityDefinition[] GetRuntimeDeck => _runtimeDeckList.ToArray();

        private void Start()
        {
            AbilityEvents.OnAbilityUsed += RemoveSelectedCard;

            // include any persisted/purchased cards into the runtime-added definitions
            if (PlayerCollection.instance != null)
                foreach (var def in PlayerCollection.instance.ownedCards)
                    if (def != null && !runtimeAddedDefinitions.Contains(def))
                        runtimeAddedDefinitions.Add(def);

            ShuffleDeck(); // Add shuffle before any cards are drawn

            cardActivePos = transform.Find("CardActivePos");
        }

        private IEnumerator WaitAndDrawStartingHand()
        {
            // give other Start() calls a frame to run (TurnManager, etc.)
            yield return new WaitForEndOfFrame();

            if (_startingHandDrawn || _cardsInHand != null && _cardsInHand.Count > 0)
            {
                Debug.LogWarning("Starting Hand Draw Attempt Fail.");
                yield break;
            }

            // prefer TurnManager configured starting hand size when available
            int count = TurnManager.instance == null ? TurnManager.instance._startingHandSize : defaultStartingHandSize;

            DrawStartingHand(count);
        }

        private void OnSceneLoaded(/*Scene scene, LoadSceneMode mode*/)
        {
            // Reset guard on new scene so a fresh starting hand can be drawn
            _startingHandDrawn = false;
            // attempt to draw for the newly loaded scene
            StartCoroutine(WaitAndDrawStartingHand());
        }

        private void ShuffleDeck()
        {
            _runtimeDeckList.Clear();

            if (_deck != null && _deck.GetDeck != null)
                _runtimeDeckList.AddRange(_deck.GetDeck);

            if (runtimeAddedDefinitions != null && runtimeAddedDefinitions.Count > 0)
                _runtimeDeckList.AddRange(runtimeAddedDefinitions);

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

        // Allow forcing a redraw by passing force = true
        public void AddDefinitionToRuntimeDeck(CardAbilityDefinition def)
        {
            if (def == null) return;
            if (!runtimeAddedDefinitions.Contains(def))
            {
                runtimeAddedDefinitions.Add(def);
                ShuffleDeck();
            }
        }

        public void RemoveSelectedCard(Team unitTeam = Team.Friendly)
        {
            if (unitTeam == Team.Enemy) return;

            // remove selectedCard from hand data
            _cardsInHand.Remove(selectedCard);
            _currentHandSize--;
            CardSplineManager.instance.RemoveSelectedCard(selectedCard);

            selectedCard = null;
        }

        public void DrawCard()
        {
            AudioManager.instance?.PlayDrawCardSfx();

            if (_currentHandSize >= _maxCards) return;
            //if (_deck == null || _deck.GetDeck == null || _deck.GetDeck.Length == 0) return;

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

            Card newCard = null;
            if (_topCardOfDeck < _runtimeDeckList.Count)
                newCard = new Card(_runtimeDeckList[_topCardOfDeck]); // This creates the card with SO data
            if (newCard == null) return;

            _cardsInHand.Add(newCard);
            CreateCardPrefab(newCard);

            _topCardOfDeck++;
            _nextCardInHandIndex++;
            _currentHandSize++;

            // If we've exhausted the deck, reshuffle it and reset the top index
            if (_topCardOfDeck >= _deck.GetDeck.Length)
            {
                ShuffleDeck();
                _topCardOfDeck = 0;
            }

            CardSplineManager.instance.ArrangeCardGOs();
        }

        public void CreateCardPrefab(Card card)
        {
            GameObject cardGO = Instantiate(Resources.Load<GameObject>("CardTestPrefab"), transform);
            if (!cardGO.GetComponent<CardSelect>()) cardGO.AddComponent<CardSelect>();
            cardGO.GetComponent<CardSelect>().OnPrefabCreation(card);
            card.CardTransform = cardGO.transform;
        }

        /// <summary>
        /// Draw multiple cards (respects existing DrawCard logic such as max hand size).
        /// </summary>
        public void DrawMultiple(int count)
        {
            if (count <= 0) return;
            for (int i = 0; i < count; i++)
            {
                DrawCard();
            }
        }

        // Modified: optional force parameter, and guard to avoid drawing multiple times per load
        public void DrawStartingHand(int count, bool force = false)
        {
            if (!force && _startingHandDrawn) return;
            _startingHandDrawn = true;
            
            if (count <= 0) return;

            DiscardAll();

            int toDraw = Mathf.Min(count, _maxCards);
            for (int i = 0; i < toDraw; i++)
                DrawCard();
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

        public void DiscardAll()
        {
            if (_cardsInHand == null || _cardsInHand.Count == 0)
                return;

            if (selectedCard?.CardTransform != null)
                Destroy(selectedCard.CardTransform.gameObject);

            foreach (var card in _cardsInHand)
                if (card?.CardTransform != null)
                    Destroy(card.CardTransform.gameObject);

            _cardsInHand.Clear();
            _nextCardInHandIndex = 0;
            _currentHandSize = 0;
            selectedCard = null;

            CardSplineManager.instance.ArrangeCardGOs();
        }

        public void SelectCard(Card card)
        {
            if (PauseMenu.isPaused) return;
            if (card == null) return;

            selectedCard = card;

            AbilityEvents.TargetingStarted();
            card.GetCardAbility.UseAility(TurnManager.GetCurrentUnit);
        }

        public void ReorderCard(Card card, int newIndex)
        {
            if (card == null || _cardsInHand == null) return;
            
            int currentIndex = _cardsInHand.IndexOf(card);
            if (currentIndex == -1 || currentIndex == newIndex) return;

            _cardsInHand.RemoveAt(currentIndex);
            newIndex = Mathf.Clamp(newIndex, 0, _cardsInHand.Count);
            _cardsInHand.Insert(newIndex, card);
            CardSplineManager.instance.ArrangeCardGOs();
        }

        public void PreviewReorder(int fromIndex, int toIndex)
        {
            if (_cardsInHand == null || fromIndex == toIndex) return;
            if (fromIndex < 0 || fromIndex >= _cardsInHand.Count) return;
            if (toIndex < 0 || toIndex >= _cardsInHand.Count) return;
            
            var card = _cardsInHand[fromIndex];
            _cardsInHand.RemoveAt(fromIndex);
            toIndex = Mathf.Clamp(toIndex, 0, _cardsInHand.Count);
            _cardsInHand.Insert(toIndex, card);
            CardSplineManager.instance.ArrangeCardGOs();
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