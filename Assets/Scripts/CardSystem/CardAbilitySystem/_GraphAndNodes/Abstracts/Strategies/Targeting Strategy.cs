using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardSystem
{
    // Base abstract targeting strategy
    public abstract class TargetingStrategy : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override)] public short input;

        public bool isAOE;
        public float radius;

        public virtual void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            if (abilityData.GetUnit.team != Team.Enemy)
                AudioManager.instance?.PlayCardSelectSfx();
        }
        public abstract IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished);
        protected abstract IEnumerable<GameObject> GetGameObjectsInRadius(Unit unit);
    }
}