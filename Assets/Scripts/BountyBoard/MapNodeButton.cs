using UnityEngine;
using UnityEngine.UI;

public class MapNodeButton : MonoBehaviour
{
    public int nodeIndex;
    public string targetSceneName;

    [Header("UI References")]
    public Button button;
    public Image background;

    private void Start()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnClicked);
        }

        RefreshVisual();
    }

    private void OnEnable()
    {
        //When the map scene reloads, update visuals
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (SceneProgressManager.Instance == null || button == null)
        {
            return;
        }

        bool completed = SceneProgressManager.Instance.IsNodeCompleted(nodeIndex);
        bool unlocked = SceneProgressManager.Instance.IsNodeUnlocked(nodeIndex);

        button.interactable = unlocked && !completed;

        if (background != null)
        {
            Color c;

            if (completed)
            {
                c = Color.gray;
            }
            else if (!unlocked)
            {
                c = new Color(0.25f, 0.25f, 0.25f); // locked
            }
            else
            {
                c = Color.white;
            }

            background.color = c;
        }
    }

    private void OnClicked()
    {
        if (SceneProgressManager.Instance == null)
        {
            return;
        }

        SceneProgressManager.Instance.EnterNode(nodeIndex, targetSceneName);
    }
}