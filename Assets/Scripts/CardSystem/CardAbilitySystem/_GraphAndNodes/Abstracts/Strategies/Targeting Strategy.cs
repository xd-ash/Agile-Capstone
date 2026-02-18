using System;
using System.Collections;
using UnityEngine;
using XNode;

namespace CardSystem
{
    // Base abstract targeting strategy
    public abstract class TargetingStrategy : AbilityNodeBase
    {
        [Input(connectionType = ConnectionType.Override)] public short abilityRoot;
        [Output(connectionType = ConnectionType.Override)] public bool aoeStrat;

        //[SerializeField] private bool _requireAbilityTargetConfirm;

        protected OnAOETarget _aoeStrat;
        //public bool GetTargetconfirmBool => _requireAbilityTargetConfirm;

        public virtual void StartTargeting(AbilityData abilityData, Action onFinished)
        {
            if (abilityData.GetUnit.GetTeam != Team.Enemy)
            {
                AbilityEvents.TargetingStarted();
                AudioManager.Instance?.PlayCardSelectSfx();
            }
            foreach (NodePort port in Outputs)
            {
                if (port.Connection == null || port.Connection.node == null || port.Connection.node is not OnAOETarget)
                    continue;
                _aoeStrat = port.Connection.node as OnAOETarget;
                _aoeStrat?.InitNode();
            }
        }
        public abstract IEnumerator TargetingCoro(AbilityData abilityData, Action onFinished);
    }
}