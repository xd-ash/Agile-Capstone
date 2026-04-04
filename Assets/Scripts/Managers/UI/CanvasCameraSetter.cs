using System.Collections;
using UnityEngine;

public class CanvasCameraSetter : MonoBehaviour
{
    private Canvas _canvas;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        _canvas = GetComponent<Canvas>();

        TransitionScene.SceneSwap += (x) => Invoke(nameof(SetCam), 1f);
    }
    private void OnDestroy()
    {
        TransitionScene.SceneSwap -= (x) => Invoke(nameof(SetCam), 1f);
    }
    private void SetCam()
    {
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = Camera.main;
    }
}
