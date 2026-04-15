using UnityEngine;

public class TargetingEmptyIdentifier : MonoBehaviour
{
    private void Awake()
    {
        TurnManager.Instance.OnTurnEnd += DeleteMe;
    }
    private void OnDestroy()
    {
        TurnManager.Instance.OnTurnEnd -= DeleteMe;
    }
    private void DeleteMe(Unit notUsed)
    {
        Destroy(gameObject);
    }
}
