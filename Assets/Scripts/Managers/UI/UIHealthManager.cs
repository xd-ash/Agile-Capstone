using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider playerHealthSlider;
    //[SerializeField] private Slider enemyHealthSlider;
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

        /*
        if (enemyHealthSlider != null && enemy != null)
        {
            enemyHealthSlider.maxValue = enemy.maxHealth;
            enemyHealthSlider.value    = Mathf.Clamp(enemy.health, 0, enemy.maxHealth);
            enemyHealthSlider.gameObject.SetActive(false);
        }
<<<<<<< Updated upstream
        */

        // Initialize shield UI
        if (playerShieldSlider != null && player != null)
        {
            // default shield bar max is player's max health (adjust in inspector if you use different scale)
            playerShieldSlider.maxValue = player.maxHealth;
            playerShieldSlider.gameObject.SetActive(player.GetShield() > 0);
            playerShieldSlider.value = player.GetShield();
        }

        if (playerShieldText != null)
            playerShieldText.gameObject.SetActive(player != null && player.GetShield() > 0);
=======
>>>>>>> Stashed changes
    }

    private void OnEnable()
    {
        DamageEvents.OnPlayerDamaged += UpdatePlayerHealth;
<<<<<<< Updated upstream
        //DamageEvents.OnEnemyDamaged  += UpdateEnemyHealth;

        ShieldEvents.OnPlayerShieldChanged += UpdatePlayerShield;
        ShieldEvents.OnEnemyShieldChanged  += UpdateEnemyShield;
=======
        DamageEvents.OnEnemyDamaged  += UpdateEnemyHealth;
>>>>>>> Stashed changes
    }

    private void OnDisable()
    {
        DamageEvents.OnPlayerDamaged -= UpdatePlayerHealth;
<<<<<<< Updated upstream
        //DamageEvents.OnEnemyDamaged  -= UpdateEnemyHealth;

        ShieldEvents.OnPlayerShieldChanged -= UpdatePlayerShield;
        ShieldEvents.OnEnemyShieldChanged  -= UpdateEnemyShield;
=======
        DamageEvents.OnEnemyDamaged  -= UpdateEnemyHealth;
>>>>>>> Stashed changes
    }

    private void UpdatePlayerHealth(int current, int max)
    {
        if (playerHealthSlider == null) return;
        if (playerHealthSlider.maxValue != max) playerHealthSlider.maxValue = max;
        playerHealthSlider.value = Mathf.Clamp(current, 0, max);

        playerHealthText.text = $"Player Health: {current}/{max}";
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
<<<<<<< Updated upstream
    */
    private void UpdatePlayerShield(int current)
    {
        if (playerShieldSlider != null)
        {
            playerShieldSlider.value = Mathf.Clamp(current, 0, (int)playerShieldSlider.maxValue);
            playerShieldSlider.gameObject.SetActive(current > 0);
        }

        if (playerShieldText != null)
        {
            playerShieldText.text = current > 0 ? $"Shield: {current}" : string.Empty;
            playerShieldText.gameObject.SetActive(current > 0);
        }
    }

    private void UpdateEnemyShield(int current)
    {
        // Optionally add enemy shield UI; for now we don't have slider/text for enemy shield.
    }
=======
>>>>>>> Stashed changes
}