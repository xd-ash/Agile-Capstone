using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

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
        }

        public int _topCardOfDeck = 0;
        public int _nextCardInHandIndex = 0;
        public int _maxCards = 100;
        public int _currentHandSize = 0;

        [SerializeField] private Deck _deck;
        [SerializeField] public List<Card> _cardsInHand = new();
        [SerializeField] private SplineContainer splineContainer;
        public Card selectedCard = null;

        private Dictionary<Transform, Sequence> _activeSequences = new Dictionary<Transform, Sequence>();
        [SerializeField] private float _tweenDuration = 0.25f;

        private void Start()
        {
            AbilityEvents.OnAbilityUsed += RemoveCard;
            ShuffleDeck(); // Add shuffle before any cards are drawn
        }

        private void ShuffleDeck()
        {
            if (_deck == null || _deck.GetDeck == null || _deck.GetDeck.Length <= 1) return;

            // Fisher-Yates shuffle algorithm
            CardAbilityDefinition[] cards = _deck.GetDeck;
            for (int i = cards.Length - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                // Swap
                CardAbilityDefinition temp = cards[i];
                cards[i] = cards[randomIndex];
                cards[randomIndex] = temp;
            }
        }

        public void RemoveCard()
        {
            // remove selectedCard from hand data
            _cardsInHand.Remove(selectedCard);
            _currentHandSize--;
            ArrangeCardGOs();

            // destroy world prefab
            if (selectedCard?.CardTransform != null)
                Destroy(selectedCard.CardTransform.gameObject);

            selectedCard = null;
        }

        public void DrawCard()
        {
            AudioManager.instance.PlayDrawCardSfx();

            if (_currentHandSize >= _maxCards) return;

            Card newCard = null;
            if (_topCardOfDeck < _deck.GetDeck.Length)
                newCard = new Card(_deck.GetDeck[_topCardOfDeck]); // This creates the card with SO data
            if (newCard == null) return;

            _cardsInHand.Add(newCard);
            CreateCardPrefab(newCard);

            _topCardOfDeck++;
            _nextCardInHandIndex++;
            _currentHandSize++;

            ArrangeCardGOs();
        }

        public void CreateCardPrefab(Card card)
        {
            GameObject cardGO = Instantiate(Resources.Load<GameObject>("CardTestPrefab"), transform);
            if (!cardGO.GetComponent<CardSelect>()) cardGO.AddComponent<CardSelect>();
            cardGO.GetComponent<CardSelect>().OnPrefabCreation(card);
            card.CardTransform = cardGO.transform;
        }

        public void ArrangeCardGOs()
        {
            if (_currentHandSize == 0) return;
            var spline = splineContainer != null ? splineContainer.Spline : null;
            if (spline == null)
            {
                Debug.LogWarning("CardManager: SplineContainer or Spline is not assigned.");
                return;
            }

            int count = Mathf.Min(_currentHandSize, _cardsInHand.Count);
            int slots = Mathf.Max(1, count);

            float cardSpacing = 1f / slots;

            // Center the group along the spline range [0,1]
            float firstCardPos = 0.5f - (count - 1) * (cardSpacing / 2f);
            firstCardPos = Mathf.Clamp01(firstCardPos);

            for (int i = 0; i < count; i++)
            {
                float t = firstCardPos + i * cardSpacing;
                t = Mathf.Clamp01(t);

                Vector3 splinePosition = spline.EvaluatePosition(t);
                Vector3 forward = spline.EvaluateTangent(t);
                Vector3 up = spline.EvaluateUpVector(t);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                var tr = _cardsInHand[i]?.CardTransform;
                if (tr != null)
                {
                    UpdateTransformWithTween(tr, splinePosition, rotation, false);
                }
            }
        }

        /// <summary>
        /// Draw multiple cards (respects existing DrawCard logic such as max hand size).
        /// </summary>
        public void DrawMultiple(int count)
        {
            if (count <= 0) return;
            for (int i = 0; i < count; i++)
            {
                // DrawCard already guards against max hand size and end-of-deck.
                DrawCard();
            }
        }

        /// <summary>
        /// Return up to <paramref name="count"/> CardAbilityDefinition objects from the top of the deck
        /// without modifying deck state (non-destructive peek).
        /// </summary>
        public CardAbilityDefinition[] PeekTopDefinitions(int count)
        {
            if (_deck == null || _deck.GetDeck == null || count <= 0)
                return Array.Empty<CardAbilityDefinition>();

            int available = Mathf.Max(0, Mathf.Min(count, _deck.GetDeck.Length - _topCardOfDeck));
            if (available == 0) return Array.Empty<CardAbilityDefinition>();

            CardAbilityDefinition[] result = new CardAbilityDefinition[available];
            Array.Copy(_deck.GetDeck, _topCardOfDeck, result, 0, available);
            return result;
        }

        /// <summary>
        /// Draw a fresh starting hand (discard existing then draw up to count, but never exceed _maxCards).
        /// </summary>
        public void DrawStartingHand(int count)
        {
            // Defensive: make sure we don't exceed _maxCards
            if (count <= 0) return;

            // Ensure hand is empty first (prevents duplicates)
            DiscardAll();

            int toDraw = Mathf.Min(count, _maxCards);
            for (int i = 0; i < toDraw; i++)
                DrawCard();
        }

        /// <summary>
        /// Discard all cards currently in hand (destroy their prefabs, clear data).
        /// </summary>
        public void DiscardAll()
        {
            if (_cardsInHand == null || _cardsInHand.Count == 0)
                return;

            // Destroy any world prefabs
            foreach (var card in _cardsInHand)
            {
                if (card?.CardTransform != null)
                {
                    Destroy(card.CardTransform.gameObject);
                }
            }

            // Clear hand data
            _cardsInHand.Clear();
            _nextCardInHandIndex = 0;
            _currentHandSize = 0;

            ArrangeCardGOs();
        }

        /// <summary>
        /// Called by CardUI or CardSelect when player chooses a card via UI or world object.
        /// Centralizes selection behavior used previously by CardSelect.OnMouseDown.
        /// </summary>
        public void SelectCard(Card card)
        {
            if (PauseMenu.isPaused) return;
            if (card == null) return;

            selectedCard = card;

            // Existing workflow used in CardSelect:
            AbilityEvents.TargetingStarted();
            card.GetCardAbility.UseAility(TurnManager.GetCurrentUnit);
        }

        // Add these methods to the CardManager class:
        public void ReorderCard(Card card, int newIndex)
        {
            if (card == null || _cardsInHand == null) return;
            
            int currentIndex = _cardsInHand.IndexOf(card);
            if (currentIndex == -1 || currentIndex == newIndex) return;

            _cardsInHand.RemoveAt(currentIndex);
            newIndex = Mathf.Clamp(newIndex, 0, _cardsInHand.Count);
            _cardsInHand.Insert(newIndex, card);
            ArrangeCardGOs();
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
            ArrangeCardGOs();
        }

        private void UpdateTransformWithTween(Transform transform, Vector3 targetPosition, Quaternion targetRotation, bool isHovered)
        {
            // Kill any existing sequence for this transform
            if (_activeSequences.TryGetValue(transform, out Sequence oldSequence))
            {
                oldSequence.Kill();
                _activeSequences.Remove(transform);
            }

            // Create a new sequence
            Sequence sequence = DOTween.Sequence();
            
            float duration = isHovered ? _tweenDuration * 0.4f : _tweenDuration;
            Ease easeType = isHovered ? Ease.OutQuad : Ease.InOutQuad;

            sequence.Join(transform.DOMove(targetPosition, duration).SetEase(easeType));
            sequence.Join(transform.DORotateQuaternion(targetRotation, duration).SetEase(easeType));

            // Store the new sequence
            _activeSequences[transform] = sequence;
        }

        private void OnDisable()
        {
            foreach (var sequence in _activeSequences.Values)
            {
                sequence.Kill();
            }
            _activeSequences.Clear();
        }

        public void UpdateCardPosition(Card card, bool isHovered)
        {
            if (card == null || splineContainer == null) return;
            
            var spline = splineContainer.Spline;
            if (spline == null) return;

            int cardIndex = _cardsInHand.IndexOf(card);
            if (cardIndex == -1) return;

            int count = Mathf.Min(_currentHandSize, _cardsInHand.Count);
            int slots = Mathf.Max(1, count);
            float cardSpacing = 1f / slots;
            
            float firstCardPos = 0.5f - (count - 1) * (cardSpacing / 2f);
            firstCardPos = Mathf.Clamp01(firstCardPos);
            
            float t = firstCardPos + cardIndex * cardSpacing;
            t = Mathf.Clamp01(t);

            Vector3 splinePosition = spline.EvaluatePosition(t);
            Vector3 forward = spline.EvaluateTangent(t);
            Vector3 up = spline.EvaluateUpVector(t);
            Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

            if (isHovered)
            {
                splinePosition += Vector3.up * 0.5f;
            }

            var tr = card.CardTransform;
            if (tr != null)
            {
                UpdateTransformWithTween(tr, splinePosition, rotation, isHovered);
            }
        }

        // Add this helper method to CardManager class
        public Sequence GetActiveTweenSequence(Transform transform)
        {
            if (_activeSequences.TryGetValue(transform, out Sequence sequence))
            {
                return sequence;
            }
            return null;
        }
    }
}