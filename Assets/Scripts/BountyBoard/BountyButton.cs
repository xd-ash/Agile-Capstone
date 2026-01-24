using UnityEngine;
using UnityEngine.UI;

public class BountyButtonSimple : MonoBehaviour
{
    [Tooltip("Name of the combat scene to load for this bounty")]
    [SerializeField] private string _combatSceneName = "LevelOne";
    
    private Button _button;

    private void Awake()
    {
        if (TryGetComponent(out _button))
            _button?.onClick.AddListener(OnClicked);
        else
            Debug.LogWarning("BountyButton on " + gameObject.name + " has no Button component.");
    }

    private void OnClicked()
    {
        if (string.IsNullOrEmpty(_combatSceneName))
        {
            Debug.LogError("BountyButton: combatSceneName is empty on " + gameObject.name);
            return;
        }

        TransitionScene.instance.StartTransition(_combatSceneName);
    }
}