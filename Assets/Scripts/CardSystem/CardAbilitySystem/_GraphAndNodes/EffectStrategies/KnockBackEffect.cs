using System;
using UnityEngine;
using static IsoMetricConversions;

namespace CardSystem
{
    public class KnockBackEffect : EffectStrategy
    {
        public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            base.StartEffect(abilityData, onFinished, effectValueChange);

            foreach (GameObject target in abilityData.Targets)
            {
                if (target == null) return;

                Vector2Int casterGridPos = ConvertToGridFromIsometric(abilityData.GetUnit.transform.localPosition);
                Vector2Int targetGridPos = ConvertToGridFromIsometric(target.transform.localPosition);
                //Vector2Int 
            }

            onFinished?.Invoke();
        }
    }
}