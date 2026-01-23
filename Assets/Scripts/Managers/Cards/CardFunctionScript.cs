using CardSystem;
using TMPro;
using UnityEngine;
using System;

public class CardFunctionScript : MonoBehaviour
{
    public Card Card { get; private set; }
    public bool IsSelected { get; private set; } = false;
    public bool IsDragging { get; private set; } = false;

    private void OnMouseDown()
    {
        if (PauseMenu.isPaused || IsSelected) return;

        bool isShopActive = CardShopManager.Instance != null;

        // Shop-mode: show confirmation popup instead of drag/drop purchase
        if (isShopActive)
        {
            int price = Card.ShopCost;
            string cardName = Card?.GetCardName ?? "Card";

            Action confirmAction = () =>
            {
                if (CurrencyManager.instance != null && CurrencyManager.instance.TrySpend(price))
                {
                    DeckAndHandManager.instance?.AddDefinitionToRuntimeDeck(Card.GetCardAbility);

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
                Debug.LogWarning("Shop confirm popup is null. Fallback confirm action called.");
            };

            ShopConfirmPopup.Instance?.Show(price, cardName, confirmAction, cancelAction);

            if (ShopConfirmPopup.Instance == null)
                cancelAction();
        }

        if (DeckAndHandManager.instance.CardsInHand.IndexOf(Card) == -1) return;

        IsDragging = true;
    }

    // Try activate a card, return true if successful, false if not
    public bool TryActivateCard()
    {
        if (Card == null || Card.GetCardAbility?.RootNode == null || DeckAndHandManager.instance == null /*|| DeckAndHandManager.instance.SelectedCard != null*/)
            return false;

        var currentUnit = TurnManager.GetCurrentUnit;
        int cost = Card.GetCardAbility.GetApCost;

        if (currentUnit == null || !currentUnit.CanSpend(cost))
        {
            OutOfApPopup.Instance?.Show();
            return false;
        }

        IsSelected = true;
        DeckAndHandManager.instance.SelectCard(Card);
        CardSplineManager.instance.ArrangeCardGOs();

        Card.GetCardAbility.UseAility(currentUnit);
        return true;
    }

    public void ClearSelection(float tweenDuration)
    {
        IsSelected = false;
        IsDragging = false;
    }

    public void OnPrefabCreation(Card card)
    {
        Card = card;
        Card.CardTransform = transform;
        transform.name = card.GetCardName;
    }

    public void EnableShopMode()
    {
        int cost = Mathf.Max(0, Card.ShopCost);

        // If the prefab has a cost display (third TextMeshPro), update it.
        TextMeshPro[] cardTextFields = GetComponentsInChildren<TextMeshPro>();
        if (cardTextFields.Length >= 3)
            cardTextFields[2].text = cost.ToString();
    }
}
