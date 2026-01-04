using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;

namespace CardSystem
{
    public class CardSelect : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private GameObject _cardHighlight;
        private SpriteRenderer _highlightRenderer;
        private CardFunctionScript _cfs;

        [Header("Visual Settings")]
        [SerializeField] private float _handAreaHeight = 2f; // Height of the hand area (add gizmo for editing?)
        private bool _isAboveHandArea = false;

        [Header("Visual Feedback")]
        [SerializeField] private float _hoverScaleMultiplier = 1.2f;
        [SerializeField] private float _dragScaleMultiplier = 1.4f;
        [SerializeField] private float _tweenDuration = 0.2f;
        [SerializeField] private float _rotationAmount = 5f;
        [SerializeField] private Color _validDropColor = Color.green;
        private Color _originalColor;
        private Vector3 _originalScale;

        // Drag state
        //private bool _isDragging = false;
        private Vector3 _dragOffset;
        private Vector3 _startPosition;
        private int _startIndex;

        private void OnEnable()
        {
            _cfs = GetComponent<CardFunctionScript>();

            SetupVisuals();
            //AbilityEvents.OnAbilityTargetingStopped += ClearSelection;
            AbilityEvents.OnAbilityTargetingStopped += ReturnCardToHand;
            if (TurnManager.instance != null)
                TurnManager.instance.OnPlayerTurnEnd += ReturnCardToHand;
        }

        private void OnDestroy()
        {
            //AbilityEvents.OnAbilityTargetingStopped -= ClearSelection;
            AbilityEvents.OnAbilityTargetingStopped -= ReturnCardToHand;
            if (TurnManager.instance != null)
                TurnManager.instance.OnPlayerTurnEnd -= ReturnCardToHand;
        }

        private void Start()
        {
            _originalScale = transform.localScale;
            _originalColor = _spriteRenderer.color;
        }

        private void SetupVisuals()
        {
            _cardHighlight = transform.Find("CardHighlight")?.gameObject;
            _cardHighlight?.SetActive(false);

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _highlightRenderer = _cardHighlight?.GetComponent<SpriteRenderer>();

            if (_highlightRenderer != null)
                _highlightRenderer.sortingOrder = _spriteRenderer.sortingOrder + 1;

            _startIndex = CardManager.instance.CardsInHand.IndexOf(_cfs.Card);

            UpdateSortingOrders();
        }

