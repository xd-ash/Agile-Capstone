using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopConfirmPopup : MonoBehaviour
{
    public static ShopConfirmPopup Instance { get; private set; }

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private Action _onConfirm;
    private Action _onCancel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (_canvasGroup == null)
            _canvasGroup = GetComponentInChildren<CanvasGroup>();

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.gameObject.SetActive(false);
        }

        if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirmPressed);
        if (_cancelButton != null) _cancelButton.onClick.AddListener(OnCancelPressed);
    }

    /// <summary>
    /// Show the confirmation popup.
    /// </summary>
    /// <param name="price">Price to display</param>
    /// <param name="message">Message (e.g. card name)</param>
    /// <param name="onConfirm">Action executed when the player confirms</param>
    /// <param name="onCancel">Action executed when the player cancels</param>
    public void Show(int price, string cardName, Action onConfirm = null, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (_messageText != null)
            _messageText.text = $"Buy \"{cardName}\" for {price}?";

        if (_priceText != null)
            _priceText.text = price.ToString();

        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }

    private void OnConfirmPressed()
    {
        _onConfirm?.Invoke();
        Hide();
    }

    private void OnCancelPressed()
    {
        _onCancel?.Invoke();
        Hide();
    }

    public void Hide()
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.gameObject.SetActive(false);
        _onConfirm = null;
        _onCancel = null;
    }
}