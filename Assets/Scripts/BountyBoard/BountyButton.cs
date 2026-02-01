using UnityEngine;
using UnityEngine.UI;

public class BountyButton : MonoBehaviour
{
    private Button _button;

    [Space(10), Tooltip("Combat scene data to pass on scene swap")]
    [SerializeField] private CombatMapData _combatNodeData; //needs some kind of system for randomizing?

    private void Awake()
    {
        if (TryGetComponent(out _button))
            _button?.onClick.AddListener(OnClicked);
        else
            Debug.LogWarning("BountyButton on " + gameObject.name + " has no Button component.");
    }

    private void OnClicked()
    {
        PlayerDataManager.Instance.SetCurrMapNodeData(_combatNodeData);
        TransitionScene.instance.StartTransition("Combat");
    }
}
//struct to store data on how many enemies/players to spawn based on which node is selected
//move me somewhere else?
[System.Serializable]
public struct CombatMapData
{
    public int maxPlayersAllowed;
    public int maxEnemiesAllowed;
}