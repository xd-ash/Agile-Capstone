using System;
using CardSystem;

public interface IStoppable
{
    void Stop(Guid guid);
}
public interface IAcceptSpawnObjs
{
    void AcceptObject(AbilityData abilityData, SpawnObjectTracker tracker);
}
public interface IPassSpawnedObjs
{
    void PassObject(AbilityData abilityData, SpawnObjectTracker tracker);
}