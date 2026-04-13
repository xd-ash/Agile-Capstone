using CardSystem;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CardState { PackViewer, DeckViewer, Shop, Rewards, Combat, UpgradeMenu }

public static class CardPrefabSetterUpper
{
    private static float _combatScale = 1.44f; // transform scale determined through in scene editing

    public static bool SetupCardPrefab(Card card, CardState cardState = CardState.Combat, Action onClick = null)
    {
        if (card == null || card.GetCardAbility == null || card.GetCardTransform == null)
            return FailPrefabSetup($"(Null card data)");
        if (!FillTextFields(card))
            return FailPrefabSetup($"(Text field fill failure.)");

        card.GetCardTransform.name = card.GetCardName;

        SetRarityVisuals(card);

        switch (cardState)
        {
            case CardState.PackViewer:
                return SetupPackViewerCard(card, onClick);
            case CardState.DeckViewer:
                return SetupDeckViewerCard(card, onClick);
            case CardState.Shop:
                return SetupShopCard(card, onClick);
            case CardState.Rewards:
                return SetupRewardsCard(card, onClick);
            case CardState.UpgradeMenu:
                return SetupUpgradeMenuCard(card, onClick);
            default:
                return SetupCombatCard(card, onClick);
        }
    }
    private static bool SetupPackViewerCard(Card card, Action onClick = null)
    {
        DisableBoxCollider(card.GetCardTransform);
        SetCardState(card, CardState.PackViewer, onClick);
        return true;
    }
    private static bool SetupDeckViewerCard(Card card, Action onClick = null)
    {
        RemoveButton(card.GetCardTransform);
        SetCardState(card, CardState.DeckViewer, onClick);
        return true;
    }
    private static bool SetupShopCard(Card card, Action onClick = null)
    {
        RemoveButton(card.GetCardTransform);
        SetCardState(card, CardState.Shop, onClick);
        return true;
    }
    private static bool SetupRewardsCard(Card card, Action onClick = null)
    {
        RemoveButton(card.GetCardTransform);
        SetCardState(card, CardState.Rewards, onClick);
        return true;
    }
    private static bool SetupUpgradeMenuCard(Card card, Action onClick = null)
    {
        RemoveButton(card.GetCardTransform);
        SetCardState(card, CardState.UpgradeMenu, onClick);
        return true;
    }
    private static bool SetupCombatCard(Card card, Action onClick = null)
    {
        card.GetCardTransform.localScale = Vector3.one * _combatScale;
        RemoveButton(card.GetCardTransform);
        SetCardState(card, CardState.Combat, onClick);
        return true;
    }

    private static bool FillTextFields(Card card)
    {
        // Get all TextMeshPro components (non-UI version)
        TextMeshProUGUI[] cardTextFields = card.GetCardTransform.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var cardTextField in cardTextFields)
            if (cardTextField == null) return false;

        if (cardTextFields.Length >= 3)
        {
            // Update text content
            cardTextFields[0].text = card.GetCardAbility.GetCardName;
            cardTextFields[1].text = card.GetDescription;
            cardTextFields[2].text = card.GetCardAbility.GetApCost.ToString();
        }
        else
        {
            Debug.LogError("Card prefab is missing required TextMeshProUGUI components");
            return false;
        }

        if (!card.GetCardTransform.TryGetComponent(out DeckViewerCardVisuals visualsControl))
            return false;
        visualsControl?.ApplyVisuals(card.GetCardAbility);
        return true;
    }
    private static bool SetRarityVisuals(Card card)
    {
        var rarityDiamond = card.GetCardTransform.Find("RarityDiamond")?.GetComponent<Image>();
        if (rarityDiamond == null)
            return FailPrefabSetup($"Rarity diamond null.");

        Color rarityColor = Color.gray7;
        switch (card.GetCardRarity)
        {
            case CardRarity.Epic:
                rarityColor = Color.mediumPurple;
                break;
            case CardRarity.Rare:
                rarityColor = Color.mediumBlue;
                break;
            default:
                break;
        }
        rarityDiamond.color = rarityColor;
        return true;
    }
    private static bool RemoveButton(Transform cardTrans)
    {
        var button = cardTrans.GetComponentInChildren<Button>();
        if (button == null) return false;
        button.gameObject.SetActive(false);
        return true;
    }
    private static bool SetButtonFunc(Transform cardTrans, Action buttonFunc)
    {
        var button = cardTrans.GetComponentInChildren<Button>();
        if (button == null) return false;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => buttonFunc?.Invoke());
        return true;
    }
    private static bool DisableBoxCollider(Transform cardTrans)
    {
        if (!cardTrans.TryGetComponent(out BoxCollider2D bc))
            return false;
        bc.enabled = false;
        return true;
    }
    private static void SetCardState(Card card, CardState state, Action onClick = null)
    {
        if (!card.GetCardTransform.TryGetComponent(out CardSelect cs)) return;
        cs.InitCardSelect(card, state, onClick);
    }
    public static void SetCombatCardGOOrder(Transform bringThisForward = null)
    {
        if (DeckAndHandManager.Instance == null) return;

        var cardGoParent = DeckAndHandManager.Instance.transform;
        var curHand = DeckAndHandManager.Instance.CardsInHand;

        for (int i = 0; i < curHand.Count; i++)
        {
            if (curHand[i] == null || curHand[i].GetCardTransform == null) continue;

            //curHand[i].GetCardTransform.SetParent(cardGoParent);
            curHand[i].GetCardTransform.SetSiblingIndex(i);
        }

        if (bringThisForward == null) return;

        bringThisForward.SetAsLastSibling();
    }
    private static bool FailPrefabSetup(string message)
    {
        Debug.LogWarning($"Card prefab has failed to set up. {message}");
        return false;
    }
}
