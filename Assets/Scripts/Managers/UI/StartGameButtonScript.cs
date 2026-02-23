using UnityEngine;
using UnityEngine.UI;

public class StartGameButtonScript : MonoBehaviour
{
    [SerializeField] private string _targetScene = "NodeMap";
    [SerializeField] private bool _isNewGame = true;
    private GameObject _confirmPopup;
    private Button _button;

    private void OnEnable()
    {
        _confirmPopup = transform.parent.Find("NewGameConfirmPanel")?.gameObject;
        _confirmPopup?.SetActive(false);

        if (_isNewGame || !TryGetComponent(out _button)) return;

        _button.interactable = false;
        Invoke(nameof(ToggleInteractable), 0.01f);
    }
    private void ToggleInteractable()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.Log("PlayerDataManager is null for some reason :(");
            return;
        }
        _button.interactable = SaveLoadScript.CheckForSaveGame && PlayerDataManager.Instance.GetCompletedNodes.Length > 0;
    }
    public void ShowConfirmPopupOrLaunch()
    {
        if (SaveLoadScript.CheckForSaveGame && PlayerDataManager.Instance.GetCompletedNodes.Length > 0)
            _confirmPopup?.SetActive(true);
        else
            LaunchGame();
    }
    public void LaunchGame()
    {
        if (_isNewGame)
            SaveLoadScript.CreateNewGame?.Invoke();
        SaveLoadScript.LoadGame?.Invoke();
        TransitionScene.Instance.StartTransition(_targetScene);
    }
}
