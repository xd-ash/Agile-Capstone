using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;

public class SceneSwapWindowTool : EditorWindow
{
    private Vector2 scrollPosition;

    Dictionary<string, List<string>> _sceneCollections = new();
    Dictionary<string, bool> _foldOutDict = new();

    [MenuItem("Window/Scene Swap Tool")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneSwapWindowTool>();
        window.titleContent = new GUIContent("Scene Swap Tool");
        window.Show();
    }

    private void OnEnable()
    {
        minSize = new Vector2(225, 300);
        maxSize = new Vector2(225.1f, 300.1f);
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Grab all project relevant scenes, add them to dictionary with asset contatining
        // folder as key and list of scene paths as value
        _sceneCollections = new();

        var sceneGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        List<string> scenePaths = new();

        foreach (var sceneGUID in sceneGUIDs)
            scenePaths.Add(AssetDatabase.GUIDToAssetPath(sceneGUID));
        foreach (var scenePath in scenePaths)
        {
            string[] pathComponents = scenePath.Split('/');
            string sceneContainingfolder = pathComponents[^2];
            
            if (!_sceneCollections.ContainsKey(sceneContainingfolder))
                _sceneCollections[sceneContainingfolder] = new();
            if (!_foldOutDict.ContainsKey(sceneContainingfolder))
                _foldOutDict[sceneContainingfolder] = true;

            List<string> paths = _sceneCollections[sceneContainingfolder];
            if (!paths.Contains(scenePath))
                paths.Add(scenePath);
        }

        // Remove any unused foldout menus (allows for folder renaming or asset reorganizing with proper updates)
        for (int i = _sceneCollections.Count - 1; i >= 0; i--)
            if (_sceneCollections.ElementAt(i).Value.Count == 0)
            {
                _sceneCollections.Remove(_sceneCollections.ElementAt(i).Key);
                _foldOutDict.Remove(_foldOutDict.ElementAt(i).Key);
            }

        // Draw foldouts for each scene asset containing folder with button elements that
        // trigger a scene swap for thie respective scenes and "highlight" current scene name
        foreach (var sceneKVP in _sceneCollections)
        {
            _foldOutDict[sceneKVP.Key] = EditorGUILayout.Foldout(_foldOutDict[sceneKVP.Key], sceneKVP.Key);
            
            if (_foldOutDict[sceneKVP.Key])
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    foreach (var scenePath in sceneKVP.Value)
                    {
                        string[] pathComponents = scenePath.Split('/');
                        string sceneName = pathComponents[^1].Split(".")[0];

                        if (SceneManager.GetActiveScene().name == sceneName) 
                        {
                            GUILayout.Space(-5);
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); //line seperator
                            GUILayout.Space(-10);
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(25);

                            if (SceneManager.GetActiveScene().name != sceneName)
                            {
                                if (GUILayout.Button(sceneName))
                                    if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                        EditorSceneManager.OpenScene(scenePath);
                                
                                GUILayout.FlexibleSpace();
                                continue;
                            }
                            GUILayout.Label("> " + sceneName + " <");
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.Space(-10);
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); //line seperator
                        GUILayout.Space(-5);
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
