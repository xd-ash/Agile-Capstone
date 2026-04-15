using System;
using System.Collections.Generic;
using CardSystem;
using UnityEngine;
using XNode;

[CreateNodeMenu("Misc Effects/Spawn Object")]
public class SpawnObjectEffect : EffectStrategy, IStoppable, IPassSpawnedObjs
{
    public GameObject _prefab;
    public Dictionary<Guid, List<SpawnObjectTracker>> spawnedObjs = new();
    public enum SpawnObjectType { Trap, other }
    public SpawnObjectType objectType = SpawnObjectType.Trap;

    [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public float objEffects;

    public override void StartEffect(AbilityData abilityData, Action onFinished, int effectValueChange = 0)
    {
        base.StartEffect(abilityData, onFinished, effectValueChange);

        foreach (var target in abilityData.Targets)
        {
            GameObject prefab = Instantiate(_prefab, Vector3.zero, Quaternion.identity, FindFirstObjectByType<MapCreator>().transform);
            prefab.transform.localPosition = target.transform.localPosition;
            Destroy(target.gameObject);

            if (!prefab.TryGetComponent(out SpawnObjectTracker sot))
            {
                switch (objectType)
                {
                    case SpawnObjectType.Trap:
                        sot = prefab.AddComponent<TrapObjectTracker>();
                        break;
                    default:
                        break;
                }
            }
            sot.Initialize(abilityData.GetGUID, abilityData.GetUnit);

            if (!spawnedObjs.ContainsKey(abilityData.GetGUID))
                spawnedObjs.Add(abilityData.GetGUID, new List<SpawnObjectTracker>());

            spawnedObjs[abilityData.GetGUID].Add(sot);

            PassObject(abilityData, sot);
        }

        _onFinished?.Invoke();
    }

    public void PassObject(AbilityData abilityData, SpawnObjectTracker tracker)
    {
        foreach (NodePort port in Outputs)
        {
            if (port.Connection == null || port.Connection.node == null || port.Connection.node is not IAcceptSpawnObjs)
                continue;

            (port.Connection.node as IAcceptSpawnObjs).AcceptObject(abilityData, tracker);
        }
    }

    public void Stop(Guid guid)
    {
        if (!spawnedObjs.ContainsKey(guid)) return;

        for (int i = spawnedObjs[guid].Count - 1; i >= 0; i--)
            Destroy(spawnedObjs[guid][i].gameObject);

        spawnedObjs.Remove(guid);
    }
}
