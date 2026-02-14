using CardSystem;
using XNode;

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

            tracker.SetOnTrigger(() => (port.Connection.node as EffectStrategy).StartEffect(abilityData, null));
        }
    }

    public void PassObject(AbilityData abilityData, SpawnObjectTracker tracker)
    {
        throw new System.NotImplementedException();
    }
}