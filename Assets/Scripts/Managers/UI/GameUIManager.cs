using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider _playerHealthSlider;
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _playerShieldText;
    [SerializeField] private Slider _playerShieldSlider;

    [Header("Turn")]
    [SerializeField] private TextMeshProUGUI _turnText;
    
    [Header("Win/Loss")]
    [SerializeField] private GameObject winText;
    [SerializeField] private GameObject loseText;

    public static GameUIManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    
    public void UpdateTurnText(TurnManager.Turn curTurn)
    {
        if (_turnText != null)
            _turnText.text = $"{curTurn}'s Turn";
    }

    public void ToggleWinLossText(bool didWin)
    {
        GameObject text = didWin ? winText : loseText;
        text?.SetActive(true);
    }

    private void OnEnable()
    {
        DamageEvents.OnPlayerDamaged += UpdatePlayerHealth;
        ShieldEvents.OnPlayerShieldChanged += UpdatePlayerShield;
        
    }

    private void OnDisable()
    {
        DamageEvents.OnPlayerDamaged -= UpdatePlayerHealth;
        ShieldEvents.OnPlayerShieldChanged -= UpdatePlayerShield;
    }

    private void UpdatePlayerHealth(int current, int max)
    {
        if (_playerHealthSlider == null) return;
        if (_playerHealthSlider.maxValue != max) _playerHealthSlider.maxValue = max;
        _playerHealthSlider.value = Mathf.Clamp(current, 0, max);

        if (_playerHealthText == null) return;
        _playerHealthText.text = $"Player Health: {current}/{max}";
    }

    private void UpdatePlayerShield(int current)
    {
        if (_playerShieldSlider == null) return;
        if (_playerShieldSlider.maxValue != _playerHealthSlider.maxValue)
            _playerShieldSlider.maxValue = _playerHealthSlider.maxValue;

        _playerShieldSlider.value = Mathf.Clamp(current, 0, (int)_playerShieldSlider.maxValue);
        _playerShieldSlider.gameObject.SetActive(current > 0);

        if (_playerShieldText == null) return;
        _playerShieldText.gameObject.SetActive(current > 0);
        _playerShieldText.text = current > 0 ? $"Shield: {current}" : string.Empty;
    }
}