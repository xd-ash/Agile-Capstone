using UnityEngine;

public class TrapObjectTracker : SpawnObjectTracker
{
    public void CheckForTriggerOnTouch(Vector2Int pos, Unit unitThatTriggered)
    {
        if (pos != _pos) return;
        
        InvokeOnTrigger(unitThatTriggered);
    }

    public override void OnSpawn()
    {
        if (MapCreator.Instance == null) return;
        MapCreator.TileEntered += CheckForTriggerOnTouch;
    }
}