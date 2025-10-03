using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace CardSystem
{
    public class CardManager : MonoBehaviour
    {
        public int _topCardOfDeck = 0;

        public int _nextCardInHandIndex = 0;

        public int _maxCards = 5;

        public int _currentHandSize = 0;

        public DeckSO _testDeck;

        public List<CardBase> _cardsInHand = new();//make visible in inspector in some way

        [SerializeField] private SplineContainer splineContainer;

        [SerializeField] public Transform spawnPoint;

        public CardCreator DetermineCardCreator(CardSO cardSO)
        {
            switch (cardSO.GetCardTypeID()[0])
            {
                case '1':
                    return new MiscCardCreator(cardSO);
                case '2':
                    return new RangeCardCreator(cardSO);
                case '3':
                    return new MeleeCardCreator(cardSO);
                default:
                    throw new System.NotImplementedException();
            }
        }
        public void DrawCard()
        {
            //Debug.Log("test");
            _cardsInHand.Add(DetermineCardCreator(_testDeck._deck[_topCardOfDeck]).CreateCard(transform));           
            _topCardOfDeck++;
            _nextCardInHandIndex++;
            _currentHandSize++; 

            ArrangeCardGOs();
        }
        public void UseCard()
        {
            _cardsInHand[0].Use();
        }
        public void ArrangeCardGOs()
        {
            if (_currentHandSize == 0) return;
            float cardSpacing = 1f / (_maxCards);
            float firstCardPos = 0.5f - (_currentHandSize - 1) * (cardSpacing / 2);
            Spline spline = splineContainer.Spline;
            for (int i = 0; i < _currentHandSize; i++)
            {
                float t = firstCardPos + i * cardSpacing;
                Vector3 splinePosition = spline.EvaluatePosition(t);
                Vector3 forward = spline.EvaluateTangent(t);
                Vector3 up = spline.EvaluateUpVector(t);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                _cardsInHand[i]._cardTransform.DOMove(splinePosition, 0.25f);
                _cardsInHand[i]._cardTransform.DOLocalRotateQuaternion(rotation, 0.25f);
            }
        }
    }
}