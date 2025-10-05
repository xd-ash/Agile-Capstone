using System;
using UnityEngine;

namespace CardSystem
{
    public enum DamageTypes
    {
        None,
        Slash,
        Pierce,
        Fire,
        Emotional
    }

    [CreateNodeMenu("Effect/Damage")]
    public class DamageEffect : EffectStrategy
    {
        public int damage;
        public DamageTypes damageType;

        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            abilityData.GetUnit.TakeDamage(damage);
            onFinished();
        }
    }
}