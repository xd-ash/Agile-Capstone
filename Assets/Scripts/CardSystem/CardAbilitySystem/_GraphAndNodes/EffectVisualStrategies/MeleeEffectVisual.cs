using CardSystem;
using UnityEngine;

public class MeleeEffectVisual : EffectVisualsStrategy
{
    [SerializeField] private GameObject _meleeEffect;

    public override void CreateVisualEffect(AbilityData abilityData, Unit target)
    {
        if (target == null) return;
        Unit unit = abilityData.GetUnit;

        // normalized dir from unit to target
        Vector3 dirToTarget = (target.transform.position - unit.transform.position).normalized;

        if (_meleeEffect != null)
        {
            GameObject effect = Instantiate(_meleeEffect);
            effect.transform.position = target.GetComponentInChildren<SpriteRenderer>().transform.position - dirToTarget * 0.2f;
            if (!effect.TryGetComponent<Animator>(out Animator eAnimator) || !effect.TryGetComponent<AbilityEffectDestroyer>(out AbilityEffectDestroyer eDestroyer))
                return;
            float animLength = eAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            eDestroyer.Invoke("DeleteMe", animLength);
        }
    }
}
