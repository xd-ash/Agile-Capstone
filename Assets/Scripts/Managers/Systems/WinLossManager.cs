using UnityEngine;
using System.Collections.Generic;

//Temp Class for easy Win/Loss condition and cyclical gameplay for build
public class WinLossManager : MonoBehaviour
{
    public GameObject winText, loseText;
    public float textDuration = 3f;

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
        GameObject text = didWin ? winText : loseText;
        if (text != null)
        {
            text.SetActive(true);
        }

        Invoke(nameof(TriggerSceneTrans), textDuration);
    }

    public void TriggerSceneTrans()
    {
        //If we have our progress manager, use that
        if (SceneProgressManager.Instance != null)
        {
            SceneProgressManager.Instance.CompleteCurrentNode();
            SceneProgressManager.Instance.ReturnToMap();
        }
        else
        {
            //Fallback to existing transition flow
            if (TransitionScene.instance != null)
                TransitionScene.instance.StartTransition();
        }
    }
}