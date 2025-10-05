using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private Slider enemyHealthSlider;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private Unit player;
    [SerializeField] private Unit enemy;

    private void Awake()
    {
        if (playerHealthSlider != null && player != null)
        {
            playerHealthSlider.maxValue = player.maxHealth;
            playerHealthSlider.value    = Mathf.Clamp(player.health, 0, player.maxHealth);

            playerHealthText.text = $"Player Health: {player.health}/{player.maxHealth}";
        }

        if (enemyHealthSlider != null && enemy != null)
        {
            enemyHealthSlider.maxValue = enemy.maxHealth;
            enemyHealthSlider.value    = Mathf.Clamp(enemy.health, 0, enemy.maxHealth);
            enemyHealthSlider.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        DamageEvents.OnPlayerDamaged += UpdatePlayerHealth;
        DamageEvents.OnEnemyDamaged  += UpdateEnemyHealth;
    }

    private void OnDisable()
    {
        DamageEvents.OnPlayerDamaged -= UpdatePlayerHealth;
        DamageEvents.OnEnemyDamaged  -= UpdateEnemyHealth;
    }

    private void UpdatePlayerHealth(int current, int max)
    {
        if (playerHealthSlider == null) return;
        if (playerHealthSlider.maxValue != max) playerHealthSlider.maxValue = max;
        playerHealthSlider.value = Mathf.Clamp(current, 0, max);

        playerHealthText.text = $"Player Health: {current}/{max}";
    }

    private void UpdateEnemyHealth(int current, int max)
    {
        if (enemyHealthSlider == null) return;
        if (enemyHealthSlider.maxValue != max) enemyHealthSlider.maxValue = max;
        enemyHealthSlider.value = Mathf.Clamp(current, 0, max);

        if (enemyHealthSlider.value != enemyHealthSlider.maxValue && !enemyHealthSlider.gameObject.activeInHierarchy)
            enemyHealthSlider.gameObject.SetActive(true); // Adam added 10-5, enemy bar hidden by default, show once dmg taken
    }
}