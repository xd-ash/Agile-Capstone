using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LibraryTabButtonConfig : MonoBehaviour
{
    public Vector2 activeSize;
    public Vector2 inactiveSize;
    public bool toggleOnEnable;

    private Button _button;
    private TextMeshProUGUI _tabText;
    RectTransform _transform;

    private void Awake()
    {
        _transform = GetComponent<RectTransform>();
        _button = GetComponent<Button>();
        _tabText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void OnEnable()
    {
        if (toggleOnEnable)
            _button?.onClick?.Invoke();
    }

    public void SetActive(bool isActive)
    {
        _transform.sizeDelta = isActive ? activeSize : inactiveSize;
        _tabText.gameObject.SetActive(isActive);
    }
}
