using UnityEngine;

public class SceneProgressManager : MonoBehaviour
{
    private string _mapSceneName = "NodeMap";
    private int _nodeCount = 4;

    //Progress data
    private bool[] _nodeCompleted;
    private bool[] _nodeUnlocked;
    private int _currentNodeIndex = -1;
    private bool _nodeMapCompleted = false;

    public bool GetNodeMapCompleted => _nodeMapCompleted;

    public static SceneProgressManager Instance { get; private set; }
    private void Awake()
    {
        //Basic singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //Initialize arrays once
        if (_nodeCompleted == null || _nodeCompleted.Length != _nodeCount)
        {
            _nodeCompleted = new bool[_nodeCount];
            _nodeUnlocked = new bool[_nodeCount];

            //By default, unlock node 0
            if (_nodeCount > 0)
                _nodeUnlocked[0] = true;
        }
    }
    public void ResetNodes()
    {
        _currentNodeIndex = -1;

        _nodeCompleted = new bool[_nodeCount];
        _nodeUnlocked = new bool[_nodeCount];

        //By default, unlock node 0
        if (_nodeCount > 0)
            _nodeUnlocked[0] = true;

        _nodeMapCompleted = false;
    }
    //Use in Save/Load script
    public void GrabNodeData(ref bool[] completedNodes, ref bool[] unlockedNodes, ref int curNodeIndex)
    {
        completedNodes = _nodeCompleted;
        unlockedNodes = _nodeUnlocked;
        curNodeIndex = _currentNodeIndex;
    }
    public void LoadNodeData(bool[] completedNodes, bool[] unlockedNodes, int curNodeIndex)
    {
        _nodeCompleted = completedNodes;
        _nodeUnlocked = unlockedNodes;
        _currentNodeIndex = curNodeIndex;
    }
    //
    public bool IsNodeCompleted(int index)
    {
        return index >= 0 && index < _nodeCount && _nodeCompleted[index];
    }

    public bool IsNodeUnlocked(int index)
    {
        return index >= 0 && index < _nodeCount && _nodeUnlocked[index];
    }

    //Called by a map node button when you click it
    public void EnterNode(int index, string sceneName)
    {
        if (!IsNodeUnlocked(index))
        {
            Debug.Log("Tried to enter locked node " + index);
            return;
        }

        _currentNodeIndex = index;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneProgressManager: target scene name is empty for node " + index);
            return;
        }

        TransitionScene.instance.StartTransition(sceneName);
    }

    //Called when shop or a bounty is finished
    public void CompleteCurrentNode()
    {
        if (_currentNodeIndex < 0 || _currentNodeIndex >= _nodeCount)
        {
            Debug.LogWarning("CompleteCurrentNode called with invalid currentNodeIndex " + _currentNodeIndex);
            return;
        }

        _nodeCompleted[_currentNodeIndex] = true;

        int next = _currentNodeIndex + 1;
        if (next >= 0 && next < _nodeCount)
            _nodeUnlocked[next] = true;

        if (next == _nodeCount)
            _nodeMapCompleted = true;
    }

    public void ReturnToMap()
    {
        if (string.IsNullOrEmpty(_mapSceneName))
        {
            Debug.LogError("SceneProgressManager: mapSceneName is empty.");
            return;
        }

        TransitionScene.instance.StartTransition(_mapSceneName);
    }
}