using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    [Tooltip("If true this GameObject will persist across scene loads.")]
    [SerializeField] private bool persistAcrossScenes = true;

    [Tooltip("If true, `LoadScene(string)` will call async load by default.")]
    [SerializeField] private bool useAsyncByDefault = false;

    private void Awake()
    {
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Simple method you can wire from a UI Button (accepts a string parameter in the inspector).
    /// Example: drag the GameObject that has this component into the Button OnClick slot,
    /// choose SceneTransitioner -> LoadScene (string) and type the scene name.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[SceneTransitioner] LoadScene called with empty sceneName.");
            return;
        }

        if (!SceneExistsInBuild(sceneName))
        {
            Debug.LogWarning($"[SceneTransitioner] Scene '{sceneName}' not found in Build Settings.");
            return;
        }

        if (useAsyncByDefault)
            SceneManager.LoadSceneAsync(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Explicit async loader if you want to call from code.
    /// </summary>
    public void LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        if (!SceneExistsInBuild(sceneName))
        {
            Debug.LogWarning($"[SceneTransitioner] Scene '{sceneName}' not found in Build Settings.");
            return;
        }
        SceneManager.LoadSceneAsync(sceneName);
    }

    /// <summary>
    /// Load by build index (also easy to wire from code).
    /// </summary>
    public void LoadScene(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[SceneTransitioner] Build index {buildIndex} is out of range.");
            return;
        }
        SceneManager.LoadScene(buildIndex);
    }

    private bool SceneExistsInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, sceneName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
