using UnityEngine;

public class SaveLoadController : MonoBehaviour
{
    public static SaveLoadController instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void OnContineGame(string sceneName)
    {
        SaveLoadScript.LoadGame?.Invoke();
        TransitionScene.instance.StartTransition();
    }
}
