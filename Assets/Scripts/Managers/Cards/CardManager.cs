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
        public int _maxCards = 5;
        public int _currentHandSize = 0;

        [SerializeField] private Deck _deck;
        [SerializeField] private List<Card> _cardsInHand = new();
        [SerializeField] private SplineContainer splineContainer;
        public Card selectedCard = null;

        private void Start()
        {
            AbilityEvents.OnAbilityUsed += RemoveCard;
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
            if (_currentHandSize >= _maxCards) return;

            Card newCard = null;
            if (_topCardOfDeck < _deck.GetDeck.Length)
                newCard = new Card(_deck.GetDeck[_topCardOfDeck]);
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

        }

        public void ArrangeCardGOs()
        {
            if (_currentHandSize == 0) return;
            float cardSpacing = 1f / (_maxCards);
            float firstCardPos = 0f; //0.5f - (_currentHandSize - 1) * (cardSpacing / 2);
            Spline spline = splineContainer.Spline;
            for (int i = 0; i < _currentHandSize; i++)
            {
                float t = firstCardPos + i * cardSpacing;
                Vector3 splinePosition = spline.EvaluatePosition(t);
                Vector3 forward = spline.EvaluateTangent(t);
                Vector3 up = spline.EvaluateUpVector(t);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                _cardsInHand[i].CardTransform.DOMove(splinePosition, 0.25f);
                _cardsInHand[i].CardTransform.DOLocalRotateQuaternion(rotation, 0.25f);
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
    }
}