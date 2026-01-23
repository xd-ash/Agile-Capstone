using UnityEngine.Splines;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CardSystem
{
    public class CardSplineManager : MonoBehaviour
    {
        private SplineContainer _splineContainer;

        private Dictionary<Transform, Sequence> _activeSequences = new Dictionary<Transform, Sequence>();
        [SerializeField] private float _tweenDuration = 0.25f;
        [SerializeField] private int _cardSortingOrderBaseValue = 5;

        public int GetCardSortingOrderBaseValue => _cardSortingOrderBaseValue;

        public static CardSplineManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);

            _splineContainer = FindFirstObjectByType<SplineContainer>();
        }

        public void ArrangeCardGOs()
        {
            int handSize = DeckAndHandManager.instance.GetCurrentHandSize;
            var cardsInHand = DeckAndHandManager.instance.CardsInHand;

            if (handSize == 0) return;
            if (_splineContainer.Spline == null)
            {
                Debug.LogWarning("CardManager: SplineContainer or Spline is not assigned.");
                return;
            }

            int count = Mathf.Min(handSize, cardsInHand.Count);
            int slots = Mathf.Max(1, count);

            float cardSpacing = 1f / slots;

            // Center the group along the spline range [0,1]
            float firstCardPos = 0.5f - (count - 1) * (cardSpacing / 2f);
            firstCardPos = Mathf.Clamp01(firstCardPos);

            for (int i = 0; i < count; i++)
            {
                float t = firstCardPos + i * cardSpacing;
                t = Mathf.Clamp01(t);

                Vector3 splinePosition = _splineContainer.Spline.EvaluatePosition(t);
                Vector3 forward = _splineContainer.Spline.EvaluateTangent(t);
                Vector3 up = _splineContainer.Spline.EvaluateUpVector(t);
                Quaternion rotation = Quaternion.LookRotation(up, Vector3.Cross(up, forward).normalized);

                var tr = cardsInHand[i]?.GetCardTransform;
                var cs = tr?.GetComponent<CardSelect>();
                if (tr != null)
                    UpdateTransformWithTween(tr, splinePosition, rotation, false);
                cs?.UpdateSortingOrders();
            }
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

        public void UpdateCardHoverPosition(Card card, bool isHovered)
        {
            int handSize = DeckAndHandManager.instance.GetCurrentHandSize;
            var cardsInHand = DeckAndHandManager.instance.CardsInHand;
            var spline = _splineContainer?.Spline;

            if (card == null || spline == null) return;

            int cardIndex = cardsInHand.IndexOf(card);
            if (cardIndex == -1) return;

            int count = Mathf.Min(handSize, cardsInHand.Count);
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
                splinePosition += Vector3.up * 0.5f;

            var tr = card.GetCardTransform;
            if (tr != null)
                UpdateTransformWithTween(tr, splinePosition, rotation, isHovered);
        }

        public void RemoveSelectedCard(Card selectedCard)
        {
            if (selectedCard?.GetCardTransform != null)
                Destroy(selectedCard.GetCardTransform.gameObject);

            ArrangeCardGOs();
        }

        /*public Sequence GetActiveTweenSequence(Transform transform)
        {
            if (_activeSequences.TryGetValue(transform, out Sequence sequence))
                return sequence;
            return null;
        }*/
    }
}