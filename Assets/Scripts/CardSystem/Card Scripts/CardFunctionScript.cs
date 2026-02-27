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
        if (PauseMenu.isPaused || IsSelected || DeckAndHandManager.Instance == null || DeckAndHandManager.Instance.GetSelectedCard != null || TurnManager.IsEnemyTurn) return;

        bool isShopActive = CardShopManager.Instance != null;

        // Shop-mode: show confirmation popup instead of drag/drop purchase
        if (isShopActive)
        {
            int price = Card.GetShopCost;
            string cardName = Card?.GetCardName ?? "Card";

            Action confirmAction = () =>
            {
                if (CurrencyManager.Instance != null && CurrencyManager.Instance.TrySpend(price))
                {
                    DeckAndHandManager.Instance?.AddDefinitionToRuntimeDeck(Card.GetCardAbility);

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

        if (DeckAndHandManager.Instance.CardsInHand.IndexOf(Card) == -1) return;
        if (OptionsSettings.IsCardSelectOnClick) return;

        IsDragging = true;
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
            }
        }
        else
            Debug.LogError("Card prefab is missing required TextMeshPro components");
    }

    public void EnableShopMode()
    {
        int cost = Mathf.Max(0, Card.GetShopCost);

        // If the prefab has a cost display (third TextMeshPro), update it.
        TextMeshPro[] cardTextFields = GetComponentsInChildren<TextMeshPro>();
        if (cardTextFields.Length >= 3)
            cardTextFields[2].text = cost.ToString();
    }
}
