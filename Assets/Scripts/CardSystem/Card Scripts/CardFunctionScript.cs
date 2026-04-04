using CardSystem;
using DG.Tweening;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CardFunctionScript : MonoBehaviour
{
    [SerializeField]private CardState _state;
    public Card Card { get; private set; }
    public bool IsSelected { get; private set; } = false;
    public bool IsDragging { get; private set; } = false;

    private Action _onMouseDown;

    private void OnMouseDown()
    {
        _onMouseDown?.Invoke();
    }
    // Try activate a card, return true if successful, false if not
    public bool TryActivateCard()
    {
        if (Card == null || Card.GetCardAbility?.RootNode == null || DeckAndHandManager.Instance == null /*|| DeckAndHandManager.Instance.SelectedCard != null*/)
            return false;

        var currentUnit = TurnManager.GetCurrentUnit;
        int cost = Card.GetCardAbility.GetApCost;

        if (currentUnit == null || !currentUnit.CanSpend(cost))
        {
            OutOfApPopup.Instance?.Show();
            return false;
        }

        IsSelected = true;
        DeckAndHandManager.Instance?.SelectCard(Card);
        CardSplineManager.Instance?.ArrangeCardGOs();

        Card.UseAbility(currentUnit);
        return true;
    }

    public void ClearSelection(float tweenDuration)
    {
        IsSelected = false;
        IsDragging = false;
    }

    public void OnPrefabCreation(Card card, CardState state)
    {
        _state = state;
        SetOnMouseDown();

        Card = card;
        transform.name = card.GetCardName;

        CardPrefabSetterUpper.SetupCardPrefab(Card, _state);
    }

    public void EnableShopMode()
    {
        int cost = Mathf.Max(0, Card.GetShopCost);
        // If the prefab has a cost display (third TextMeshPro), update it.
        //TextMeshPro[] cardTextFields = GetComponentsInChildren<TextMeshPro>();
        //if (cardTextFields.Length >= 3)
        //cardTextFields[2].text = cost.ToString();
        Debug.Log("Cost replacing AP display is disabled. Displaying only the AP on shop card");
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
                tmp = () =>
                {
                    if (PauseMenu.isPaused || IsSelected) return;

                    int price = Card.GetShopCost;
                    string cardName = Card?.GetCardName ?? "Card";

                    Action confirmAction = () =>
                    {
                        if (CurrencyManager.Instance != null && CurrencyManager.Instance.TrySpend(price))
                        {
                            DeckAndHandManager.Instance?.AddCardToRuntimeDeck(Card);

                            CardShopManager.Instance?.DeleteCard(gameObject);
                        }
                        else
                            OutOfApPopup.Instance?.Show();
                    };

                    Action cancelAction = () =>
                    {
                        // no-op; popup will just close
                        Debug.LogWarning("Shop confirm popup is null. Fallback confirm action called.");
                    };

                    ShopConfirmPopup.Instance?.Show(price, cardName, confirmAction, cancelAction);

                    if (ShopConfirmPopup.Instance == null)
                        cancelAction();
                    return;
                };
                break;
            case CardState.Rewards:
                break;
            case CardState.Combat:
                tmp = () =>
                {
                    if (RewardsDisplayScript.IsRewarding || WinLossManager.Instance != null && WinLossManager.Instance.IsGameComplete) return;
                    if (PauseMenu.isPaused || IsSelected || DeckAndHandManager.Instance == null || DeckAndHandManager.Instance.GetSelectedCard != null || TurnManager.IsEnemyTurn) return;
                    if (TurnManager.Instance != null && TurnManager.GetCurrentUnit.GetIsMoving) return;
                    if (DeckAndHandManager.Instance.CardsInHand.IndexOf(Card) == -1) return;
                    if (OptionsSettings.IsCardSelectOnClick) return;

                    IsDragging = true;
                };
                break;
        }
        _onMouseDown = tmp;
    }
    public void SetOnMouseDown(CardState state, Action onclick)
    {
        if (_state != state)
        {
            Debug.Log($"State mismatch. Incoming: {state}, cur:{_state} ({name})");
            return;
        }
        _onMouseDown = null;
        _onMouseDown += () => { IsSelected = true; };
        _onMouseDown += onclick;
    }
}
