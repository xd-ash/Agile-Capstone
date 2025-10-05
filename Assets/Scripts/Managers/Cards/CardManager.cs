using AStarPathfinding;
using DG.Tweening;
using OldCardSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
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
            {
                instance = this;
            }
            else
            {
                Debug.LogError($"{this.gameObject.name} has been destroyed due to singleton conflict");
                Destroy(this.gameObject);
            }
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
        public void RemoveCard()
        {
            _cardsInHand.Remove(selectedCard);
            _currentHandSize--;
            ArrangeCardGOs();
            Destroy(selectedCard.CardTransform.gameObject); 
            selectedCard = null;
        }
        public void CreateCardPrefab(Card card)
        {
            GameObject cardGO = Instantiate(card.GetCardPrefab, transform);
            if(!cardGO.GetComponent<CardSelect>()) cardGO.AddComponent<CardSelect>();
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
    }
}