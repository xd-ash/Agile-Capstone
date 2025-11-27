using System;
<<<<<<< Updated upstream:Assets/Scripts/CardSystem/CardAbilitySystem/_GraphAndNodes/EffectStrategies/DamageEffect.cs
using UnityEditor.Experimental.GraphView;
=======
using System.Linq;
>>>>>>> Stashed changes:Assets/Scripts/CardSystem/CardAbilitySystem/EffectStrategies/DamageEffect.cs
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
        //public DamageTypes damageType;

        public override void StartEffect(AbilityData abilityData, Action onFinished)
        {
            base.StartEffect(abilityData, onFinished);

            foreach (GameObject target in abilityData.Targets)
            {
<<<<<<< Updated upstream:Assets/Scripts/CardSystem/CardAbilitySystem/_GraphAndNodes/EffectStrategies/DamageEffect.cs
                if (target != null && target.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    bool hit = CombatMath.RollHit(abilityData.GetUnit, targetUnit, out int hitChance, out float roll);

                    if (!hit)
                    {
                        // TODO: floating 'Miss' text here
                        continue;
                    }

                    if (_hasDuration)
                        targetUnit.StartCoroutine(DoEffectOverTime(targetUnit, _duration, _effectValue));
                    else
                        targetUnit.ChangeHealth(_effectValue, false);
=======
                if (target != null)
                {
                    target.GetComponent<Unit>().TakeDamage(damage);
>>>>>>> Stashed changes:Assets/Scripts/CardSystem/CardAbilitySystem/EffectStrategies/DamageEffect.cs
                }
            }
            if (abilityData.Targets.Count<GameObject>() > 0)
                abilityData.GetUnit.SpendAP(CardManager.instance.selectedCard.APCost); //maybe fix this? kinda messy
                                                                                       //move somewhere else? event?
            onFinished();
        }
    }
}