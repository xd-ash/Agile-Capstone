using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider _playerHealthSlider;
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _playerShieldText;
    [SerializeField] private Slider _playerShieldSlider; // optional visual bar overlay (set max to player's maxHealth by default)

    [Header("Unit")]
    [SerializeField] private Unit _player;
    [SerializeField] private Unit _enemy;

    [Header("Turn")]
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _apText;

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

        if (_playerHealthSlider != null && _player != null)
        {
            _playerHealthSlider.maxValue = _player.GetMaxHealth;
            _playerHealthSlider.value    = Mathf.Clamp(_player.GetHealth, 0, _player.GetMaxHealth);

            _playerHealthText.text = $"Player Health: {_player.GetHealth}/{_player.GetMaxHealth}";
        }

        // Initialize shield UI
        if (_playerShieldSlider != null && _player != null)
        {
            // default shield bar max is player's max health (adjust in inspector if you use different scale)
            _playerShieldSlider.maxValue = _player.GetMaxHealth;
            _playerShieldSlider.gameObject.SetActive(_player.GetShield() > 0);
            _playerShieldSlider.value = _player.GetShield();
        }

        if (_playerShieldText != null)
            _playerShieldText.gameObject.SetActive(_player != null && _player.GetShield() > 0);
    }

    public void UpdateApText(Team unitTeam = Team.Friendly)
    {
        Unit curUnit = TurnManager.GetCurrentUnit;
        if (_apText == null) return;
        if (TurnManager.IsPlayerTurn && curUnit != null)
            _apText.text = $"Player AP:\n{curUnit.GetAP}/{curUnit.GetMaxAP}";
        else if (TurnManager.IsEnemyTurn && curUnit != null)
            _apText.text = $"Enemy AP:\n{curUnit.GetAP}/{curUnit.GetMaxAP}";
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
        ShieldEvents.OnEnemyShieldChanged  += UpdateEnemyShield;

        AbilityEvents.OnAbilityUsed += UpdateApText;
    }

    private void OnDisable()
    {
        DamageEvents.OnPlayerDamaged -= UpdatePlayerHealth;

        ShieldEvents.OnPlayerShieldChanged -= UpdatePlayerShield;
        ShieldEvents.OnEnemyShieldChanged  -= UpdateEnemyShield;
    }

    private void UpdatePlayerHealth(int current, int max)
    {
        if (_playerHealthSlider == null) return;
        if (_playerHealthSlider.maxValue != max) _playerHealthSlider.maxValue = max;
        _playerHealthSlider.value = Mathf.Clamp(current, 0, max);

        _playerHealthText.text = $"Player Health: {current}/{max}";
    }

    private void UpdatePlayerShield(int current)
    {
        if (_playerShieldSlider != null)
        {
            _playerShieldSlider.value = Mathf.Clamp(current, 0, (int)_playerShieldSlider.maxValue);
            _playerShieldSlider.gameObject.SetActive(current > 0);
        }

        if (_playerShieldText != null)
        {
            _playerShieldText.text = current > 0 ? $"Shield: {current}" : string.Empty;
            _playerShieldText.gameObject.SetActive(current > 0);
        }
    }

    private void UpdateEnemyShield(int current)
    {
        // Optionally add enemy shield UI; for now we don't have slider/text for enemy shield.
    }
}