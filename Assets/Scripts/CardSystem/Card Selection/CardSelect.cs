using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace CardSystem
{
    public class CardSelect : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private GameObject _cardHighlight;
        private SpriteRenderer _highlightRenderer;
        private Card _card;
        
        [SerializeField] private Sprite _cardSprite;
        [SerializeField] private bool selected = false;

        [Header("Visual Settings")]
        [SerializeField] private float handAreaHeight = 2f; // Height of the hand area
        [SerializeField] private float activationThreshold = 2.1f; // Just above hand area
        [SerializeField] private int _hoverSortingBoost = 1000;
        
        [Header("Movement Settings")]
        [SerializeField] private float smoothSpeed = 10f;
        [SerializeField] private float destroyThreshold = -3f; // Threshold for destroying cards dropped below hand

        [Header("Visual Feedback")]
        [SerializeField] private float hoverScaleMultiplier = 1.2f;
        [SerializeField] private float dragScaleMultiplier = 1.4f;
        [SerializeField] private float scaleDuration = 0.2f;
        [SerializeField] private float rotationAmount = 5f;
        [SerializeField] private Color validDropColor = Color.green;
        [SerializeField] private Color invalidDropColor = Color.red;
        
        [Header("Card Text Components")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        
        // Drag state
        private bool isDragging = false;
        private Vector3 dragOffset;
        private Vector3 startPosition;
        private int startIndex;
        
        private int _baseSortingOrder;
        private Camera _mainCamera;
        private Color originalColor;
        private Vector3 originalScale;

        private void OnEnable()
        {
            _mainCamera = Camera.main;
            SetupVisuals();
            AbilityEvents.OnAbilityUsed += ClearSelection;
        }

        private void OnDestroy()
        {
            AbilityEvents.OnAbilityUsed -= ClearSelection;
        }

        private void SetupVisuals()
        {
            _cardHighlight = transform.Find("CardHighlight")?.gameObject;
            if (_cardHighlight != null) _cardHighlight.SetActive(false);

            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
            {
                _baseSortingOrder = _spriteRenderer.sortingOrder;
                if (_cardSprite != null) _spriteRenderer.sprite = _cardSprite;
            }

            _highlightRenderer = _cardHighlight?.GetComponent<SpriteRenderer>();
            if (_highlightRenderer != null)
            {
                _highlightRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
                _highlightRenderer.sortingOrder = _baseSortingOrder + 1;
            }
        }

        private void UpdateCardText()
        {
            if (_card == null) return;

            // Update card name text
            if (_nameText != null)
            {
                _nameText.text = _card.GetCardName;
                UpdateSortingOrders(_baseSortingOrder); // Ensure proper text layering
            }

            // Update description text
            if (_descriptionText != null)
            {
                _descriptionText.text = _card.GetDescription;
                UpdateSortingOrders(_baseSortingOrder); // Ensure proper text layering
            }
        }

        private void Start()
        {
            originalScale = transform.localScale;
            originalColor = _spriteRenderer.color;
        }

        private void OnMouseEnter()
        {
            if (!selected && !PauseMenu.isPaused && !isDragging)
            {
                if (_cardHighlight != null) _cardHighlight.SetActive(true);
                transform.DOScale(originalScale * hoverScaleMultiplier, scaleDuration);
                BringToFront();
            }
        }

        private void OnMouseExit()
        {
            if (!selected && !PauseMenu.isPaused && !isDragging)
            {
                if (_cardHighlight != null) _cardHighlight.SetActive(false);
                transform.DOScale(originalScale, scaleDuration);
                RestoreOrder();
            }
        }

        private void OnMouseDown()
        {
            if (PauseMenu.isPaused || selected || CardManager.instance == null) return;

            startPosition = transform.position;
            startIndex = CardManager.instance._cardsInHand.IndexOf(_card);
            if (startIndex == -1) return;

            isDragging = true;
            dragOffset = transform.position - GetMouseWorldPosition();
            
            // Stop any active animations
            transform.DOKill();
            
            // Visual feedback for picking up
            transform.DOScale(originalScale * dragScaleMultiplier, scaleDuration);
            transform.DORotate(new Vector3(0, 0, Random.Range(-rotationAmount, rotationAmount)), scaleDuration);
            
            BringToFront();
        }

        private void OnMouseDrag()
        {
            if (!isDragging || PauseMenu.isPaused || CardManager.instance == null) return;

            // Direct position setting without any constraints
            transform.position = GetMouseWorldPosition() + dragOffset;

            // Visual feedback based on position relative to hand area
            if (transform.position.y > handAreaHeight)
            {
                _spriteRenderer.DOColor(validDropColor, 0.2f);
                // Don't call UpdateCardPosition here to prevent snapping
            }
            else
            {
                _spriteRenderer.DOColor(originalColor, 0.2f);
                UpdateCardOrder(); // Only update order when inside hand area
            }
        }

        private void OnMouseUp()
        {
            if (!isDragging) return;
            isDragging = false;

            Debug.Log($"Card released at height: {transform.position.y}, Hand area height: {handAreaHeight}");

            if (CardManager.instance == null)
            {
                ResetPosition();
                return;
            }

            // If card is dropped above hand area, activate it
            if (transform.position.y > handAreaHeight)
            {
                Debug.Log($"Attempting to activate card {_card?.GetCardName}");
                // Remove card from hand before any visual changes
                CardManager.instance._cardsInHand.Remove(_card);
                CardManager.instance.ArrangeCardGOs();
                
                // Then try to activate
                TryActivateCard();
            }
            else
            {
                // Reset visual feedback
                transform.DOScale(originalScale, scaleDuration);
                transform.DORotate(Vector3.zero, scaleDuration);
                _spriteRenderer.DOColor(originalColor, scaleDuration);
                
                // If within hand area, just return to position
                FinalizeCardPosition();
            }
        }

        private void UpdateCardOrder()
        {
            int newIndex = CalculateCardIndex();
            if (newIndex != -1 && newIndex != startIndex)
            {
                CardManager.instance.PreviewReorder(startIndex, newIndex);
                startIndex = newIndex;
            }
        }

        private void FinalizeCardPosition()
        {
            int finalIndex = CalculateCardIndex();
            if (finalIndex != -1 && finalIndex != startIndex)
            {
                CardManager.instance.ReorderCard(_card, finalIndex);
            }
            else
            {
                CardManager.instance.UpdateCardPosition(_card, false);
            }
            RestoreOrder();
        }

        private int CalculateCardIndex()
        {
            var cards = CardManager.instance?._cardsInHand;
            if (cards == null || cards.Count <= 1) return -1;

            float myX = transform.position.x;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == _card || cards[i]?.CardTransform == null) continue;
                if (cards[i].CardTransform.position.x > myX) return i;
            }
            return cards.Count - 1;
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -_mainCamera.transform.position.z;
            return _mainCamera.ScreenToWorldPoint(mousePos);
        }

        private void BringToFront()
        {
            UpdateSortingOrders(_baseSortingOrder + _hoverSortingBoost);
            if (CardManager.instance != null && !isDragging)
            {
                // Only update position if not being dragged
                CardManager.instance.UpdateCardPosition(_card, true);
            }
        }

        private void RestoreOrder()
        {
            UpdateSortingOrders(_baseSortingOrder);
            if (CardManager.instance != null && !isDragging)
            {
                // Only update position if not being dragged
                CardManager.instance.UpdateCardPosition(_card, false);
            }
        }

        // Update the UpdateSortingOrders method to handle text movement more reliably
        private void UpdateSortingOrders(int baseOrder)
        {
            if (_spriteRenderer == null) return;

            _spriteRenderer.sortingOrder = baseOrder;
            if (_highlightRenderer != null)
            {
                _highlightRenderer.sortingOrder = baseOrder + 1;
            }

            // Update all TextMeshPro components sorting order
            var texts = GetComponentsInChildren<TextMeshPro>();
            foreach (var text in texts)
            {
                if (text == null) continue;
                text.sortingOrder = baseOrder + 2;
            }
        }

        private void ResetPosition()
        {
            if (CardManager.instance != null)
            {
                CardManager.instance.UpdateCardPosition(_card, false);
            }
            else
            {
                transform.position = startPosition;
            }
            RestoreOrder();
        }

        private void TryActivateCard()
        {
            Debug.Log($"TryActivateCard - Starting activation process for {_card?.GetCardName}");
            
            if (_card == null || _card.GetCardAbility?.RootNode == null || CardManager.instance == null)
            {
                Debug.LogError("Card, ability, or root node is null");
                ResetPosition();
                return;
            }

            var currentUnit = TurnManager.GetCurrentUnit;
            int cost = _card.GetCardAbility.RootNode.GetApCost;
            
            if (currentUnit == null || !currentUnit.CanSpend(cost))
            {
                Debug.LogWarning($"Cannot activate card: {(currentUnit == null ? "No unit" : "Not enough AP")}");
                OutOfApPopup.Instance?.Show();
                ResetPosition();
                return;
            }

            // Remove card from hand before activating to prevent spline issues
            CardManager.instance._cardsInHand.Remove(_card);
            CardManager.instance.ArrangeCardGOs(); // Rearrange remaining cards

            selected = true;
            CardManager.instance.selectedCard = _card;
            
            Debug.Log($"Activating ability for card: {_card.GetCardName}");
            AbilityEvents.TargetingStarted();
            _card.GetCardAbility.UseAility(currentUnit);
        }

        private void ClearSelection()
        {
            selected = false;
            if (_cardHighlight != null) _cardHighlight.SetActive(false);
            RestoreOrder();
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
                cardTextFields[2].text = card.GetCardAbility.RootNode.GetApCost.ToString();
                
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