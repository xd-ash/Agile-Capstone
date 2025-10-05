using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace CardSystem
{
    public abstract class TargetingStrategy : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override)] public bool enter; //only one thing can be plugged in

        public bool isAOE;
        public float radius;
        public LayerMask layerMask;

        public abstract void StartTargeting(AbilityData abilityData, Action onFinished);
        public abstract IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished);
        protected abstract IEnumerable<GameObject> GetGameObjectsInRadius(Unit unit);
    }
}