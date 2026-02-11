using System;
using System.Collections.Generic;
using CardSystem;
using UnityEngine;
using XNode;

[CreateNodeMenu("Misc Effects/Spawn Object")]
public class SpawnObject : EffectStrategy, IStoppable, IPassSpawnedObjs
{
    public GameObject _prefab;
    public Dictionary<Guid, List<SpawnObjectTracker>> spawnedObjs = new();

    [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public float objEffects;

    public void PassObject(AbilityData abilityData, SpawnObjectTracker tracker)
    {
        foreach (NodePort port in Outputs)
        {
            if (port.Connection == null || port.Connection.node == null || port.Connection.node is not IAcceptSpawnObjs)
                continue;

            (port.Connection.node as IAcceptSpawnObjs).AcceptObject(abilityData, tracker);
        }
    }

    public override void StartEffect(AbilityData abilityData, Action onFinished)
    {
        base.StartEffect(abilityData, onFinished);

        GameObject prefab = Instantiate(_prefab, Vector3.zero, Quaternion.identity);

        SpawnObjectTracker sot; //= prefab.GetComponent<SpawnObjectTracker>();
        if (!prefab.TryGetComponent(out sot))
            sot = prefab.AddComponent<SpawnObjectTracker>();

        /*if (sot == null)
            sot = prefab.AddComponent<SpawnObjectTracker>();
        */
        sot.SpawnObjectTRacker(abilityData.GetGUID);

        if (!spawnedObjs.ContainsKey(abilityData.GetGUID))
            spawnedObjs.Add(abilityData.GetGUID, new List<SpawnObjectTracker>());

        spawnedObjs[abilityData.GetGUID].Add(sot);

        PassObject(abilityData, sot);
    }

    public void Stop(Guid guid)
    {
        if (!spawnedObjs.ContainsKey(guid)) return;

        for (int i = spawnedObjs[guid].Count - 1; i >= 0; i--)
            Destroy(spawnedObjs[guid][i].gameObject);

        spawnedObjs.Remove(guid);
    }
}
public class SpawnObjectTracker : MonoBehaviour
{
    public bool one;

    private Guid _guid;
    private Action _OnTouched;

    private void Update()
    {
        //different inputs trigger different objs, bool for differenciating objs
        if (Input.GetKeyDown(KeyCode.Alpha1) && one)
        {
            InvokeOnTouched();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && !one)
        {
            InvokeOnTouched();
        }
    }
    //pseudo constructor
    public void SpawnObjectTRacker(Guid guid)
    {
        _guid = guid;
    }
    public void OnTouch(Action action)
    {
        _OnTouched += action;
    }
    public void InvokeOnTouched()
    {
        _OnTouched?.Invoke();
    }
}