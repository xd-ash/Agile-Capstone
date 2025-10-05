using System;
using System.Linq;
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
            foreach (GameObject target in abilityData.Targets)
            {
                if (target != null)
                {
                    target.GetComponent<Unit>().TakeDamage(damage);
                }
            }
            if (abilityData.Targets.Count<GameObject>() > 0)
                abilityData.GetUnit.SpendAP(CardManager.instance.selectedCard.APCost); //maybe fix this? kinda messy
                                                                                       //move somewhere else? event?
            onFinished();
        }
    }
}