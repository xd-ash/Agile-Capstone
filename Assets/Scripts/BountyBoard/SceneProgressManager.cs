using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneProgressManager : MonoBehaviour
{
    public static SceneProgressManager Instance { get; private set; }

    public string mapSceneName = "NodeMap";
    public int nodeCount = 5;

    //Progress data
    private bool[] nodeCompleted;
    private bool[] nodeUnlocked;
    private int currentNodeIndex = -1;

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
        if (nodeCompleted == null || nodeCompleted.Length != nodeCount)
        {
            nodeCompleted = new bool[nodeCount];
            nodeUnlocked = new bool[nodeCount];

            //By default, unlock node 0
            if (nodeCount > 0)
            {
                nodeUnlocked[0] = true;
            }
        }
    }

    //Use in Save/Load script
    public void GrabNodeData(ref bool[] completedNodes, ref bool[] unlockedNodes, ref int curNodeIndex)
    {
        completedNodes = nodeCompleted;
        unlockedNodes = nodeUnlocked;
        curNodeIndex = currentNodeIndex;
    }
    public void LoadNodeData(bool[] completedNodes, bool[] unlockedNodes, int curNodeIndex)
    {
        nodeCompleted = completedNodes;
        nodeUnlocked = unlockedNodes;
        currentNodeIndex = curNodeIndex;
    }
    //
    public bool IsNodeCompleted(int index)
    {
        return index >= 0 && index < nodeCount && nodeCompleted[index];
    }

    public bool IsNodeUnlocked(int index)
    {
        return index >= 0 && index < nodeCount && nodeUnlocked[index];
    }

    //Called by a map node button when you click it
    public void EnterNode(int index, string sceneName)
    {
        if (!IsNodeUnlocked(index))
        {
            Debug.Log("Tried to enter locked node " + index);
            return;
        }

        currentNodeIndex = index;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneProgressManager: target scene name is empty for node " + index);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    //Called when shop or a bounty is finished
    public void CompleteCurrentNode()
    {
        if (currentNodeIndex < 0 || currentNodeIndex >= nodeCount)
        {
            Debug.LogWarning("CompleteCurrentNode called with invalid currentNodeIndex " + currentNodeIndex);
            return;
        }

        nodeCompleted[currentNodeIndex] = true;

        int next = currentNodeIndex + 1;
        if (next >= 0 && next < nodeCount)
        {
            nodeUnlocked[next] = true;
        }
    }

    public void ReturnToMap()
    {
        if (string.IsNullOrEmpty(mapSceneName))
        {
            Debug.LogError("SceneProgressManager: mapSceneName is empty.");
            return;
        }

        SceneManager.LoadScene(mapSceneName);
    }
}