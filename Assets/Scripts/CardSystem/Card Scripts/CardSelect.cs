using System.Collections;
using UnityEngine;
using DG.Tweening;
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

        private Action _onMouseDown, _onMouseUp, _onMouseDrag, _onMouseEnter, _onMouseExit;

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

        public void InitCardSelect(Card card, CardState state, Action onCardClick = null)
        {
            _state = state;

            SetupVisuals();

            _originalScale = transform.localScale;
            _originalColor = _cardImage.color;

            SetOnMouseDown();
            SetOnMouseUp();
            SetOnMouseDrag();
            SetOnMouseEnter();
            SetOnMouseExit();

            OnPrefabCreation(card, onCardClick);
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
            _onMouseEnter?.Invoke();
        }
        private void OnMouseExit()
        {
            _onMouseExit?.Invoke();
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

        // card lerp to "active position" to fix cards covering playing grid
        private IEnumerator MoveCardToActivePos()
        {
            Transform target = DeckAndHandManager.Instance.CardActivePos;
            Vector3 targetPos = target.transform.localPosition + Vector3.up * HandPositionController.Instance.GetCardActivePosYAdjustment;
            Vector3 initCardPos = transform.localPosition;
            Quaternion initCardRot = transform.localRotation;

            //Lerp duration uses tween duration
            for (float timer = 0f; timer < _tweenDuration; timer += Time.deltaTime)
            {
                float lerpRatio = timer / _tweenDuration;
                transform.localPosition = Vector3.Lerp(initCardPos, targetPos, lerpRatio);
                transform.localRotation = Quaternion.Lerp(initCardRot, target.transform.localRotation, lerpRatio);
                yield return null;
            }

            transform.localPosition = targetPos;
        }
        public void ToggleHighlightAndScale(bool isHoveredOrSelected)
        {
            _cardHighlight?.SetActive(isHoveredOrSelected);
            transform.localPosition += isHoveredOrSelected ? Vector3.forward : Vector3.back;

            float scaleMultiplier = isHoveredOrSelected ? _hoverScaleMultiplier : 1;
            transform.DOScale(_originalScale * scaleMultiplier, _tweenDuration);

            if (_state != CardState.Combat) return;

            CardPrefabSetterUpper.SetCombatCardGOOrder(isHoveredOrSelected ? transform : null);

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
        private void OnPrefabCreation(Card card, Action prefabButtonOnclick = null)
        {
            if (card == null)
            {
                Debug.LogError("OnPrefabCreation: Card parameter is null");
                return;
            }

            _cfs?.OnPrefabCreation(card, _state, prefabButtonOnclick);

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
                case CardState.Combat:
                    tmp = () =>
                    {
                        if (RewardsDisplayScript.IsRewarding || WinLossManager.Instance != null && WinLossManager.Instance.IsGameComplete) return;

                        if (ToggleHandPosButton.Instance != null && ToggleHandPosButton.Instance.IsHovered) return;

                        // Block card interaction if tutorial is active and not on card step
                        if (TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None &&
                            TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.CardsOnly &&
                            TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.MoveAndCards)
                        {
                            ToggleHighlightAndScale(false);
                            DeckAndHandManager.Instance.ToggleCollidersOnHover(transform, false);
                            ReturnCardToHand();
                            return;
                        }
                        if (TransitionScene.IsTutorial && _cfs.Card.GetCardAbility?.GetCardCategory != TutorialManager.Instance.GetExpectedCatagory && TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None) return;

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
                default:
                    break;
            }
            _onMouseDown = tmp;
        }
        private void SetOnMouseUp()
        {
            Action tmp = null;
            switch (_state)
            {
                case CardState.Combat:
                    tmp = () =>
                    {
                        if (RewardsDisplayScript.IsRewarding || WinLossManager.Instance != null && WinLossManager.Instance.IsGameComplete) return;

                        if (ToggleHandPosButton.Instance != null && ToggleHandPosButton.Instance.IsHovered) return;

                        if (TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None &&
                            TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.CardsOnly &&
                            TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.MoveAndCards)
                        {
                            ToggleHighlightAndScale(false);
                            DeckAndHandManager.Instance.ToggleCollidersOnHover(transform, false);
                            ReturnCardToHand();
                            return;
                        }
                        if (TransitionScene.IsTutorial && _cfs.Card.GetCardAbility?.GetCardCategory != TutorialManager.Instance.GetExpectedCatagory && TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None) return;

                        if (PauseMenu.isPaused || _cfs.IsSelected || DeckAndHandManager.Instance == null || TurnManager.IsEnemyTurn) return;

                        if (!_cfs.IsDragging && !OptionsSettings.IsCardSelectOnClick || 
                            OptionsSettings.IsCardSelectOnClick && DeckAndHandManager.Instance.GetSelectedCard != null || 
                            CardShopManager.Instance != null) return;

                        if (DeckAndHandManager.Instance == null)
                        {
                            ReturnCardToHand();
                            return;
                        }
                        DeckAndHandManager.Instance.ToggleCollidersOnHover(transform, false);


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
                default:
                    break;
            }
            _onMouseUp = tmp;
        }
        private void SetOnMouseDrag()
        {
            Action tmp = null;
            switch (_state)
            {
                case CardState.Combat:
                    tmp = () =>
                    {
                        if (RewardsDisplayScript.IsRewarding || WinLossManager.Instance != null && WinLossManager.Instance.IsGameComplete) return;

                        if (ToggleHandPosButton.Instance != null && ToggleHandPosButton.Instance.IsHovered) return;

                        // Block card interaction if tutorial is active and not on card step
                        if (TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None &&
                            TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.CardsOnly &&
                            TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.MoveAndCards)
                        {
                            ToggleHighlightAndScale(false);
                            DeckAndHandManager.Instance.ToggleCollidersOnHover(transform, false);
                            ReturnCardToHand();
                            return;
                        }
                        if (TransitionScene.IsTutorial && _cfs.Card.GetCardAbility?.GetCardCategory != TutorialManager.Instance.GetExpectedCatagory && TutorialManager.CurrentInputMode != TutorialManager.TutorialInputMode.None) return;

                        //disable drag with click to select option enabled
                        if (OptionsSettings.IsCardSelectOnClick || !_cfs.IsDragging || PauseMenu.isPaused || CardShopManager.Instance != null || DeckAndHandManager.Instance == null || _cfs.IsSelected)
                            return;

                        // Temporarily remove from hand management
                        DeckAndHandManager.Instance.RemoveCard(_cfs.Card);
                        DeckAndHandManager.Instance.SelectCard(_cfs.Card);
                        DeckAndHandManager.Instance.ToggleCollidersOnHover(transform, false);

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
                default:
                    break;
            }
            _onMouseDrag = tmp;
        }
      
        private void SetOnMouseEnter()
        {
            Action tmp = null;
            switch (_state)
            {
                case CardState.Combat:
                    tmp = () =>
                    {
                        if (RewardsDisplayScript.IsRewarding) return;

                        if (ToggleHandPosButton.Instance != null && ToggleHandPosButton.Instance.IsHovered) return;

                        if (!_cfs.IsSelected && !_cfs.IsDragging && !PauseMenu.isPaused && DeckAndHandManager.Instance.GetSelectedCard == null)
                        {
                            ToggleHighlightAndScale(true);
                            DeckAndHandManager.Instance.ToggleCollidersOnHover(transform, true);
                        }
                        int cost = _cfs.Card?.GetCardAbility?.GetApCost ?? 0;
                        APDisplay.Instance?.ShowPreview(cost);
                    };
                    break;
                case CardState.DeckViewer:
                case CardState.CardRemoval:
                case CardState.FreeCardRemoval:
                case CardState.CardSwap:
                case CardState.UpgradeMenu:
                case CardState.FreeUpgradeMenu:
                    tmp = () =>
                    {
                        if (DeckEditingController.IsPreviewingEdit || CampNodeController.IsPreviewingUpgrade) return;
                        if (ShopConfirmPopup.Instance != null && ShopConfirmPopup.Instance.gameObject.activeInHierarchy) return;

                        if (!_cfs.IsSelected && !_cfs.IsDragging && !PauseMenu.isPaused)
                            ToggleHighlightAndScale(true);
                    };
                    break;
                case CardState.Inactive:
                    break;
                case CardState.Shop:
                    tmp = () =>
                    {
                        if (DeckViewerScript.Instance != null && DeckViewerScript.Instance.gameObject.activeInHierarchy) return;
                        if (ShopConfirmPopup.Instance != null && ShopConfirmPopup.Instance.gameObject.activeInHierarchy) return;

                        if (!_cfs.IsSelected  && !_cfs.IsDragging && !PauseMenu.isPaused)
                            ToggleHighlightAndScale(true);
                    };
                    break;
                default:
                    tmp = () =>
                    {
                        if (!_cfs.IsSelected  && !_cfs.IsDragging && !PauseMenu.isPaused)
                            ToggleHighlightAndScale(true);
                    };
                    break;
            }
            _onMouseEnter = tmp;
        }
        private void SetOnMouseExit()
        {
            Action tmp = null;
            switch (_state)
            {
                case CardState.Combat:
                    tmp = () =>
                    {
                        if (RewardsDisplayScript.IsRewarding) return;

                        if (!_cfs.IsSelected && !_cfs.IsDragging && !PauseMenu.isPaused)
                        {
                            ToggleHighlightAndScale(false);
                            DeckAndHandManager.Instance.ToggleCollidersOnHover(transform, false);

                            if (DeckAndHandManager.Instance != null && DeckAndHandManager.Instance.GetSelectedCard != null) return;

                            APDisplay.Instance?.ClearPreview();
                        }
                    };
                    break;
                case CardState.DeckViewer:
                case CardState.CardRemoval:
                case CardState.FreeCardRemoval:
                case CardState.CardSwap:
                case CardState.UpgradeMenu:
                case CardState.FreeUpgradeMenu:
                    tmp = () =>
                    {
                        if (DeckEditingController.IsPreviewingEdit || CampNodeController.IsPreviewingUpgrade) return;
                        if (ShopConfirmPopup.Instance != null && ShopConfirmPopup.Instance.gameObject.activeInHierarchy) return;

                        if (!_cfs.IsSelected && !_cfs.IsDragging && !PauseMenu.isPaused)
                            ToggleHighlightAndScale(false);
                    };
                    break;
                case CardState.Inactive:
                    break;
                case CardState.Shop:
                    tmp = () =>
                    {
                        if (DeckViewerScript.Instance != null && DeckViewerScript.Instance.gameObject.activeInHierarchy) return;
                        if (ShopConfirmPopup.Instance != null && ShopConfirmPopup.Instance.gameObject.activeInHierarchy) return;

                        if (!_cfs.IsSelected && !_cfs.IsDragging && !PauseMenu.isPaused)
                            ToggleHighlightAndScale(false);
                    };
                    break;
                default:
                    tmp = () =>
                    {
                        if (!_cfs.IsSelected && !_cfs.IsDragging && !PauseMenu.isPaused)
                            ToggleHighlightAndScale(false);
                    };
                    break;
            }
            _onMouseExit = tmp;
        }
    }
}