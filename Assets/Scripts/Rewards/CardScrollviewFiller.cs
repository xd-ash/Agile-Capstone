using CardSystem;
using System;
using UnityEngine;
using static GameObjectPool;

public static class CardScrollviewFiller
{
    public static void BuildScrollViewContent(Transform contentParent, GameObject contentPrefab, CardAbilityDefinition[] cardCollection, CardState cardState, Action<Transform, CardAbilityDefinition> onCreationAction = null)
    {
        bool isFullCard = contentPrefab.TryGetComponent(out CardSelect cs);

        ClearScrollViewContent(contentParent);

        foreach (var card in cardCollection)
        {
            if (card == null) continue;

            GameObject content = Spawn(contentPrefab, Vector3.zero, Quaternion.identity, contentPrefab.transform.localScale, contentParent);
            onCreationAction?.Invoke(content.transform, card);

            if (!isFullCard) continue;

            var tempCard = new Card(card, card.GetBaseCardRarity, content.transform);
            CardPrefabSetterUpper.SetupCardPrefab(tempCard, cardState);
        }
    }
    public static void BuildScrollViewContent(Transform contentParent, GameObject contentPrefab, Card[] cardCollection, CardState cardState, Action<Transform, Card> onCreationAction = null, Action<Transform, Card> onClickAction = null)
    {
        bool isFullCard = contentPrefab.TryGetComponent(out CardSelect cs);

        ClearScrollViewContent(contentParent);

        foreach (var card in cardCollection)
        {
            if (card == null) continue;

            GameObject content = Spawn(contentPrefab, Vector3.zero, Quaternion.identity, contentPrefab.transform.localScale, contentParent);
            onCreationAction?.Invoke(content.transform, card);

            if (!isFullCard) continue;

            card.OnPrefabCreation(content.transform);
            CardPrefabSetterUpper.SetupCardPrefab(card, cardState, onClickAction == null ? null : () => onClickAction(card.GetCardTransform, card));
        }
    }

    public static void ClearScrollViewContent(Transform contentParent)
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            GameObject.Destroy(contentParent.GetChild(i).gameObject);
    }
}
