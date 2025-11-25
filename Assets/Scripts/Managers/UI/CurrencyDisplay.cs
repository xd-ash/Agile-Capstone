using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";

    private void Awake()
    {
        if (label == null) label = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.OnBalanceChanged += UpdateDisplay;
            UpdateDisplay(CurrencyManager.instance.Balance);
        }
    }

    private void OnDisable()
    {
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.OnBalanceChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int newBalance)
    {
        if (label == null) return;
        label.text = $"{prefix}{newBalance}{suffix}";
    }
}