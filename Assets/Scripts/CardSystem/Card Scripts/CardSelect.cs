using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

namespace CardSystem
{
    public class CardSelect : MonoBehaviour
    {
        private Image _cardImage;
        private GameObject _cardHighlight;
        private SpriteRenderer _highlightRenderer;
        private CardFunctionScript _cfs;

        private CardState _state;

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

        private Action _onMouseDown, _onMouseUp, _onMouseDrag;

        private void OnEnable()
        {
            _cfs = GetComponent<CardFunctionScript>();
        }

        private void OnDestroy()
        {
            if (_state != CardState.Combat) return;

            AbilityEvents.OnAbilityTargetingStopped -= ReturnCardToHand;
            if (TurnManager.Instance != null)
                TurnManager.Instance.OnTurnEnd -= OnTurnEnd;
        }

        public void InitCardSelect(CardState state)
        {
            _state = state;

            SetupVisuals();

            _originalScale = transform.localScale;
            _originalColor = _cardImage.color;

            SetOnMouseDown();
            SetOnMouseUp();
            SetOnMouseDrag();

            if (_state != CardState.Combat) return;

            AbilityEvents.OnAbilityTargetingStopped += ReturnCardToHand;
            if (TurnManager.Instance != null)
                TurnManager.Instance.OnTurnEnd += OnTurnEnd;
        }
        //Bandaid fix for sawpping to OnTurnEnd action in turn manager
        private void OnTurnEnd(Unit unit)
        {
            if (unit.GetTeam != Team.Friendly) return;

            ReturnCardToHand();
        }

        private void OnMouseEnter()
        {
            if (!_cfs.IsSelected && !PauseMenu.isPaused && !_cfs.IsDragging && DeckAndHandManager.Instance.GetSelectedCard == null)
                ToggleHighlightAndScale(true);

            if (_state != CardState.Combat) return;
            int cost = _cfs.Card?.GetCardAbility?.GetApCost ?? 0;
            APDisplay.Instance?.ShowPreview(cost);
        }
        private void OnMouseExit()
        {
            if (!_cfs.IsSelected && !PauseMenu.isPaused && !_cfs.IsDragging)
            {
                //if (DeckAndHandManager.Instance != null && DeckAndHandManager.Instance.GetSelectedCard != null) return;

                ToggleHighlightAndScale(false);

                if (_state != CardState.Combat || DeckAndHandManager.Instance != null && DeckAndHandManager.Instance.GetSelectedCard != null) return;

                APDisplay.Instance?.ClearPreview();
            }
        }
        private void OnMouseDown()
        {
            _onMouseDown?.Invoke();
        }
        private void OnMouseUp()
        {
            _onMouseUp?.Invoke();
        }
        private void OnMouseDrag()
        {
            _onMouseDrag?.Invoke();
        }

        //temp? card lerp to "active position" to fix cards covering playing grid
        private IEnumerator MoveCardToActivePos()
        {
            Transform target = DeckAndHandManager.Instance.CardActivePos;
            Vector3 initCardPos = transform.localPosition;
            Quaternion initCardRot = transform.localRotation;

            //Lerp duration uses tween duration
            for (float timer = 0f; timer < _tweenDuration; timer += Time.deltaTime)
            {
                float lerpRatio = timer / _tweenDuration;
                transform.localPosition = Vector3.Lerp(initCardPos, target.transform.localPosition, lerpRatio);
                transform.localRotation = Quaternion.Lerp(initCardRot, target.transform.localRotation, lerpRatio);
                yield return null;
            }

            transform.localPosition = target.localPosition;
        }
        public void ToggleHighlightAndScale(bool isHoveredOrSelected)
        {
            _cardHighlight?.SetActive(isHoveredOrSelected);

            float scaleMultiplier = isHoveredOrSelected ? _hoverScaleMultiplier : 1;
            transform.DOScale(_originalScale * scaleMultiplier, _tweenDuration);

            if (_state != CardState.Combat) return;

            CardPrefabSetterUpper.SetCombatCardGOOrder(isHoveredOrSelected ? transform : null);

            if (!_cfs.IsDragging && !OptionsSettings.IsCardSelectOnClick)
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
            CardPrefabSetterUpper.SetCombatCardGOOrder();
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
            _cardImage.DOKill();

            ToggleHighlightAndScale(false);
            DeckAndHandManager.Instance?.ClearSelection();
            UpdateCardPrefabOrder(false, true);
        }

