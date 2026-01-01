using System;
using System.Collections;
using UnityEngine;
using XNode;

namespace CardSystem
{
    // Base abstract effect strategy class
    public abstract class EffectStrategy : AbilityNodeBase
    {
        [SerializeField] protected bool _hasDuration;
        [SerializeField] protected int _duration;
        [SerializeField] protected int _effectValue;

        protected EffectVisualsStrategy _visualsStrategy;
        [Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public long effectVisuals;

        //[Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public float duration; // used for old failed duration node
        public bool HasDuration { get { return _hasDuration; } set { _hasDuration = value; } }

        public virtual void StartEffect(AbilityData abilityData, Action onFinished)
        {
            var def = this.graph as CardAbilityDefinition;
            //Debug.Log($"def effect: {def.abilitySFX.name}");
            AudioManager.instance?.SetPendingUseSfx(def.GetAbilitySFX);

            if (_visualsStrategy == null)
            {
                //make this better?
                try
                {
                    _visualsStrategy = GetPort("effectVisuals").Connection.node as EffectVisualsStrategy;
                }
                catch
                {
                    Debug.Log("null effect visual");
                }
            }
        }

        // Coroutine to cause an effect over a duration, with interval of 1s for testing
        public virtual IEnumerator DoEffectOverTime(Unit unit, int duration, int effectValue = 0)
        {
            int tempDur = duration;
            int newVal = effectValue / duration; // make this more accurate (currently int division)
            bool isHeal = this is HelpfulEffect;
            GameObject auraGO = SpawnPlaceholderAura(unit, this);
            do
            {
                if (newVal != 0)
                    unit.ChangeHealth(newVal, isHeal);
                yield return new WaitForSeconds(1); // tie this to turn order in some way, maybe event
                tempDur--;
            } while (tempDur > 0);
            Destroy(auraGO);
        }

        // Spawn placeholder sprite to show buff/debuffs & HOTs/DOTs on unit
        static GameObject SpawnPlaceholderAura(Unit affectedUnit, EffectStrategy effectStrat)
        {
            Transform auraParent = affectedUnit.transform.Find("AuraPlaceholder");
            string path = "";

            switch (effectStrat)
            {
                case HealEffect:
                    path = "TempAuraEmpties/HealOverTime";
                    break;
                case DamageEffect:
                    path = "TempAuraEmpties/DamageOverTime";
                    break;
                case BuffEffect:
                    path = "TempAuraEmpties/Buff";
                    break;
                case DebuffEffect:
                    path = "TempAuraEmpties/Debuff";
                    break;
            }
            GameObject auraGO = Instantiate(Resources.Load<GameObject>(path), auraParent);
            auraGO.transform.localPosition = new Vector3(0f, 0.2f * auraParent.childCount, 0f); // hard coded in y shift value for rough stacking of placeholder auras
            return auraGO;
        }
        /*
        public virtual void DoOverTimeEffect(GameObject target, EffectStrategy thisEffect)
        {
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is OverTimeEffect == false)
                    continue;

                if (target.GetComponent<Unit>() == null)
                {
                    Debug.Log("Do Over Time method target.gecomp<unit> is null");
                    continue;
                }
                OverTimeEffect OTEPort = port.Connection.node as OverTimeEffect;
                OTEPort.StartOverTimeEffect(target.GetComponent<Unit>(), thisEffect);
            }
        }*/
    }
}