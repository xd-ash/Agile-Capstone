using CardSystem;
using System.Collections;
using UnityEngine;

public class BulletEffectVisual : EffectVisualsStrategy
{
    [SerializeField] private GameObject _smokeEffect;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private float _bulletTravelDur = 1f;

    public override void CreateVisualEffect(AbilityData abilityData, Unit target)
    {
        if (target == null) return;
        Unit unit = abilityData.GetUnit;

        // normalized dir from unit to target
        Vector3 dirToTarget = (target.transform.position - unit.transform.position).normalized;

        // Gun smoke isntantiation
        if (_smokeEffect != null)
        {
            GameObject smoke = Instantiate(_smokeEffect);
            smoke.transform.position = unit.GetComponentInChildren<SpriteRenderer>().transform.position + dirToTarget * 0.2f;
            if (!smoke.TryGetComponent<Animator>(out Animator sAnimator) || !smoke.TryGetComponent<AbilityEffectDestroyer>(out AbilityEffectDestroyer sDestroyer))
                return;
            float animLength = sAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            sDestroyer.Invoke("DeleteMe", animLength);
        }

        if (_bullet != null)
        {
            // Bullet instatiation, rotation, and travel coro start
            GameObject bullet = Instantiate(_bullet, unit.transform.position, Quaternion.LookRotation(Vector3.back, dirToTarget));
            if (!bullet.TryGetComponent<AbilityEffectDestroyer>(out AbilityEffectDestroyer bDestroyer))
                return;
            Vector3 startPos = unit.GetComponentInChildren<SpriteRenderer>().transform.position;
            Vector3 endPos = target.GetComponentInChildren<SpriteRenderer>().transform.position;
            bDestroyer.StartCoroutine(BulletTravelCoro(bullet, startPos, endPos));
            bDestroyer.Invoke("DeleteMe", _bulletTravelDur);
        }
    }

    public IEnumerator BulletTravelCoro(GameObject bullet, Vector3 startPos, Vector3 endPos)
    {
        for (float timer = 0f; timer < _bulletTravelDur; timer += Time.deltaTime)
        {
            float lerpRatio = timer / _bulletTravelDur;

            bullet.transform.localPosition = Vector3.Lerp(startPos, endPos, lerpRatio);
            yield return null;
        }
    }
}
