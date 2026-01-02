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
        private Card _card;

        [SerializeField] private bool _selected = false;

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
        private bool _isDragging = false;
        private Vector3 _dragOffset;
        private Vector3 _startPosition;
        private int _startIndex;

        // field to track if any card is currently being used
        [SerializeField] private bool _isAnyCardActive = false;

        private void OnEnable()
        {
            SetupVisuals();
            AbilityEvents.OnAbilityTargetingStopped += ClearSelection;
            AbilityEvents.OnAbilityTargetingStarted += OnTargetingStarted; // Use the correct event
            if (TurnManager.instance != null)
                TurnManager.instance.OnPlayerTurnEnd += ReturnCardToHand;
        }

        private void OnDestroy()
        {
            AbilityEvents.OnAbilityTargetingStopped -= ClearSelection;
            AbilityEvents.OnAbilityTargetingStarted -= OnTargetingStarted; // Use the correct event   
            if (TurnManager.instance != null)
                TurnManager.instance.OnPlayerTurnEnd -= ReturnCardToHand;
        }

        private void Start()
        {
            _originalScale = transform.localScale;
            _originalColor = _spriteRenderer.color;
        }

        // Add handler method
        private void OnTargetingStarted()
        {
            _isAnyCardActive = true;
        }

        private void SetupVisuals()
        {
            _cardHighlight = transform.Find("CardHighlight")?.gameObject;
            _cardHighlight?.SetActive(false);

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _highlightRenderer = _cardHighlight?.GetComponent<SpriteRenderer>();

            if (_highlightRenderer != null)
                _highlightRenderer.sortingOrder = _spriteRenderer.sortingOrder + 1;

            UpdateSortingOrders();
        }

        #region OnMouse Methods
        private void OnMouseEnter()
        {
            if (!_selected && !PauseMenu.isPaused && !_isDragging && !_isAnyCardActive)
            {
                _cardHighlight?.SetActive(true);
                transform.DOScale(_originalScale * _hoverScaleMultiplier, _tweenDuration);
                BringToFront();
            }
        }

        private void OnMouseExit()
        {
            if (!_selected && !PauseMenu.isPaused && !_isDragging)
            {
                _cardHighlight?.SetActive(false);
                transform.DOScale(_originalScale, _tweenDuration);
                RestoreOrder();
            }
        }

        private void OnMouseDown()
        {
            if (PauseMenu.isPaused || _selected) return;

            bool isShopActive = CardShopManager.Instance != null;

            // Shop-mode: show confirmation popup instead of drag/drop purchase
            if (isShopActive)
            {
                //Debug.Log($"Attempting to purchase shop item: {_card?.GetCardName} for {_shopCost} currency");
                int price = _card.ShopCost;
                string cardName = _card?.GetCardName ?? "Card";

                Action confirmAction = () =>
                {
                    //Debug.Log($"Confirmed purchase of {cardName} for {price} currency");
                    if (CurrencyManager.instance != null && CurrencyManager.instance.TrySpend(price))
                    {
                        // add card to runtime deck (this method also adds it to player card collection)
                        CardManager.instance?.AddDefinitionToRuntimeDeck(_card.GetCardAbility);

                        if (isShopActive)
                            CardShopManager.Instance?.DeleteCard(gameObject);
                        else
                            Destroy(gameObject);
                    }
                    else
                        OutOfApPopup.Instance?.Show();
                };

                Action cancelAction = () =>
                {
                    // no-op; popup will just close
                };

                ShopConfirmPopup.Instance?.Show(price, cardName, confirmAction, cancelAction);

                if (ShopConfirmPopup.Instance == null)
                {
                    Debug.LogWarning("Shop confirm popup is null. Fallback confirm action called.");
                    confirmAction();
                }
            }

            // Check for active cards
            if (PauseMenu.isPaused || _selected || CardManager.instance == null || _isAnyCardActive) return;

            _startPosition = transform.position;
            _startIndex = CardManager.instance.CardsInHand.IndexOf(_card);
            if (_startIndex == -1) return;

            _isDragging = true;
            _dragOffset = transform.position - MouseFunctionManager.instance.GetMouseWorldPosition();

            // Stop any active animations
            transform.DOKill();

            // Visual feedback for picking up
            transform.DOScale(_originalScale * _dragScaleMultiplier, _tweenDuration);
            transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-_rotationAmount, _rotationAmount)), _tweenDuration);

            BringToFront();
        }

        private void OnMouseUp()
        {
            if (!_isDragging) return;
            _isDragging = false;

            if (CardManager.instance == null)
            {
                //ResetPosition();
                ReturnCardToHand();
                return;
            }

            // If card is dropped above hand area, try to activate it
            if (_isAboveHandArea)
            {
                StartCoroutine(MoveCardToActivePos());
                TryActivateCard();
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
                //FinalizeCardOrder();
                UpdateCardOrder(true);
            }

            _isAboveHandArea = false;
        }
        private void OnMouseDrag()
        {
            if (!_isDragging || PauseMenu.isPaused || _isAnyCardActive ||
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
                    CardManager.instance.CardsInHand.Remove(_card);
                }
                else
                {
                    _spriteRenderer.DOColor(_originalColor, _tweenDuration).SetUpdate(true);
                    // Add back to hand management
                    CardManager.instance.CardsInHand.Insert(CalculateCardIndex(), _card); 
                }
            }

            // Only update order when in hand area
            if (!_isAboveHandArea)
                UpdateCardOrder();
        }
        #endregion

        private void TryActivateCard()
        {
            if (_card == null || _card.GetCardAbility?.RootNode == null || CardManager.instance == null || _isAnyCardActive)
            {
                ReturnCardToHand();
                return;
            }

            var currentUnit = TurnManager.GetCurrentUnit;
            int cost = _card.GetCardAbility.GetApCost;

            if (currentUnit == null || !currentUnit.CanSpend(cost))
            {
                OutOfApPopup.Instance?.Show();
                ReturnCardToHand();
                return;
            }

            // Only remove from hand if we can actually use the card
            _selected = true;
            CardManager.instance.SelectCard(_card);
            CardSplineManager.instance.ArrangeCardGOs();

            // First invoke targeting started to set up restrictions
            AbilityEvents.TargetingStarted();
            _card.GetCardAbility.UseAility(currentUnit);
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

        private void ClearSelection()
        {
            if (TurnManager.instance.currTurn == TurnManager.Turn.Enemy) return;

            _selected = false;
            _isAnyCardActive = false; // Reset when ability is finished
            _spriteRenderer.color = Color.white;
            _cardHighlight?.SetActive(false);
            RestoreOrder();
        }

        #region Card Sorting/Ordering
        private void BringToFront()
        {
            UpdateSortingOrders(CardManager.instance.CardsInHand.Count);

            // Only update position if explicitly not dragging
            if (!_isDragging)
                CardSplineManager.instance.UpdateCardPosition(_card, true);
        }
        private void UpdateCardOrder(bool isFinal = false)
        {
            if (!isFinal && transform.position.y > _handAreaHeight) return;

            int newIndex = CalculateCardIndex();
            if (newIndex != -1 && newIndex != _startIndex)
            {
                CardManager.instance.ReorderCard(_card, newIndex);
                _startIndex = newIndex;
                if (!isFinal) return;
            }

            CardSplineManager.instance.UpdateCardPosition(_card, false);
            RestoreOrder();
        }
        /*merged these into update card order above (delete later if no bugs)
        private void UpdateCardOrder()
        {
            // Only calculate new index if card is in hand area
            if (transform.position.y <= _handAreaHeight)
            {
                int newIndex = CalculateCardIndex();
                if (newIndex != -1 && newIndex != _startIndex)
                {
                    CardManager.instance.PreviewReorder(_startIndex, newIndex);
                    _startIndex = newIndex;
                }
            }
        }
        private void FinalizeCardOrder()
        {
            int finalIndex = CalculateCardIndex();
            if (finalIndex != -1 && finalIndex != _startIndex)
                CardManager.instance.ReorderCard(_card, finalIndex);
            else
                CardSplineManager.instance.UpdateCardPosition(_card, false);
            RestoreOrder();
        }*/
        private void RestoreOrder()
        {
            UpdateSortingOrders();
            
            // Only update position if explicitly not dragging
            if (!_isDragging)
                CardSplineManager.instance?.UpdateCardPosition(_card, false);
        }

        // set sorting order of sprites/texts based on card index
        private void UpdateSortingOrders(int sortingBoost = 0)
        {
            Debug.Log("test");

            if (_spriteRenderer == null) return;
            int index = CardManager.instance.CardsInHand.IndexOf(_card);
            bool isShopActive = CardShopManager.Instance != null;

            _spriteRenderer.sortingOrder = sortingBoost + (isShopActive ? 0 : index);
            if (_highlightRenderer != null)
                _highlightRenderer.sortingOrder = sortingBoost + (isShopActive ? 0 : index);

            // Update all TextMeshPro components sorting order
            foreach (var text in GetComponentsInChildren<TextMeshPro>())
                if (text != null)
                    text.sortingOrder = sortingBoost + (isShopActive ? 0 : index);
        }

        public void ReturnCardToHand()
        {
            StopAllCoroutines();
            ClearSelection();

            // Kill any active tweens
            transform.DOKill();
            _spriteRenderer.DOKill();

            // Destroy the card object
            if (CardManager.instance != null)
            {
                CardManager.instance.CardsInHand.Add(_card);
                //CardSplineManager.instance.ArrangeCardGOs();
                RestoreOrder();
            }
        }

        public void ResetPosition()
        {
            if (CardManager.instance != null)
                CardSplineManager.instance.UpdateCardPosition(_card, false);
            else
                transform.position = _startPosition;

            RestoreOrder();
        }

        #endregion

        #region Misc Methods and Calculations
        private int CalculateCardIndex()
        {
            // If we're above hand area, don't calculate new index
            if (transform.position.y > _handAreaHeight)
                return _startIndex; // Return original index to prevent reordering

            var cards = CardManager.instance?.CardsInHand;
            if (cards == null || cards.Count <= 1) return -1;

            float myX = transform.position.x;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == _card || cards[i]?.CardTransform == null) continue;
                if (cards[i].CardTransform.position.x > myX) return i;
            }
            return cards.Count - 1;
        }

        /// <summary>
        /// Initializes the CardSelect component with the given Card data.
        /// Called when a new card prefab is created.
        /// </summary>
        /// <param name="card">The Card instance to associate with this CardSelect component</param>
        public void OnPrefabCreation(Card card)
        {
            if (card == null)
            {
                Debug.LogError("OnPrefabCreation: Card parameter is null");
                return;
            }

            _card = card;
            _card.CardTransform = transform;

            transform.name = card.GetCardName;

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
        /// <summary>
        /// Enable shop-mode for this CardSelect (merged ShopCard functionality).
        /// Call this from the spawner right after OnPrefabCreation when creating shop items.
        /// </summary>
        public void EnableShopMode()
        {
            //_isShopItem = true;
            int cost = Mathf.Max(0, _card.ShopCost);

            // If the prefab has a cost display (third TextMeshPro), update it.
            TextMeshPro[] cardTextFields = GetComponentsInChildren<TextMeshPro>();
            if (cardTextFields.Length >= 3)
                cardTextFields[2].text = cost.ToString();
        }
        /* Currently unused method to update prefab text
        private void UpdateCardText()
        {
            if (_card == null) return;

            // Update card name text
            if (_nameText != null)
            {
                _nameText.text = _card.GetCardName;
                UpdateSortingOrders(); // Ensure proper text layering
            }

            // Update description text
            if (_descriptionText != null)
            {
                _descriptionText.text = _card.GetDescription;
                UpdateSortingOrders(); // Ensure proper text layering
            }
        }*/
        #endregion
    }
}