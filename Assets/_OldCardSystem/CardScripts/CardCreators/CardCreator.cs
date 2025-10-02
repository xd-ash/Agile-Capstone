using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OldCardSystem
{
    public abstract class CardCreator
    {
        public CardSO _cardSO;
        public CardCreator(CardSO cardSO)
        {
            _cardSO = cardSO;
        }
        public abstract CardBase CreateCard(Transform parent);
        public virtual bool IsAttackCard()
        {
            if (_cardSO != null)
            {
                if (_cardSO.GetCardTypeID()[1] == '1')
                {
                    return true;
                }
                return false;
            }
            else
            {
                throw new System.Exception("CardSO is null (CardCreator.IsAttackCard())");
            }
        }
        public virtual void SetCardPrefabInfo(GameObject cardGO)
        {
            cardGO.name = _cardSO.CardName;
            TextMeshPro[] cardTextFields = cardGO.transform.GetComponentsInChildren<TextMeshPro>();
            cardTextFields[0].text = _cardSO.CardName;
            cardTextFields[1].text = _cardSO.Description;
            cardTextFields[2].text = _cardSO.APCost.ToString();
        }
    }
}
