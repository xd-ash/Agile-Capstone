using UnityEngine;
using static AbilityEvents;

//temp script for target selecting text
public class EnemySelectTextScript : MonoBehaviour
{
    private GameObject _text;

    private void Awake()
    {
        _text = transform.Find("Text").gameObject;

        OnAbilityTargetingStarted += OnTargetUpdate;
        OnAbilityTargetingStopped += OnTargetUpdate;
    }
    private void OnDestroy()
    {
        OnAbilityTargetingStarted -= OnTargetUpdate;
        OnAbilityTargetingStopped -= OnTargetUpdate;
    }
    public void OnTargetUpdate()
    {
        _text.SetActive(IsTargeting);
    }
}
