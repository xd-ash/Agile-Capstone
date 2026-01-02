using UnityEngine;
using UnityEngine.UI;

public class MapNodeButton : MonoBehaviour
{
    // these need proper systems instead of inspector assignment
    [SerializeField] private int _nodeIndex; 
    [SerializeField] private string _targetSceneName;
    //

    private Button _button;
    private Image _background;

    private void Start()
    {
        if (TryGetComponent(out _button))
            _button?.onClick.AddListener(OnClicked);
        else
            Debug.LogWarning("MapNodeButton on " + gameObject.name + " has no Button component.");

        RefreshVisual();
    }

    private void OnEnable()
    {
        _button = GetComponent<Button>();
        _background = GetComponent<Image>();

        //When the map scene reloads, update visuals
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (SceneProgressManager.Instance == null || _button == null) return;

        bool completed = SceneProgressManager.Instance.IsNodeCompleted(_nodeIndex);
        bool unlocked = SceneProgressManager.Instance.IsNodeUnlocked(_nodeIndex);

        _button.interactable = unlocked && !completed;

        if (_background == null) return;

        Color c;
        if (completed)
            c = Color.gray;
        else if (!unlocked)
            c = new Color(0.25f, 0.25f, 0.25f); // locked
        else
            c = Color.white;

        _background.color = c;
    }

    private void OnClicked()
    {
        SceneProgressManager.Instance?.EnterNode(_nodeIndex, _targetSceneName);
    }
}