using System;
using UnityEngine;
using static IsoMetricConversions;

public abstract class SpawnObjectTracker : MonoBehaviour
{
    protected Guid _guid;
    protected Action<Unit> _onTriggerAction;
    protected Unit _creator;
    protected Vector2Int _pos;

    private void OnEnable()
    {
        OnSpawn();
    }
    public abstract void OnSpawn();

    public void Initialize(Guid guid, Unit creator)
    {
        _guid = guid;
        _creator = creator;
        _pos = ConvertToGridFromIsometric(transform.localPosition);
    } 
    public void SetOnTrigger(Action<Unit> action)
    {
        _onTriggerAction += action;
    }
    private void OnDestroy()
    {
        _onTriggerAction = null;
    }
    public void InvokeOnTrigger(Unit unit)
    {
        _onTriggerAction?.Invoke(unit);
    }
}
