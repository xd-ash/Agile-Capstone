using CardSystem;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CardState { PackViewer, DeckViewer, Shop, Rewards, Combat }

public static class CardPrefabSetterUpper
{
    private static float _combatScale = 1.44f; // scale value determined through in scene editing and lazily implemented

    public static bool SetupCardPrefab(Transform cardTrans, CardAbilityDefinition cardDef, CardState cardState = CardState.Combat)
    {
        if (!FillTextFields(cardTrans, cardDef)) 
            return FailPrefabSetup($"(Text field fill failure.)");

        switch (cardState)
        {
            case CardState.PackViewer:
                return SetupPackViewerCard(cardTrans, cardDef);
            case CardState.DeckViewer:
                return SetupDeckViewerCard(cardTrans, cardDef);
            case CardState.Shop:
                return SetupShopCard(cardTrans, cardDef);
            case CardState.Rewards:
                return SetupRewardsCard(cardTrans, cardDef);
            default:
                return SetupCombatCard(cardTrans, cardDef);
        }
    }
    private static bool SetupPackViewerCard(Transform cardTrans, CardAbilityDefinition cardDef)
    {
        DisableBoxCollider(cardTrans);
        SetCardState(cardTrans, CardState.PackViewer);
        return true;
    }
    private static bool SetupDeckViewerCard(Transform cardTrans, CardAbilityDefinition cardDef)
    {
        DisableBoxCollider(cardTrans);
        SetCardState(cardTrans, CardState.DeckViewer);
        return true;
    }
    private static bool SetupShopCard(Transform cardTrans, CardAbilityDefinition cardDef)
    {
        RemoveButton(cardTrans);
        SetCardState(cardTrans, CardState.Shop);
        return true;
    }
    private static bool SetupRewardsCard(Transform cardTrans, CardAbilityDefinition cardDef)
    {
        RemoveButton(cardTrans);
        SetCardState(cardTrans, CardState.Rewards);
        return true;
    }
    private static bool SetupCombatCard(Transform cardTrans, CardAbilityDefinition cardDef)
    {
        cardTrans.localScale = Vector3.one * _combatScale;
        RemoveButton(cardTrans);
        SetCardState(cardTrans, CardState.Combat);
        return true;
    }

    private static bool FillTextFields(Transform cardTrans, CardAbilityDefinition cardDef)
    {
        // Get all TextMeshPro components (non-UI version)
        TextMeshProUGUI[] cardTextFields = cardTrans.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var cardTextField in cardTextFields)
            if (cardTextField == null) return false;

        if (cardTextFields.Length >= 3)
        {
            // Update text content
            cardTextFields[0].text = cardDef.GetCardName;
            cardTextFields[1].text = cardDef.GetDescription;
            cardTextFields[2].text = cardDef.GetApCost.ToString();
        }
        else
        {
            Debug.LogError("Card prefab is missing required TextMeshProUGUI components");
            return false;
        }

        if (!cardTrans.TryGetComponent(out DeckViewerCardVisuals visualsControl))
            return false;
        visualsControl?.ApplyVisuals(cardDef);
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
    private static void SetCardState(Transform cardTrans, CardState state)
    {
        if (!cardTrans.TryGetComponent(out CardSelect cs)) return;
        cs.InitCardSelect(state);
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
