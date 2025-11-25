using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopConfirmPopup : MonoBehaviour
{
    public static ShopConfirmPopup Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;
    private Action _onCancel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.gameObject.SetActive(false);
        }

        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmPressed);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelPressed);
    }

    /// <summary>
    /// Show the confirmation popup.
    /// </summary>
    /// <param name="price">Price to display</param>
    /// <param name="message">Message (e.g. card name)</param>
    /// <param name="onConfirm">Action executed when the player confirms</param>
    /// <param name="onCancel">Action executed when the player cancels</param>
    public void Show(int price, string message, Action onConfirm = null, Action onCancel = null)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;

        if (titleText != null)
            titleText.text = "Confirm Purchase";

        if (messageText != null)
            messageText.text = message;

        if (priceText != null)
            priceText.text = price.ToString();

        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
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
        if (canvasGroup == null) return;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.gameObject.SetActive(false);
        _onConfirm = null;
        _onCancel = null;
    }
}