        private void ClearSelection()
        {
            if (TurnManager.Instance.CurrTurn == TurnManager.Turn.Enemy) return;

            _cfs.ClearSelection(_tweenDuration);
            _cardImage.color = Color.white;
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

            _cfs.OnPrefabCreation(card, _state);

            SetupVisuals();
        }

        // Initial prefeab GameObjects & sprite renderer grabbing, index calcs, and initial sorting order update
        private void SetupVisuals()
        {
            _cardHighlight = transform.Find("CardHighlight")?.gameObject;
            _cardHighlight?.SetActive(false);

            _cardImage = GetComponentInChildren<Image>();

            if (_state != CardState.Combat) return;

            CardPrefabSetterUpper.SetCombatCardGOOrder();

            _startIndex = DeckAndHandManager.Instance.CardsInHand.IndexOf(_cfs.Card);
        }

        private void SetOnMouseDown()
        {
            Action tmp = null;
            switch (_state)
            {
                case CardState.PackViewer:
                    break;
                case CardState.DeckViewer:
                    break;
                case CardState.Shop:
                    break;
                case CardState.Rewards:
                    break;
                case CardState.Combat:
                    tmp = () =>
                    {
                        // Block card interaction if tutorial is active and not on card step
                        if (TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None &&
                            TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.CardsOnly)
                            return;

                        // Check for active cards
                        if (PauseMenu.isPaused || _cfs.IsSelected || DeckAndHandManager.Instance == null || DeckAndHandManager.Instance.GetSelectedCard != null || TurnManager.IsEnemyTurn) return;

                        if (OptionsSettings.IsCardSelectOnClick) return;

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
                    };
                    break;
            }
            _onMouseDown = tmp;
        }
        private void SetOnMouseUp()
        {
            Action tmp = null;
            switch (_state)
            {
                case CardState.PackViewer:
                    break;
                case CardState.DeckViewer:
                    break;
                case CardState.Shop:
                    break;
                case CardState.Rewards:
                    break;
                case CardState.Combat:
                    tmp = () =>
                    {
                        if (TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None &&
                            TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.CardsOnly)
                            return;

                        if (!_cfs.IsDragging && !OptionsSettings.IsCardSelectOnClick || 
                            OptionsSettings.IsCardSelectOnClick && DeckAndHandManager.Instance.GetSelectedCard != null || 
                            CardShopManager.Instance != null) return;

                        if (DeckAndHandManager.Instance == null)
                        {
                            ReturnCardToHand();
                            return;
                        }

                        if (OptionsSettings.IsCardSelectOnClick)
                        {
                            // Temporarily remove from hand management
                            DeckAndHandManager.Instance.RemoveCard(_cfs.Card);
                            DeckAndHandManager.Instance.SelectCard(_cfs.Card);

                            StartCoroutine(MoveCardToActivePos());
                        }
                        else
                        {
                            // If card is dropped above hand area, try to activate it
                            if (_isAboveHandArea)
                                StartCoroutine(MoveCardToActivePos());
                            else
                            {
                                ReturnCardToHand();
                                return;
                            }
                        }

                        if (_cfs.TryActivateCard()) return;

                        ReturnCardToHand();
                    };
                    break;
            }
            _onMouseUp = tmp;
        }
        private void SetOnMouseDrag()
        {
            Action tmp = null;
            switch (_state)
            {
                case CardState.PackViewer:
                    break;
                case CardState.DeckViewer:
                    break;
                case CardState.Shop:
                    break;
                case CardState.Rewards:
                    break;
                case CardState.Combat:
                    tmp = () =>
                    {
                        // Block card interaction if tutorial is active and not on card step
                        if (TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None &&
                            TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.CardsOnly)
                            return;

                        //disable drag with click to select option enabled
                        if (OptionsSettings.IsCardSelectOnClick || !_cfs.IsDragging || PauseMenu.isPaused || CardShopManager.Instance != null || DeckAndHandManager.Instance == null || _cfs.IsSelected)
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
                            _cardImage.DOColor(spriteColor, _tweenDuration).SetUpdate(true);

                        // Only update order when in hand area
                        if (!_isAboveHandArea)
                            UpdateCardPrefabOrder(true);

                    };
                    break;
            }
            _onMouseDrag = tmp;
        }
    }
}