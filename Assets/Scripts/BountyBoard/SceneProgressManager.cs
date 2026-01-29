using UnityEngine;

public class SceneProgressManager : MonoBehaviour
{
    private string _mapSceneName = "NodeMap";
    private int _nodeCount = 4;
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

        var pdm = PlayerDataManager.Instance;
        //Initialize arrays once
        if (pdm.GetNodeCompleted == null || pdm.GetNodeCompleted.Length != _nodeCount)
        {
            var nodesUnlocked = new bool[_nodeCount];
            nodesUnlocked[0] = _nodeCount > 0;//By default, unlock node 0

            pdm.UpdateNodeData(new bool[_nodeCount], nodesUnlocked, -1);
        }
    }
    public void ResetNodes()
    {
        var pdm = PlayerDataManager.Instance;

        var nodesUnlocked = new bool[_nodeCount];
        nodesUnlocked[0] = _nodeCount > 0;//By default, unlock node 0

        pdm.UpdateNodeData(new bool[_nodeCount], nodesUnlocked, -1);

        _nodeMapCompleted = false;
    }

    public bool IsNodeCompleted(int index)
    {
        return index >= 0 && index < _nodeCount && PlayerDataManager.Instance.GetNodeCompleted[index];
    }

    public bool IsNodeUnlocked(int index)
    {
        return index >= 0 && index < _nodeCount && PlayerDataManager.Instance.GetNodeUnlocked[index];
    }

    //Called by a map node button when you click it
    public void EnterNode(int index, string sceneName)
    {
        if (!IsNodeUnlocked(index))
        {
            Debug.Log("Tried to enter locked node " + index);
            return;
        }

        PlayerDataManager.Instance.UpdateNodeData(index);

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
        var pdm = PlayerDataManager.Instance;

        if (pdm.GetCurrentNodeIndex < 0 || pdm.GetCurrentNodeIndex >= _nodeCount)
        {
            Debug.LogWarning("CompleteCurrentNode called with invalid currentNodeIndex " + pdm.GetCurrentNodeIndex);
            return;
        }

        bool[] tempNodeCompleted = pdm.GetNodeCompleted;
        bool[] tempNodeUnlocked = pdm.GetNodeUnlocked;

        tempNodeCompleted[pdm.GetCurrentNodeIndex] = true;

        int next = pdm.GetCurrentNodeIndex + 1;
        if (next >= 0 && next < _nodeCount)
            tempNodeUnlocked[next] = true;

        _nodeMapCompleted = next == _nodeCount;

        pdm.UpdateNodeData(tempNodeCompleted, tempNodeUnlocked);
    }

    public void ReturnToMap()
    {
        if (string.IsNullOrEmpty(_mapSceneName))
        {
            Debug.LogError("SceneProgressManager: mapSceneName is empty.");
            return;
        }
        SaveLoadScript.SaveGame?.Invoke();
        TransitionScene.instance.StartTransition(_mapSceneName);
    }
}