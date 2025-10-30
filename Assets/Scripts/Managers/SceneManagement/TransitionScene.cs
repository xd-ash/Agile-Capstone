using UnityEngine;

public class TransitionScene : MonoBehaviour
{
    public string sceneToLoad;
    public void StartTransition()
    { 
        Debug.Log("Scene transition started.");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);

        //Handle anything else needed between scenes here
        PauseMenu.isPaused = false;
    }
}
