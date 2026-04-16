using CardSystem;
using UnityEngine;
using static IsoMetricConversions;

[CreateNodeMenu("Visual Effects/Explosion Visual")]
public class ExplosionEffectVisual : EffectVisualsStrategy
{
    [SerializeField] private GameObject _explosionEffect;

    public override void CreateVisualEffect(AbilityData abilityData, GameObject target)
    {
        if (target == null || _explosionEffect == null) return;

        GameObject effect = Instantiate(_explosionEffect, MapCreator.Instance.transform);
        Vector3 spawnPos = abilityData.AbilityTriggerPos == -Vector2Int.one ? target.transform.localPosition : ConvertToIsometricFromGrid(abilityData.AbilityTriggerPos);
        effect.transform.localPosition = spawnPos;
        if (!effect.TryGetComponent(out Animator eAnimator) || !effect.TryGetComponent(out AbilityEffectDestroyer eDestroyer))
            return;
        float animLength = eAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        eDestroyer.Invoke("DeleteMe", animLength);
    }
}

