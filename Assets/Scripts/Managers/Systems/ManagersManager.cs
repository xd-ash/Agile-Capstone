using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ManagersManager : MonoBehaviour
{
    [SerializeField] private List<ManagerBase> _managers = new();
    public List<ManagerBase> GetManagers => _managers;

    public static ManagersManager instance;
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

    public void GrabManagers()
    {
        for (int i = _managers.Count - 1; i >= 0; i--)
            if (_managers[i] == null)
                _managers.RemoveAt(i);
        foreach (ManagerBase manager in GetComponentsInChildren<ManagerBase>())
            if(!_managers.Contains(manager))
                _managers.Add(manager);
    }
    public void UpdateManagerSceneBools()
    {
        foreach (ManagerBase manager in _managers)
            manager.ManagerSceneBoolOnGUI();
    }
    public void SetManagersActiveOrInactive(string sceneName)
    {
        foreach (ManagerBase manager in _managers)
            manager.SetManagerActiveOrInactive(sceneName);
    }

    /*[SerializeField] private Dictionary<GameObject, Dictionary<string, bool>> _managersDict = new();
    public Dictionary<GameObject, Dictionary<string, bool>> GetManagerSceneBoolDict => _managersDict;

    public void GrabManagers()
    {
        Dictionary<string, bool> tempDict = new();
        var sceneNames = GrabAllSceneNames();
        foreach (var sceneName in sceneNames)
            tempDict.Add(sceneName, false);
        RemoveNullManagers();
        UpdateSceneDicts(sceneNames);

        for (int i = 0; i < transform.childCount; i++)
            if (!CheckForManagerElementByGO(transform.GetChild(i).gameObject))
                _managersDict.Add(transform.GetChild(i).gameObject, tempDict);
    }
    private void RemoveNullManagers()
    {
        for (int i = _managersDict.Count - 1; i >= 0; i--)
            if (_managersDict.ElementAt(i).Key == null)
                _managersDict.Remove(_managersDict.ElementAt(i).Key);
    }
    private bool CheckForManagerElementByGO(GameObject gameObject)
    {
        foreach (var kvp in _managersDict)
            if (kvp.Key == gameObject)
                return true;
        return false;
    }
    private List<string> GrabAllSceneNames()
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
    public void SetSceneBool(GameObject gameObject, string sceneName, bool sceneBool)
    {
        if (!_managersDict.ContainsKey(gameObject)) return;
        _managersDict[gameObject][sceneName] = sceneBool;
    }
    private void UpdateManagerElementLists()
    {
        List<string> sceneNames = GrabAllSceneNames();

        foreach (var manager in _managers)
            manager?.UpdateSceneBoolDict(sceneNames);
    }
    private void UpdateSceneDicts(List<string> newSceneNames)
    {
        foreach (var sceneDict in _managersDict.Values)
        {
            foreach (var newSceneName in newSceneNames)
                if (!sceneDict.ContainsKey(newSceneName))
                    sceneDict.Add(newSceneName, false);
            foreach (var sceneName in sceneDict.Keys)
                if (!newSceneNames.Contains(sceneName))
                    sceneDict.Remove(sceneName);
        }
    }
    private void SetManagersActiveOrInactive(string targetScene)
    {
        foreach (var kvp in _managersDict)
            kvp.Key.SetActive(kvp.Value[targetScene]);
    }*/
    /*[SerializeField] private List<ManagerElement> _managers;
    public List<ManagerElement> GetManagers => _managers;

    public void OnGuiActions()
    {
        if (_managers == null)
            _managers = new();

        GrabManagers();
        UpdateManagerElementLists();
    }
    private void GrabManagers()
    {
        List<string> sceneNames = GrabAllSceneNames();
        RemoveNullManagers();

        for (int i = 0; i < transform.childCount; i++)
            if (!CheckForManagerElementByGO(transform.GetChild(i).gameObject))
                _managers.Add(new ManagerElement(transform.GetChild(i).gameObject, sceneNames));
    }
    private void RemoveNullManagers()
    {
        for (int i = _managers.Count - 1; i >= 0; i--)
            if (_managers[i].managerGO == null)
                _managers.RemoveAt(i);
    }
    private bool CheckForManagerElementByGO(GameObject gameObject)
    {
        foreach (var manager in _managers)
            if (manager.managerGO == gameObject)
                return true;
        return false;
    }
    private List<string> GrabAllSceneNames()
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
    private void UpdateManagerElementLists()
    {
        List<string> sceneNames = GrabAllSceneNames();

        foreach (var manager in _managers)
            manager?.UpdateSceneBoolDict(sceneNames);
    }
    private void SetManagersActiveOrInactive(string targetScene)
    {
        foreach (var manager in _managers)
            manager.SetGOActiveOrInactive(targetScene);
    }*/
}
//[System.Serializable] 
/*public class ManagerElement
{
    public GameObject managerGO;
    public Dictionary<string, bool> sceneBools;

    public ManagerElement(GameObject manager, List<string> sceneNames)
    {
        managerGO = manager;

        sceneBools = new();
        foreach (var sceneName in sceneNames)
            sceneBools.Add(sceneName, false);
    }

    // Add or remove dict elements based on current scenes provided in param
    public void UpdateSceneBoolDict(List<string> newSceneNames)
    {
        if (sceneBools == null)
            sceneBools = new();

        foreach (var scene in newSceneNames)
            if (!sceneBools.ContainsKey(scene))
                sceneBools.Add(scene, false);
        foreach (var kvp in sceneBools)
            if (!newSceneNames.Contains(kvp.Key))
                sceneBools.Remove(kvp.Key);
    }
    public bool GetSceneBool(string sceneName)
    {
        if (sceneBools.ContainsKey(sceneName))
            return sceneBools[sceneName];

        Debug.Log("scenebool dict did not contain sceneName key @ GetSceneBool. Returning False");
        return false;
    }
    public void SetGOActiveOrInactive(string currSceneName)
    {
        managerGO.SetActive(GetSceneBool(currSceneName));
    }
}
*/
[CustomEditor(typeof(ManagersManager))]
public class ManagersManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //serializedObject.UpdateIfRequiredOrScript();

        ManagersManager mm = (ManagersManager)target;
        mm.GrabManagers();

        mm.UpdateManagerSceneBools();

        foreach (ManagerBase manager in mm.GetManagers)
        {
            GUILayout.Label($"{manager.name}:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < manager.SceneNames.Count; i++)
            {
                string sceneName = manager.SceneNames[i];
                if (manager.SetSceneBool(sceneName, EditorGUILayout.Toggle(sceneName, manager.SceneBools[i])))
                    EditorUtility.SetDirty(target);
            }
            EditorGUI.indentLevel--;
        }
        /*ManagersManager mm = (ManagersManager)target;
        mm.GrabManagers();

        foreach (var kvp1 in mm.GetManagerSceneBoolDict)
        {
            GUILayout.Label($"{kvp1.Key.name}:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            foreach (var kvp2 in kvp1.Value)
                mm.SetSceneBool(kvp1.Key, kvp2.Key, EditorGUILayout.Toggle(kvp2.Key, kvp2.Value));
            EditorGUI.indentLevel--;
        }*/
        /*
        var managers = mm.GetManagers;
        foreach (var manager in managers)
        {
            GUILayout.Label($"{manager.managerGO.name}:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < manager.sceneBools.Count; i++)
            {
                var kvp = manager.sceneBools.ElementAt(i);
                manager.sceneBools[kvp.Key] = EditorGUILayout.Toggle(kvp.Key, kvp.Value);
            }
            EditorGUI.indentLevel--;
        }
        */

        //serializedObject.ApplyModifiedProperties();
    }
}