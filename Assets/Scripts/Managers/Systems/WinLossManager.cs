using UnityEngine;
using System.Collections.Generic;

//Temp Class for easy Win/Loss condition and cyclical gameplay for build
public class WinLossManager : MonoBehaviour
{
    [SerializeField] private GameObject winText, loseText;
    [SerializeField] private float textDuration = 3f;
    private bool _didWin;

    public List<Unit> enemyUnits;

    public static WinLossManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        GameOverEvents.OnGameOver += OnGameDone;
    }

    private void OnDestroy()
    {
        GameOverEvents.OnGameOver -= OnGameDone;
    }

    public void OnGameDone(bool didWin)
    {
        _didWin = didWin;
        GameObject text = _didWin ? winText : loseText;
        text?.SetActive(true);

        Invoke(nameof(TriggerSceneTrans), textDuration);
    }

    public void TriggerSceneTrans()
    {
        if (_didWin && SceneProgressManager.Instance != null)
        {
            SceneProgressManager.Instance.CompleteCurrentNode();
            if (!SceneProgressManager.Instance.NodeMapCompleted)
            {
                SceneProgressManager.Instance.ReturnToMap();
                return;
            }

            SceneProgressManager.Instance?.ResetNodes();
        }

        TransitionScene.instance?.StartTransition();
    }
}