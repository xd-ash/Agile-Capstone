using System.Collections;
using UnityEngine;

public class CanvasCameraSetter : MonoBehaviour
{
    private Canvas _canvas;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        _canvas = GetComponent<Canvas>();

        TransitionScene.SceneSwap += SetConnector;
    }
    private void OnDestroy()
    {
        TransitionScene.SceneSwap -= SetConnector;
    }
    //connector for setting cam. Ran into issues trying to just add a listener with a lambda experssion
    private void SetConnector(string unused)
    {
        Invoke(nameof(SetCam), 1f);
    }
    private void SetCam()
    {
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = Camera.main;
    }
}
