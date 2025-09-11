using UnityEngine;

namespace CardSystem
{
    public class MeleeCardCreator : CardCreator
    {
        public MeleeCardCreator(CardSO SO) : base(SO)
        {

        }

        public override CardBase CreateCard(Transform parent)
        {
            GameObject cardGO = GameObject.Instantiate(_cardSO.CardPrefab, parent);
            SetCarDPrefabInfo(cardGO);
            return IsAttackCard() ? new MeleeAttackCard(_cardSO) : new MeleeEffectCard(_cardSO);
        }
    }
}