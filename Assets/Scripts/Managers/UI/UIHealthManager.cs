using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider _playerHealthSlider;
    //[SerializeField] private Slider enemyHealthSlider;
    [SerializeField] private TextMeshProUGUI _playerHealthText;
    [SerializeField] private TextMeshProUGUI _playerShieldText;
    [SerializeField] private Slider _playerShieldSlider; // optional visual bar overlay (set max to player's maxHealth by default)
    [SerializeField] private Unit _player;
    [SerializeField] private Unit _enemy;

    private void Awake()
    {
        if (_playerHealthSlider != null && _player != null)
        {
            _playerHealthSlider.maxValue = _player.GetMaxHealth;
            _playerHealthSlider.value    = Mathf.Clamp(_player.GetHealth, 0, _player.GetMaxHealth);

            _playerHealthText.text = $"Player Health: {_player.GetHealth}/{_player.GetMaxHealth}";
        }

        /*
        if (enemyHealthSlider != null && enemy != null)
        {
            enemyHealthSlider.maxValue = enemy.maxHealth;
            enemyHealthSlider.value    = Mathf.Clamp(enemy.health, 0, enemy.maxHealth);
            enemyHealthSlider.gameObject.SetActive(false);
        }
        */

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

    private void OnEnable()
    {
        DamageEvents.OnPlayerDamaged += UpdatePlayerHealth;
        //DamageEvents.OnEnemyDamaged  += UpdateEnemyHealth;

        ShieldEvents.OnPlayerShieldChanged += UpdatePlayerShield;
        ShieldEvents.OnEnemyShieldChanged  += UpdateEnemyShield;
    }

    private void OnDisable()
    {
        DamageEvents.OnPlayerDamaged -= UpdatePlayerHealth;
        //DamageEvents.OnEnemyDamaged  -= UpdateEnemyHealth;

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

    /*
    private void UpdateEnemyHealth(int current, int max)
    {
        if (enemyHealthSlider == null) return;
        if (enemyHealthSlider.maxValue != max) enemyHealthSlider.maxValue = max;
        enemyHealthSlider.value = Mathf.Clamp(current, 0, max);

        if (enemyHealthSlider.value != enemyHealthSlider.maxValue && !enemyHealthSlider.gameObject.activeInHierarchy)
            enemyHealthSlider.gameObject.SetActive(true); // Adam added 10-5, enemy bar hidden by default, show once dmg taken
    }
    */
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