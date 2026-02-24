using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    // Concrete targeting strategy to cast ability on the unit with an option to affectSelf or not
    [CreateNodeMenu("Targeting/Self")]
    public class SelfTarget : TargetingStrategy
    {
        public override void StartTargeting(AbilityData abilityData, ref Action onFinished)
        {
            base.StartTargeting(abilityData, ref onFinished);

            abilityData.Targets = new List<GameObject>() { abilityData.GetUnit.gameObject };

            //Start target selection for players, 
            switch (abilityData.GetUnit.GetTeam)
            {
                case Team.Friendly:
                    if (!_aoeStrat)
                    {
                        onFinished?.Invoke();
                        break;
                    }
                    abilityData.GetUnit.StartTargetingCoroutine(TargetingCoro(abilityData, onFinished));
                    break;
                case Team.Enemy:
                    GoapAgent agent = abilityData.GetUnit.GetComponent<GoapAgent>();
                    _aoeStrat?.GrabTargetsInRange(ref abilityData);
                    onFinished();
                    break;
            }
        }

        public override IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished)
        {
            Unit caster = abilityData.GetUnit;
            var def = graph as CardAbilityDefinition;

            while (true)
            {
                _aoeStrat?.GrabTargetsInRange(ref abilityData);

                if (Input.GetMouseButtonDown(0))
                    break;

                yield return null;
            }

            onFinished?.Invoke();
        }
    }
}