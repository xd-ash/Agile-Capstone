using CardSystem;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ThrowableEffectVisual : EffectVisualsStrategy
{
    [SerializeField] private GameObject _throwable;
    [SerializeField] private float _travelDuration = 0.5f;

    public override void CreateVisualEffect(AbilityData abilityData, GameObject target)
    {
        
    }

    public IEnumerator ThrowablePathCoro(GameObject throwable, Vector3 startPos, Vector3 endPos)
    {
        var dir = (endPos - startPos).normalized;
        var height = Mathf.Abs(startPos.y - endPos.y);
        var normal = Vector3.Cross(dir, Vector3.back);
        var maxArcHeight = height * normal;

        throwable.transform.DOBlendableMoveBy(endPos, _travelDuration * 0.5f);
        throwable.transform.DOBlendableMoveBy(endPos, _travelDuration * 0.5f);
        //invoke
    }
    private void SecondHalfArc(GameObject throwable, Vector3 startPos, Vector3 endPos)
    {
        throwable.transform.DOBlendableMoveBy(endPos, _travelDuration * 0.5f);
    }
}