        private void OnMouseEnter()
        {
            if (!_cfs.IsSelected && !PauseMenu.isPaused && !_cfs.IsDragging && !CardManager.instance.IsCardDragging && CardManager.instance.SelectedCard == null)
                ToggleHighlightAndScale(true);
        }
        private void OnMouseExit()
        {
            if (!_cfs.IsSelected && !PauseMenu.isPaused && !_cfs.IsDragging && !CardManager.instance.IsCardDragging)
                ToggleHighlightAndScale(false);
        }
        private void OnMouseDown()
        {
            // Check for active cards
            if (PauseMenu.isPaused || _cfs.IsSelected || CardManager.instance == null || CardManager.instance.SelectedCard != null) return;

            _isAboveHandArea = false;

            _startPosition = transform.position;
            _startIndex = CardManager.instance.CardsInHand.IndexOf(_cfs.Card);
            if (_startIndex == -1) return;

            _dragOffset = transform.position - MouseFunctionManager.instance.GetMouseWorldPosition();

            // Stop any active animations
            transform.DOKill();

            // Visual feedback for picking up
            transform.DOScale(_originalScale * _dragScaleMultiplier, _tweenDuration);
            transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-_rotationAmount, _rotationAmount)), _tweenDuration);

            ToggleHighlightAndScale(true);
            //BringToFront();
        }
        private void OnMouseUp()
        {
            if (!_cfs.IsDragging) return;

            if (CardManager.instance == null)
            {
                ReturnCardToHand();
                return;
            }

            // If card is dropped above hand area, try to activate it
            if (_isAboveHandArea)
            {
                StartCoroutine(MoveCardToActivePos());
                if(!_cfs.TryActivateCard())
                    ReturnCardToHand();
            }
            else
            {
                // Kill any active tweens
                transform.DOKill();
                _spriteRenderer.DOKill();

                // Reset visual feedback
                transform.DOScale(_originalScale, _tweenDuration);
                transform.DORotate(Vector3.zero, _tweenDuration);
                _spriteRenderer.DOColor(_originalColor, _tweenDuration);
                UpdateCardOrder(true);
            }
        }
        private void OnMouseDrag()
        {
            if (!_cfs.IsDragging || PauseMenu.isPaused || CardManager.instance.SelectedCard != null ||
                CardShopManager.Instance != null || CardManager.instance == null)
                return;

            transform.position = MouseFunctionManager.instance.GetMouseWorldPosition() + _dragOffset;

            // Track when card crosses the threshold
            bool wasAboveHand = _isAboveHandArea;
            _isAboveHandArea = transform.position.y > _handAreaHeight;

            // Only trigger changes when crossing the threshold
            if (wasAboveHand != _isAboveHandArea)
            {
                if (_isAboveHandArea)
                {
                    _spriteRenderer.DOColor(_validDropColor, _tweenDuration).SetUpdate(true);
                    // Temporarily remove from hand management
                    CardManager.instance.RemoveCard(_cfs.Card);
                }
                else
                {
                    _spriteRenderer.DOColor(_originalColor, _tweenDuration).SetUpdate(true);
                    // Add back to hand management
                    CardManager.instance.InsertCard(_cfs.Card);
                }
            }

            // Only update order when in hand area
            if (!_isAboveHandArea)
                UpdateCardOrder(true);
        }

        //temp? card lerp to "active position" to fix cards covering playing grid
        private IEnumerator MoveCardToActivePos()
        {
            Transform target = CardManager.instance.CardActivePos;
            Vector3 initCardPos = transform.localPosition;

            // magic number of 0.2f hard coded in b/c this is a placeholder animation/effect
            for (float timer = 0; timer < 0.2f; timer += Time.deltaTime)
            {
                float lerpRatio = timer / 0.2f;
                transform.localPosition = Vector3.Lerp(initCardPos, target.transform.localPosition, lerpRatio);
                yield return null;
            }

            transform.localPosition = target.localPosition;
        }
        private void ToggleHighlightAndScale(bool isHoveredOrSelected)
        {
            _cardHighlight?.SetActive(isHoveredOrSelected);
            transform.DOScale(_originalScale * (isHoveredOrSelected ? _hoverScaleMultiplier : 1), _tweenDuration);
            UpdateSortingOrders(isHoveredOrSelected ? CardManager.instance.CardsInHand.Count : 0);

            // Only update position if explicitly not dragging
            if (!_cfs.IsDragging)
                CardSplineManager.instance.UpdateCardPosition(_cfs.Card, true);
        }
        private void UpdateCardOrder(bool isHovered, bool isFinal = false)
        {
            if (!isFinal && transform.position.y > _handAreaHeight) return;

            // If we're above hand area, don't calculate new index
            int newIndex = transform.position.y > _handAreaHeight ? _startIndex : CardManager.instance.CalculateCardIndex(_cfs.Card);

            CardManager.instance.ReorderCard(_cfs.Card, newIndex);
            _startIndex = newIndex == -1 ? CardManager.instance.CardsInHand.Count - 1 : newIndex;

            if (!isFinal) return;
            
            CardSplineManager.instance.UpdateCardPosition(_cfs.Card, isHovered);
            UpdateSortingOrders(isHovered ? CardManager.instance.CardsInHand.Count : 0);
            //RestoreOrder();
        }
        /*
        private void RestoreOrder()
        {
            UpdateSortingOrders();

            // Only update position if explicitly not dragging
            if (!_cfs.IsDragging)
                CardSplineManager.instance?.UpdateCardPosition(_cfs.Card, false);
        }*/
        // set sorting order of sprites/texts based on card index
        public void UpdateSortingOrders(int sortingBoost = 0)
        {
            if (_spriteRenderer == null) return;
            if (_cfs.IsDragging) sortingBoost = CardManager.instance.CardsInHand.Count;
            int baseSortingValue = CardSplineManager.instance.GetCardSortingOrderBaseValue;
            bool isShopActive = CardShopManager.Instance != null;

            _spriteRenderer.sortingOrder = baseSortingValue + sortingBoost + (isShopActive ? 0 : _startIndex);
            if (_highlightRenderer != null)
                _highlightRenderer.sortingOrder = baseSortingValue + sortingBoost + (isShopActive ? 0 : _startIndex);

            // Update all TextMeshPro components sorting order
            foreach (var text in GetComponentsInChildren<TextMeshPro>())
                if (text != null)
                    text.sortingOrder = baseSortingValue + sortingBoost + (isShopActive ? 0 : _startIndex);
        }
        public void ReturnCardToHand()
        {
            StopAllCoroutines();
            ClearSelection();

            _isAboveHandArea = false;

            // Kill any active tweens
            transform.DOKill();
            _spriteRenderer.DOKill();
             
            CardManager.instance?.ClearSelection();
            UpdateCardOrder(false, true);
        }
        private void ClearSelection()
        {
            if (TurnManager.instance.currTurn == TurnManager.Turn.Enemy) return;

            _cfs.ClearSelection();
            _spriteRenderer.color = Color.white;
            _cardHighlight?.SetActive(false);
            UpdateCardOrder(false, true);
        }
        public void OnPrefabCreation(Card card)
        {
            if (card == null)
            {
                Debug.LogError("OnPrefabCreation: Card parameter is null");
                return;
            }

            _cfs.OnPrefabCreation(card);

            // Get all TextMeshPro components (non-UI version)
            TextMeshPro[] cardTextFields = GetComponentsInChildren<TextMeshPro>();

            if (cardTextFields.Length >= 3)
            {
                // Update text content
                cardTextFields[0].text = card.GetCardName;
                cardTextFields[1].text = card.GetDescription;
                cardTextFields[2].text = card.GetCardAbility.GetApCost.ToString();

                // Make sure text components are properly attached and sorted
                foreach (var textField in cardTextFields)
                {
                    // Ensure text is child of card and follows its transform
                    textField.transform.SetParent(transform, true);

                    // Set up sorting
                    textField.sortingOrder = _spriteRenderer != null ?
                        _spriteRenderer.sortingOrder + 1 : 1;
                }
            }
            else
            {
                Debug.LogError("Card prefab is missing required TextMeshPro components");
            }

            card.CardTransform = transform;
            SetupVisuals();
        }
    }
}