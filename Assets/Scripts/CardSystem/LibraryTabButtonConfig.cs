using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LibraryTabButtonConfig : MonoBehaviour
{
    public Vector2 activeSize;
    public Vector2 inactiveSize;
    public bool toggleOnEnable;

    [SerializeField, Space(10)] private GameObject _scrollView;
    [SerializeField, Range(0f, 1f)] private float _scrollViewAlphaMultiplier;

    private Button _button;
    private TextMeshProUGUI _tabText;
    RectTransform _transform;
    private Image _scrollViewImage;

    private void Awake()
    {
        _transform = GetComponent<RectTransform>();
        _button = GetComponent<Button>();
        _tabText = GetComponentInChildren<TextMeshProUGUI>(true);
        _scrollViewImage = _scrollView?.GetComponent<Image>();
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

        if (!isActive) return;

        if (_scrollViewImage == null)
            _scrollViewImage = _scrollView?.GetComponent<Image>();
        if (_scrollViewImage != null)
            _scrollViewImage.color =  new Color(_button.image.color.r, _button.image.color.g, _button.image.color.b, _scrollViewAlphaMultiplier);
    }
}
