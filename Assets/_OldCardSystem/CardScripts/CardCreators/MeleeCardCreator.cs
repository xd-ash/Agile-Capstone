using UnityEngine;

namespace OldCardSystem
{
    public class MeleeCardCreator : CardCreator
    {
        public MeleeCardCreator(CardSO SO) : base(SO)
        {

        }

        public override CardBase CreateCard(Transform parent)
        {
            GameObject cardGO = GameObject.Instantiate(_cardSO.CardPrefab, parent);
            SetCardPrefabInfo(cardGO);
            return IsAttackCard() ? new MeleeAttackCard(_cardSO) : new MeleeEffectCard(_cardSO);
        }
    }
}