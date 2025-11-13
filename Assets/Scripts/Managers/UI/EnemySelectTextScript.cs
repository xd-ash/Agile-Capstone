using UnityEngine;
using static AbilityEvents;

//temp script for target selecting text
public class EnemySelectTextScript : MonoBehaviour
{
    private GameObject _text;

    private void Awake()
    {
        _text = transform.Find("Text").gameObject;

        AbilityEvents.OnAbilityTargetingStarted += OnTargetUpdate;
        AbilityEvents.OnAbilityTargetingStopped += OnTargetUpdate;
    }
    private void OnDestroy()
    {
        AbilityEvents.OnAbilityTargetingStarted -= OnTargetUpdate;
        AbilityEvents.OnAbilityTargetingStopped -= OnTargetUpdate;
    }
    public void OnTargetUpdate()
    {
        _text.SetActive(IsTargeting);
    }
}
