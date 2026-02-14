using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

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
        private Vector3 _dragOffset;
        private Vector3 _startPosition;
        private int _startIndex;

        private void OnEnable()
        {
            _cfs = GetComponent<CardFunctionScript>();

            SetupVisuals();
            AbilityEvents.OnAbilityTargetingStopped += ReturnCardToHand;
            if (TurnManager.Instance != null)
                TurnManager.Instance.OnPlayerTurnEnd += ReturnCardToHand;
        }

        private void OnDestroy()
        {
            AbilityEvents.OnAbilityTargetingStopped -= ReturnCardToHand;
            if (TurnManager.Instance != null)
                TurnManager.Instance.OnPlayerTurnEnd -= ReturnCardToHand;
        }

        private void Start()
        {
            _originalScale = transform.localScale;
            _originalColor = _spriteRenderer.color;
        }

        private void OnMouseEnter()
        {
            if (!_cfs.IsSelected && !PauseMenu.isPaused && !_cfs.IsDragging && DeckAndHandManager.Instance.GetSelectedCard == null)
                ToggleHighlightAndScale(true);
        }
        private void OnMouseExit()
        {
            if (!_cfs.IsSelected && !PauseMenu.isPaused && !_cfs.IsDragging && DeckAndHandManager.Instance.GetSelectedCard == null)
                ToggleHighlightAndScale(false);
        }
        private void OnMouseDown()
        {
            // Check for active cards
            if (PauseMenu.isPaused || _cfs.IsSelected || DeckAndHandManager.Instance == null || DeckAndHandManager.Instance.GetSelectedCard != null || TurnManager.IsEnemyTurn) return;

            _isAboveHandArea = false;

            _startPosition = transform.position;
            _startIndex = DeckAndHandManager.Instance.CardsInHand.IndexOf(_cfs.Card);
            if (_startIndex == -1) return;

            _dragOffset = transform.position - MouseFunctionManager.Instance.GetMouseWorldPosition();

            // Stop any active animations
            transform.DOKill();

            // Visual feedback for picking up
            transform.DOScale(_originalScale * _dragScaleMultiplier, _tweenDuration);
            transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-_rotationAmount, _rotationAmount)), _tweenDuration);

            ToggleHighlightAndScale(true);
        }
        private void OnMouseUp()
        {
            if (!_cfs.IsDragging) return;

            if (DeckAndHandManager.Instance == null)
            {
                ReturnCardToHand();
                return;
            }

            // If card is dropped above hand area, try to activate it
            if (_isAboveHandArea)
            {
                StartCoroutine(MoveCardToActivePos());

                if (_cfs.TryActivateCard()) return;
            }

            ReturnCardToHand();
        }
        private void OnMouseDrag()
        {
            if (!_cfs.IsDragging || PauseMenu.isPaused || CardShopManager.Instance != null || DeckAndHandManager.Instance == null)
                return;

            // Temporarily remove from hand management
            DeckAndHandManager.Instance.RemoveCard(_cfs.Card);
            DeckAndHandManager.Instance.SelectCard(_cfs.Card);

            transform.position = MouseFunctionManager.Instance.GetMouseWorldPosition() + _dragOffset;

            // Track when card crosses the threshold
            bool wasAboveHand = _isAboveHandArea;
            _isAboveHandArea = transform.position.y > _handAreaHeight;
            Color spriteColor = _isAboveHandArea ? _validDropColor : _originalColor;

            // Only trigger changes when crossing the threshold
            if (wasAboveHand != _isAboveHandArea)
                _spriteRenderer.DOColor(spriteColor, _tweenDuration).SetUpdate(true);

            // Only update order when in hand area
            if (!_isAboveHandArea)
                UpdateCardPrefabOrder(true);
        }

        //temp? card lerp to "active position" to fix cards covering playing grid
        private IEnumerator MoveCardToActivePos()
        {
            Transform target = DeckAndHandManager.Instance.CardActivePos;
            Vector3 initCardPos = transform.localPosition;

            //Lerp duration uses tween duration
            for (float timer = 0f; timer < _tweenDuration; timer += Time.deltaTime)
            {
                float lerpRatio = timer / _tweenDuration;
                transform.localPosition = Vector3.Lerp(initCardPos, target.transform.localPosition, lerpRatio);
                yield return null;
            }

            transform.localPosition = target.localPosition;
        }

        private void ToggleHighlightAndScale(bool isHoveredOrSelected)
        {
            _cardHighlight?.SetActive(isHoveredOrSelected);

            float scaleMultiplier = isHoveredOrSelected ? _hoverScaleMultiplier : 1;
            transform.DOScale(_originalScale * scaleMultiplier, _tweenDuration);

            UpdateSortingOrders(isHoveredOrSelected ? DeckAndHandManager.Instance.CardsInHand.Count : 0);

            // Only update position if explicitly not dragging
            if (!_cfs.IsDragging)
                CardSplineManager.Instance?.UpdateCardHoverPosition(_cfs.Card, isHoveredOrSelected);
        }

        //Calculate new index of card, then start card hand reorder and sorting orders of sprites/texts
        private void UpdateCardPrefabOrder(bool isHovered, bool isFinal = false)
        {
            // If we're above hand area, don't calculate new index
            int newIndex = isFinal ? DeckAndHandManager.Instance.CalculateCardIndex(_cfs.Card) : _startIndex;

            //Reorder card in hand & deck manager (currently not working OnDrag due
            //to the card being removed from hand on drag)  
            DeckAndHandManager.Instance.ReorderCard(_cfs.Card, newIndex);

            _startIndex = newIndex == -1 ? DeckAndHandManager.Instance.CardsInHand.Count - 1 : newIndex;

            if (!isFinal) return;
            
            CardSplineManager.Instance?.UpdateCardHoverPosition(_cfs.Card, isHovered);
            UpdateSortingOrders(isHovered ? DeckAndHandManager.Instance.CardsInHand.Count : 0);
        }

        // set sorting order of sprites/texts based on card index
        public void UpdateSortingOrders(int sortingBoost = 0)
        {
            if (_spriteRenderer == null) return;
            if (_cfs.IsDragging) sortingBoost = DeckAndHandManager.Instance.CardsInHand.Count;

            bool isShopActive = CardShopManager.Instance != null;
            int baseSortingValue = isShopActive ? 0 : CardSplineManager.Instance.GetCardSortingOrderBaseValue;
            int cardIndex = isShopActive ? 0 : DeckAndHandManager.Instance.CardsInHand.IndexOf(_cfs.Card);
             
            // Set sorting by taking into account the BaseSortingValue, sorting boost param, and card index (index set to 0 during shop scene)
            _spriteRenderer.sortingOrder = baseSortingValue + sortingBoost + cardIndex;
            
            if (_highlightRenderer != null)
                _highlightRenderer.sortingOrder = baseSortingValue + sortingBoost + cardIndex;

            // Update all TextMeshPro components sorting order
            foreach (var text in GetComponentsInChildren<TextMeshPro>())
                if (text != null)
                    text.sortingOrder = baseSortingValue + sortingBoost + cardIndex;
        }

        // Return card to hand, clear selection, stop coroutines and tweens, then update card orders
        public void ReturnCardToHand()
        {
            if (DeckAndHandManager.Instance.GetSelectedCard != _cfs.Card) return;

            StopAllCoroutines();
            ClearSelection();

            _isAboveHandArea = false;

            // Kill any active tweens
            transform.DOKill();
            _spriteRenderer.DOKill();

            ToggleHighlightAndScale(false);
            DeckAndHandManager.Instance?.ClearSelection();
            UpdateCardPrefabOrder(false, true);
        }

        private void ClearSelection()
        {
            if (TurnManager.Instance.CurrTurn == TurnManager.Turn.Enemy) return;

            _cfs.ClearSelection(_tweenDuration);
            _spriteRenderer.color = Color.white;
            _cardHighlight?.SetActive(false);
        }

        //Set initial text fields and initialize card object
        public void OnPrefabCreation(Card card)
        {
            if (card == null)
            {
                Debug.LogError("OnPrefabCreation: Card parameter is null");
                return;
            }

            _cfs.OnPrefabCreation(card);
            /*
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
                Debug.LogError("Card prefab is missing required TextMeshPro components");
            */
            SetupVisuals();
        }

        // Initial prefeab GameObjects & sprite renderer grabbing, index calcs, and initial sorting order update
        private void SetupVisuals()
        {
            _cardHighlight = transform.Find("CardHighlight")?.gameObject;
            _cardHighlight?.SetActive(false);

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _highlightRenderer = _cardHighlight?.GetComponent<SpriteRenderer>();

            if (_highlightRenderer != null)
                _highlightRenderer.sortingOrder = _spriteRenderer.sortingOrder + 1;

            _startIndex = DeckAndHandManager.Instance.CardsInHand.IndexOf(_cfs.Card);

            UpdateSortingOrders();
        }
    }
}