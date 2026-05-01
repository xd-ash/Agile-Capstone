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

        int aoeRange = 0;
        foreach (var node in (graph as CardAbilityDefinition).nodes)
        {
            if (node is not OnAOETarget) continue;
            aoeRange = (node as OnAOETarget).GetAOERange;
            break;
        }

        GameObject effect = Instantiate(_explosionEffect, MapCreator.Instance.transform);
        Vector3 spawnPos = abilityData.AbilityTriggerPos == -Vector2Int.one ? target.transform.localPosition : ConvertToIsometricFromGrid(abilityData.AbilityTriggerPos);
        effect.transform.localPosition = spawnPos;
        effect.transform.localScale = effect.transform.localScale * aoeRange;
        if (!effect.TryGetComponent(out Animator eAnimator) || !effect.TryGetComponent(out AbilityEffectDestroyer eDestroyer))
            return;
        float animLength = eAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        eDestroyer.Invoke("DeleteMe", animLength);
    }
}

