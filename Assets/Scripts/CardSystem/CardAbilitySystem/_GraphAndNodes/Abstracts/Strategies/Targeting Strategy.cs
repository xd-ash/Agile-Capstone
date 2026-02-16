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

        public virtual void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            if (abilityData.GetUnit.GetTeam != Team.Enemy)
            {
                AbilityEvents.TargetingStarted();
                AudioManager.Instance?.PlayCardSelectSfx();
            }
        }
        protected abstract IEnumerable<GameObject> GetGameObjectsInRadius(Unit unit);
    }
}