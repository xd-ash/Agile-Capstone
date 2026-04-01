using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private string _prefix = "";
    [SerializeField] private string _suffix = "";

    private void Awake()
    {
        if (_label == null) _label = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnBalanceChanged += UpdateDisplay;
            UpdateDisplay(PlayerDataManager.Instance.GetBalance);
        }
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnBalanceChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int newBalance)
    {
        if (_label == null) return;
        _label.text = $"{_prefix}{newBalance}{_suffix}";
    }
}