using UnityEngine;
using UnityEngine.UI;

public class StartGameButtonScript : MonoBehaviour
{
    [SerializeField] private string _targetScene = "NodeMap";
    [SerializeField] private bool _isNewGame = true;

    private void OnEnable()
    {
        if (!_isNewGame && TryGetComponent(out Button button))
            button.interactable = SaveLoadScript.CheckForSaveGame;
    }

    public void LaunchGame()
    {
        if (!_isNewGame)
            SaveLoadScript.LoadGame?.Invoke();
        SaveLoadScript.SaveGame?.Invoke();
        TransitionScene.instance.StartTransition(_targetScene);
    }
}
