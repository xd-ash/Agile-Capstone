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

        // runtime deck support
        public List<CardAbilityDefinition> runtimeAddedDefinitions = new List<CardAbilityDefinition>();
        private List<CardAbilityDefinition> _runtimeDeckList = new List<CardAbilityDefinition>();

        private Dictionary<Transform, Sequence> _activeSequences = new Dictionary<Transform, Sequence>();
        [SerializeField] private float _tweenDuration = 0.25f;

        public Transform cardActivePos;// temp card position when activated (to avoid cards blocking grid)

        public Action OnCardAblityCancel; //placeholder event for properly cancelling unit coroutines on card ability cancel

        private void Start()
        {
            AbilityEvents.OnAbilityUsed += RemoveSelectedCard;

            // include any persisted/purchased cards into the runtime-added definitions
            if (PlayerCollection.instance != null)
            {
                foreach (var def in PlayerCollection.instance.ownedCards)
                {
                    if (def != null && !runtimeAddedDefinitions.Contains(def))
                        runtimeAddedDefinitions.Add(def);
                }
            }

            ShuffleDeck(); // Add shuffle before any cards are drawn

            cardActivePos = transform.Find("CardActivePos");
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
            ArrangeCardGOs();

            // destroy world prefab
            if (selectedCard?.CardTransform != null)
                Destroy(selectedCard.CardTransform.gameObject);

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
                DrawCard();
            }
        }

        public CardAbilityDefinition[] PeekTopDefinitions(int count)
        {
            if ((_runtimeDeckList == null || _runtimeDeckList.Count == 0) && (_deck == null || _deck.GetDeck == null))
                return Array.Empty<CardAbilityDefinition>();

            var source = (_runtimeDeckList != null && _runtimeDeckList.Count > 0)
                ? _runtimeDeckList
                : new List<CardAbilityDefinition>(_deck.GetDeck);

            if (count <= 0) return Array.Empty<CardAbilityDefinition>();

            int available = Math.Max(0, Math.Min(count, source.Count - _topCardOfDeck));
            if (available == 0) return Array.Empty<CardAbilityDefinition>();

            CardAbilityDefinition[] result = new CardAbilityDefinition[available];
            source.CopyTo(_topCardOfDeck, result, 0, available);
            return result;
        }

        public void DrawStartingHand(int count)
        {
            if (count <= 0) return;

            DiscardAll();

            int toDraw = Mathf.Min(count, _maxCards);
            for (int i = 0; i < toDraw; i++)
                DrawCard();
        }

        public void DiscardAll()
        {
            if (_cardsInHand == null || _cardsInHand.Count == 0)
                return;

            foreach (var card in _cardsInHand)
            {
                if (card?.CardTransform != null)
                {
                    Destroy(card.CardTransform.gameObject);
                }
            }

            _cardsInHand.Clear();
            _nextCardInHandIndex = 0;
            _currentHandSize = 0;

            ArrangeCardGOs();
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
            if (_activeSequences.TryGetValue(transform, out Sequence oldSequence))
            {
                oldSequence.Kill();
                _activeSequences.Remove(transform);
            }

            Sequence sequence = DOTween.Sequence();
            
            float duration = isHovered ? _tweenDuration * 0.4f : _tweenDuration;
            Ease easeType = isHovered ? Ease.OutQuad : Ease.InOutQuad;

            sequence.Join(transform.DOMove(targetPosition, duration).SetEase(easeType));
            sequence.Join(transform.DORotateQuaternion(targetRotation, duration).SetEase(easeType));

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

        public Sequence GetActiveTweenSequence(Transform transform)
        {
            if (_activeSequences.TryGetValue(transform, out Sequence sequence))
            {
                return sequence;
            }
            return null;
        }
        /// <summary>
        /// Returns the full list of CardAbilityDefinition objects that make up the runtime deck
        /// (base deck + any runtime/purchased definitions).
        /// </summary>
        public CardAbilityDefinition[] GetRuntimeDeckDefinitions()
        {
            var list = new List<CardAbilityDefinition>();

            if (_deck != null && _deck.GetDeck != null)
                list.AddRange(_deck.GetDeck);

            if (runtimeAddedDefinitions != null && runtimeAddedDefinitions.Count > 0)
                list.AddRange(runtimeAddedDefinitions);

            return list.ToArray();
        }

        /// <summary>
        /// Debug helper: print the runtime deck to the Unity Console.
        /// Useful for quick runtime checks (call from UI button, hotkey, inspector button or code).
        /// </summary>
        public void LogRuntimeDeck()
        {
            var defs = GetRuntimeDeckDefinitions();
            Debug.Log($"[CardManager] Runtime deck contains {defs.Length} definitions.");
            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];
                Debug.Log($"[CardManager] #{i}: {(d != null ? d.GetCardName : "<null>")}");
            }
        }
    }
}