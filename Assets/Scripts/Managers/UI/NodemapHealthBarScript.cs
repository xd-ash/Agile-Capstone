using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodemapHealthBarScript : MonoBehaviour
{
    private Slider _slider;
    private TextMeshProUGUI _hpText;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _hpText = GetComponentInChildren<TextMeshProUGUI>();

        UpdateNodeMapHealthBar();
    }

    public void UpdateNodeMapHealthBar()
    {
        if (_slider == null) return;
        _slider.maxValue = PlayerDataManager.Instance.GetMaxHealth;
        _slider.value = PlayerDataManager.Instance.GetCurrentHealth;

        if (_hpText == null) return;
        _hpText.text = $"Player Health: {PlayerDataManager.Instance.GetCurrentHealth}/{PlayerDataManager.Instance.GetMaxHealth}";
    }
}
