using UnityEngine;

public class TrapObjectTracker : SpawnObjectTracker
{
    public void CheckForTriggerOnTouch(Vector2Int trapPos, Unit unitThatTriggered)
    {
        if (trapPos != _pos) return;
        
        InvokeOnTrigger(unitThatTriggered);
    }

    public override void OnSpawn()
    {
        if (MapCreator.Instance == null) return;
        ByteMapController.TileEntered += CheckForTriggerOnTouch;
    }
}