using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    // Class manages the deck & card hand collections and funtionalities
    // such as shuffling, adding cards, drawing cards, etc.
    public class DeckAndHandManager : MonoBehaviour
    {
        //Singleton setup
        public static DeckAndHandManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);

            ShuffleDeck(); // Add shuffle before any cards are drawn
        }

        [SerializeField] private Transform _cardHandParent;

        private int _topCardOfDeck = 0;
        [SerializeField] private int _maxCards = 10;
        [SerializeField] public int _startingHandSize = 5; // draw this many cards at start of player turn

        [SerializeField] private List<Card> _cardsInHand = new();
        private Card _selectedCard = null;

        public bool _startingHandDrawn = false;// internal guard to avoid drawing twice for the same scene load

        public int CardsToDraw { get; private set; } = 0;
        private Coroutine _carDrawCoro;
        [SerializeField] private float _cardDrawDelay = 0.3f;

        public Transform CardActivePos { get; private set; } // temp card position to move card to when activated (avoid cards blocking grid)
        public Card GetSelectedCard => _selectedCard;
        public List<Card> CardsInHand => _cardsInHand;
        public int GetCurrentHandSize => _cardsInHand.Count;
        public int GetMaxHandSize => _maxCards;
        public bool CanDrawCard => _cardsInHand.Count < _maxCards && 
            _cardsInHand.Count < PlayerDataManager.Instance?.GetPlayerDeck?.GetCardsInDeck?.Count;

        public Action OnCardAblityCancel;
        public static Action<Transform> OnUpdateCardColliders;

        private void Start()
        {
            AbilityEvents.OnAbilityUsed += RemoveSelectedCard;
            CardActivePos = transform.Find("CardActivePos");
        }
        private void OnDestroy()
        {
            AbilityEvents.OnAbilityUsed -= RemoveSelectedCard;
        }
        //draws cards based on count param, which is default 1
        public void DrawCard(int count = 1)
        {
            CardsToDraw += count;
            if (_carDrawCoro == null)
                _carDrawCoro = StartCoroutine(DrawCardCoro());
        }

        private IEnumerator DrawCardCoro()
        {
            while (CardsToDraw > 0)
            {
                AudioManager.Instance?.PlayDrawCardSfx();
                var deck = PlayerDataManager.Instance.GetPlayerDeck;

                if (_cardsInHand.Count >= _maxCards)
                {
                    CardsToDraw = 0;
                    _carDrawCoro = null;
                    yield break;
                }
                if (deck == null || deck.GetCardsInDeck == null || deck.GetCardsInDeck.Count == 0) yield break;

                // If we've exhausted the deck, reshuffle it and reset the top index
                if (_topCardOfDeck >= deck.GetCardsInDeck.Count)
                {
                    ShuffleDeck();
                    _topCardOfDeck = 0;
                    continue;
                }

                var newCard = CreateCardAndPrefab();
                if (newCard == null || newCard.GetCardAbility == null)
                {
                    CardsToDraw--;
                    continue;
                }

                _cardsInHand.Add(newCard);

                _topCardOfDeck++;

                // If we've exhausted the deck, reshuffle it and reset the top index
                if (_topCardOfDeck >= deck.GetCardsInDeck.Count)
                {
                    ShuffleDeck();
                    _topCardOfDeck = 0;
                }

                OnUpdateCardColliders?.Invoke(null);

                HandPositionController.Instance?.AdjustSplineKnotsOnHandSize();
                CardSplineManager.Instance?.ArrangeCardGOs();

                CardsToDraw--;
                yield return new WaitForSeconds(_cardDrawDelay);
            }

            CardsToDraw = 0;
            _carDrawCoro = null;
        }

        // Modified: optional force parameter, and guard to avoid drawing multiple times per load
        public void DrawStartingHand(bool force = false)
        {
            _startingHandDrawn = true;

            if (!force && _startingHandDrawn) return;
            _startingHandDrawn = true;

            if (_startingHandSize <= 0) return;

            DiscardAll();

            //_startingHandSize += PlayerDataManager.Instance.GetStartingHandSizeBuff;

            int toDraw = Mathf.Min(_startingHandSize, _maxCards);
            DrawCard(toDraw);
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

            CardSplineManager.Instance?.ArrangeCardGOs();
        }

        //disable non hovered card cox colliders to avoid overlap issues
        public void ToggleCollidersOnHover(Transform triggeredCard, bool disableOtherCards)
        {
            for (int i = 0; i < _cardsInHand.Count; i++)
            {
                var cardTrans = _cardsInHand[i]?.GetCardTransform;
                var bc = cardTrans?.GetComponent<BoxCollider2D>();
                if (cardTrans == triggeredCard || cardTrans == null || bc == null) continue;
                bc.enabled = !disableOtherCards;
            }
        }
        public void SelectCard(Card card)
        {
            if (PauseMenu.isPaused || card == null) return;

            // Block cards if tutorial is active and not in card step
            if (TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None &&
                TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.CardsOnly &&
                TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.MoveAndCards)
                return;

            if (_selectedCard != card)
                _selectedCard = card;
        }

        public void RemoveSelectedCard(Team unitTeam = Team.Friendly)
        {
            if (unitTeam == Team.Enemy) return;

            // remove selectedCard from hand data
            _cardsInHand.Remove(_selectedCard);
            CardSplineManager.Instance?.RemoveSelectedCard(_selectedCard);

            _selectedCard = null;
        }

        public void RemoveCard(Card card)
        {
            _cardsInHand.Remove(card);
        }

        public void InsertCard(Card card)
        {
            if (!_cardsInHand.Contains(card))
                _cardsInHand.Insert(Math.Max(CalculateCardIndex(card), 0), card);
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
            CardSplineManager.Instance?.ArrangeCardGOs();
        }

        public void AddCardToRuntimeDeck(Card card)
        {
            if (card == null) return;

            PlayerDataManager.Instance.UpdateCardData(card);
            ShuffleDeck();
        }

        private void ShuffleDeck()
        {
            if (PlayerDataManager.Instance == null) return;

            if (PlayerDataManager.Instance.GetPlayerDeck.GetCardsInDeck == null || PlayerDataManager.Instance.GetPlayerDeck.GetCardsInDeck.Count <= 1) return;
            var cardsInDeck = new List<Card>(PlayerDataManager.Instance.GetPlayerDeck.GetCardsInDeck);

            UnityEngine.Random.InitState(PlayerDataManager.Instance.GetGeneralSeed - int.Parse($"{PlayerDataManager.Instance.GetCurrentNodeIndex.x}{PlayerDataManager.Instance.GetCurrentNodeIndex.y}"));

            // Fisher-Yates shuffle algorithm on runtime list
            for (int i = cardsInDeck.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                var temp = cardsInDeck[i];
                cardsInDeck[i] = cardsInDeck[randomIndex];
                cardsInDeck[randomIndex] = temp;
            }

            //update player deck with shuffled version;
            PlayerDataManager.Instance.UpdateCardData(new(cardsInDeck));

            _topCardOfDeck = 0;
        }

        public Card[] PeekTopCards(int count)
        {
            if (count <= 0) return Array.Empty<Card>();

            var deck = new Deck(PlayerDataManager.Instance.GetPlayerDeck);
            if (deck.GetCardsInDeck == null || deck.GetCardsInDeck.Count == 0) return Array.Empty<Card>();
            var cardsInDeck = deck.GetCardsInDeck;

            int available = Math.Max(0, Math.Min(count, cardsInDeck.Count - _topCardOfDeck));
            if (available == 0) return Array.Empty<Card>();

            Card[] result = new Card[available];
            cardsInDeck.CopyTo(_topCardOfDeck, result, 0, available);
            return result;
        }

        public Card CreateCardAndPrefab()
        {
            var deck = new Deck(PlayerDataManager.Instance.GetPlayerDeck);
            if (deck.GetCardsInDeck == null || deck.GetCardsInDeck.Count == 0) return null;
            var cardsInDeck = deck.GetCardsInDeck;

            if (_topCardOfDeck >= cardsInDeck.Count) return null;
            Card newCard = cardsInDeck[_topCardOfDeck];

            while (_cardsInHand.Contains(newCard))
            {
                _topCardOfDeck++;
                if (_topCardOfDeck >= cardsInDeck.Count) return null;
                newCard = cardsInDeck[_topCardOfDeck];
            }

            GameObject cardGO = Instantiate(Resources.Load<GameObject>("NewCardPrefab"), _cardHandParent);

            newCard.OnPrefabCreation(cardGO.transform);
            CardPrefabSetterUpper.SetupCardPrefab(newCard, CardState.Combat);

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
            if (PlayerDataManager.Instance == null) return;
            var cardsInDeck = PlayerDataManager.Instance.GetPlayerDeck.GetCardsInDeck;

            Debug.Log($"[CardManager] Runtime deck contains {cardsInDeck.Count} definitions.");
            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                var d = cardsInDeck[i];
                Debug.Log($"[CardManager] #{i}: {(d != null ? d.GetCardName : "<null>")}");
            }
        }
    }
}