using UnityEngine;

namespace OldCardSystem
{
    public class MiscCardCreator : CardCreator
    {
        public MiscCardCreator(CardSO SO) : base(SO)
        {

        }

        public override CardBase CreateCard(Transform parent)
        {
            throw new System.NotImplementedException();
        }
    }
}