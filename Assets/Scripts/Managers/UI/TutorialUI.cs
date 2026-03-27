using TMPro;
using UnityEngine;

// Handles showing/hiding the tutorial message box UI
public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _messageText;

    public void Show(string message)
    {
        _messageText.text = message;
        _panel.SetActive(true);
    }

    public void Hide()
    {
        _panel.SetActive(false);
    }
}