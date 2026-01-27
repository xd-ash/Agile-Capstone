using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class ManagerBase : MonoBehaviour
{
    // Parallel lists representing each scene in the project and if the manager should be active within the scene
    [SerializeField] protected List<bool> _sceneBools;
    [SerializeField] protected List<string> _sceneNames;
    public List<bool> SceneBools => _sceneBools;
    public List<string> SceneNames => _sceneNames;

    public virtual void ManagerSceneBoolOnGUI()
    {
        UpdateSceneLists(GetAllSceneNames());
    }
    protected virtual List<string> GetAllSceneNames()
    {
        List<string> sceneNames = new();
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

        foreach (var sceneGUID in sceneGUIDs)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
            string[] pathComponents = scenePath.Split('/');
            string sceneName = pathComponents[^1].Split(".")[0];
            sceneNames.Add(sceneName);
        }

        return sceneNames;
    }
    protected virtual void UpdateSceneLists(List<string> newSceneNames)
    {
        if (_sceneBools == null) { Debug.Log("test"); _sceneBools = new(); }
        if (_sceneNames == null) { Debug.Log("test"); _sceneNames = new(); }

        foreach (var newSceneName in newSceneNames)
            if (!SceneNames.Contains(newSceneName))
            {
                _sceneBools.Add(false);
                _sceneNames.Add(newSceneName);
            }
        foreach (var sceneName in SceneNames)
            if (!newSceneNames.Contains(sceneName))
            {
                _sceneBools.RemoveAt(SceneNames.IndexOf(sceneName));
                _sceneNames.Remove(sceneName);
            }
    }
    public virtual bool SetSceneBool(string sceneName, bool sceneBool)
    {
        if (!_sceneNames.Contains(sceneName)) return false;

        int index = _sceneNames.IndexOf(sceneName);
        if (_sceneBools[index] == sceneBool) return false;
        _sceneBools[index] = sceneBool;
        return true;
    }
    public virtual void SetManagerActiveOrInactive(string sceneName)
    {
        if (!_sceneNames.Contains(sceneName))
        {
            Debug.LogError($"Setting manager GO active/insactive failed. Scene name not in list");
            return;
        }

        int index = SceneNames.IndexOf(sceneName);
        this.gameObject.SetActive(_sceneBools[index]);
    }
}
