using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace CardSystem
{
    /*
    [CreateNodeMenu("Effect Extensions/Over Time Effect")]
    public class OverTimeEffect : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override)] public float input;

        [SerializeField] private int _duration;
        [SerializeField] private int _tickFrequency = 1; // add bool for if ticks are needed? restrict to just dmg/heal?

        public void StartOverTimeEffect(Unit affectedUnit, EffectStrategy effectStrat, int effectValue)
        {
            SpawnPlaceholderAura(affectedUnit, effectStrat, effectValue);// placeholder aura spawns

            // maybe start coroutine on an aura manager?
        }
        public void SpawnPlaceholderAura(Unit affectedUnit, EffectStrategy effectStrat, int effectValue)
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
            affectedUnit.StartCoroutine(DurationEffectCoro(_duration, _tickFrequency, path.Substring(15), auraGO, effectValue));
        }

        //Placeholder coroutine to tick down and delete aura placeholder after duration
        public IEnumerator DurationEffectCoro(int duration, int frequency, string auraName, GameObject auraPlaceholder, int effectValue)
        {
            int effectTime = duration;
            while (effectTime > 0)
            {
                effectTime -= frequency;
                Debug.Log($"{auraName.ToString()} tick.");
                yield return new WaitForSeconds(frequency); // change to listen for turn change/ unit turn (w/ some manager?)
            }
            Destroy(auraPlaceholder);
        }
    }
    */
}