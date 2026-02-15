using CardSystem;
using XNode;
using System.Collections.Generic;
using UnityEngine;

public class OnTouched : AbilityNodeBase, IAcceptSpawnObjs, IPassSpawnedObjs
{
    [Input(connectionType = ConnectionType.Override)] public float input;

    [Output(dynamicPortList = true, connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] public byte effects;

    public void AcceptObject(AbilityData abilityData, SpawnObjectTracker tracker)
    {
        foreach (NodePort port in Outputs)
        {
            if (port.Connection == null || port.Connection.node == null || port.Connection.node is not EffectStrategy)
                continue;

            //tracker.SetOnTrigger((unit) => (port.Connection.node as EffectStrategy).StartEffect(abilityData, null));

            //set the trigger action to grab the incoming unit praram (from event/trigger method), set abilityData target, then start the given effect
            tracker.SetOnTrigger((unit) =>
            {
                abilityData.Targets = new List<GameObject>() { unit.gameObject };
                (port.Connection.node as EffectStrategy).StartEffect(abilityData, () => Destroy(tracker.gameObject));
            });
        }
    }

    public void PassObject(AbilityData abilityData, SpawnObjectTracker tracker)
    {
        throw new System.NotImplementedException();
    }
}