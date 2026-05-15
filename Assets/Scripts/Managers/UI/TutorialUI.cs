using TMPro;
using UnityEngine;

// Handles showing/hiding the tutorial message box UI
public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private TextMeshProUGUI _apDiscaimerText;

    public void Show(string message, bool showAPDisclaimer = true)
    {
        _messageText.text = message;
        _apDiscaimerText?.gameObject?.SetActive(showAPDisclaimer);
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }
}