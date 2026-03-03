using System;
using UnityEngine;

namespace CardSystem
{
    public abstract class EffectStrategy : AbilityNodeBase
    {
        [SerializeField, HideInInspector] protected int _effectValue;
        protected Action _onFinished;

        [Input(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte input;

        protected EffectVisualsStrategy _visualsStrategy;
        [Output(connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public long effectVisuals;

        public virtual void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
        {
            var def = this.graph as CardAbilityDefinition;
            AudioManager.Instance?.SetPendingUseSfx(def.GetAbilitySFX);

            if (_visualsStrategy == null)
                _visualsStrategy = GetPort("effectVisuals")?.Connection?.node as EffectVisualsStrategy;

            _onFinished = onFinished;

            int temp = _effectValue;
            _effectValue += effectValueChange;
            _onFinished += () => { ResetValue(temp); };

            int dirIndex = ComputeAttackDirection(abilityData);

            var dirAnimator = abilityData.GetUnit?.GetComponent<DirectionAnimator>();
            dirAnimator?.PlayAttack(def.GetAttackAnimKey, dirIndex, null);

            DoEffect(abilityData);
        }
        
        protected virtual void DoEffect(AbilityData abilityData) { }

        protected void ResetValue(int initVal)
        {
            _effectValue = initVal;
        }

        private int ComputeAttackDirection(AbilityData abilityData)
        {
            if (abilityData == null || abilityData.GetUnit == null)
                return 0;

            Vector2Int casterPos = ByteMapController.Instance.GetPositionOfUnit(abilityData.GetUnit);

            if (abilityData.Targets != null)
            {
                foreach (var t in abilityData.Targets)
                {
                    if (t == null) continue;
                    var targetUnit = t.GetComponent<Unit>();
                    if (targetUnit == null) continue;

                    Vector2Int targetPos = ByteMapController.Instance.GetPositionOfUnit(targetUnit);
                    if (targetPos.x >= 0) // valid position
                        return DirectionAnimator.GetDirIndexFromDelta(targetPos - casterPos);
                }
            }

            Vector2Int triggerPos = abilityData.AbilityTriggerPos;
            if (triggerPos.x >= 0 && triggerPos != casterPos)
                return DirectionAnimator.GetDirIndexFromDelta(triggerPos - casterPos);

            return abilityData.GetUnit.GetComponent<DirectionAnimator>()?.GetLastDir ?? 0;
        }
    }
}
