using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class ManagerBase : MonoBehaviour
{
    // Parallel lists representing each scene in the project and if the manager should be active within the scene
    public List<bool> SceneBools { get; protected set; }
    public List<string> SceneNames { get; protected set; }

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
        if (SceneBools == null) SceneBools = new();
        if (SceneNames == null) SceneNames = new();

        foreach (var newSceneName in newSceneNames)
            if (!SceneNames.Contains(newSceneName))
            {
                SceneBools.Add(false);
                SceneNames.Add(newSceneName);
            }
        foreach (var sceneName in SceneNames)
            if (!newSceneNames.Contains(sceneName))
            {
                SceneBools.RemoveAt(SceneNames.IndexOf(sceneName));
                SceneNames.Remove(sceneName);
            }
    }
    public virtual bool SetSceneBool(string sceneName, bool sceneBool)
    {
        if (!SceneNames.Contains(sceneName)) return false;

        int index = SceneNames.IndexOf(sceneName);
        SceneBools[index] = sceneBool;
        return true;
    }
}
