using UnityEngine;

public class ReturnToMapButton : MonoBehaviour
{
    public void OnClickReturn()
    {
        if (SceneProgressManager.Instance != null)
        { 
            SceneProgressManager.Instance.CompleteCurrentNode();
            SceneProgressManager.Instance.ReturnToMap();
        }
        else
        {
            TransitionScene.Instance.StartTransition();
        }
    }
}