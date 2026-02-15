using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardSystem;

public enum Team {Friendly, Enemy}
public class Unit : MonoBehaviour, IDamagable
{
    [Header("Team and stats")] 
    [SerializeField] private Team _team;
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _health;

    [Header("Shield")]
    [SerializeField] private int _maxShield = 25;
    [SerializeField] private int _shield = 0; // current shield amount (absorb damage before health)

    [Header("Action Points")]
    [SerializeField] private int _maxAP;
    [SerializeField] private int _ap;

    //[Header("SFX")]
    //public AudioClip stepSfx;
    [SerializeField] private AudioClip _damageSfx;
    [SerializeField] private AudioClip _shieldHitSfx;

    [Header("Placeholder Stuff")]
    [SerializeField] private Slider _enemyHPBar;
    [SerializeField] private TextMeshProUGUI _hitChanceText;

    private Coroutine _targetingCoroutine;

    public Team GetTeam => _team;
    public int GetMaxHealth => _maxHealth;
    public int GetHealth => _health;
    public int GetMaxAP => _maxAP;
    public int GetAP => _ap;

    public event Action<Unit> OnApChanged;

    private void Awake()
    {
        _health = _maxHealth;
        _ap = _maxAP;
        RaiseHealthEvent();
        HideHitChance();

        // Ensure UI gets initial shield state
        if (_team == Team.Friendly)
            ShieldEvents.RaisePlayerShieldChanged(_shield);
        else
        {
            _enemyHPBar = GetComponentInChildren<Slider>();
            //_enemyHPBar.gameObject.SetActive(false); // commented this out so enemy HP bar show from start
            ShieldEvents.RaiseEnemyShieldChanged(_shield);
        }
    }
    private void Start()
    {
        if (_team != Team.Friendly) return;
        DeckAndHandManager.Instance.OnCardAblityCancel += () => StopCoroutine(_targetingCoroutine);
        TurnManager.Instance.OnPlayerTurnEnd += () => StopCoroutine(_targetingCoroutine);
    }
    private void OnDestroy()
    {
        if (_team != Team.Friendly) return;
        DeckAndHandManager.Instance.OnCardAblityCancel -= () => StopTargetingCoro();
        TurnManager.Instance.OnPlayerTurnEnd -= () => StopTargetingCoro();
    }
    private void StopTargetingCoro()
    {
        if (_team != Team.Friendly || _targetingCoroutine == null) return;

        StopCoroutine(_targetingCoroutine);
    }
    /// <summary>
    /// ChangeHealth handles both healing (isGain = true) and damage (isGain = false).
    /// When taking damage, shield is consumed first (if >0).
    /// </summary>
    public void ChangeHealth(int amount, bool isGain)
    {
        int uAmount = Math.Abs(amount);

        if (!isGain)
        {
            // Apply damage: shield absorbs first
            int remainingDamage = uAmount;

            if (_shield > 0)
            {
                AudioManager.Instance?.PlaySFX(_shieldHitSfx);

                int absorbed = Mathf.Min(_shield, remainingDamage);
                _shield -= absorbed;
                remainingDamage -= absorbed;
                //Debug.Log($"[{team}] '{name}' shield absorbed {absorbed} damage (shield remaining: {shield}).");

                // Notify UI about shield change
                if (_team == Team.Friendly)
                    ShieldEvents.RaisePlayerShieldChanged(_shield);
                else
                    ShieldEvents.RaiseEnemyShieldChanged(_shield);
            }

            if (remainingDamage > 0)
            {
                _health -= remainingDamage;
                AudioManager.Instance?.PlaySFX(_damageSfx);
                //Debug.Log($"[{team}] '{name}' took {remainingDamage} damage (post-shield). Health now {health}/{maxHealth}.");
            }
            else
            {
                //Debug.Log($"[{team}] '{name}' took no health damage thanks to shield.");
            }
        }
        else
        {
            // Healing path
            _health += uAmount;
            //Debug.Log($"[{team}] '{name}' healed {uAmount}. Health now {health}/{maxHealth}.");
        }

        // Clamp and death handling
        if (_health >= _maxHealth)
            _health = _maxHealth;
        else if (_health <= 0)
        {
            _health = 0;

            //Temp Win/Loss condition stuff
            //
            if (_team == Team.Friendly)
                GameOverEvents.OnGameWinOrLoss(false);
            else
            {
                WinLossManager.Instance.GetEnemyUnits.Remove(this);
                if (WinLossManager.Instance.GetEnemyUnits.Count == 0)
                    GameOverEvents.OnGameWinOrLoss(true);
            }
            //

            MapCreator.Instance.UpdateUnitPositionByteMap(IsoMetricConversions.ConvertToGridFromIsometric(transform.localPosition));
            Destroy(gameObject);
            //Debug.Log($"[{team}] '{name}' unit died");
        }

        //Placeholder enemy healthbar updating
        if (_team == Team.Enemy)
            UpdateHealthBar();

        RaiseHealthEvent();
    }

    /// <summary>
    /// Add shield amount. If duration > 0, shield amount will be removed after duration seconds.
    /// </summary>
    public void AddShield(int amount, float duration = 0f)
    {
        if (amount <= 0) return;
        _shield += amount;
        if (_shield >= _maxShield)
            _shield = _maxShield;
        //Debug.Log($"[{team}] '{name}' gained {amount} shield (total shield: {shield}).");

        // Raise shield event for UI
        if (_team == Team.Friendly)
            ShieldEvents.RaisePlayerShieldChanged(_shield);
        else
            ShieldEvents.RaiseEnemyShieldChanged(_shield);

        if (duration > 0f)
        {
            StartCoroutine(RemoveShieldAfter(duration, amount));
        }
    }

    /// <summary>
    /// Remove up to `amount` from shield immediately.
    /// </summary>
    public void RemoveShield(int amount)
    {
        if (amount <= 0) return;
        int removed = Mathf.Min(_shield, amount);
        _shield -= removed;
        //Debug.Log($"[{team}] '{name}' lost {removed} shield (remaining shield: {shield}).");

        // Raise shield event for UI
        if (_team == Team.Friendly)
            ShieldEvents.RaisePlayerShieldChanged(_shield);
        else
            ShieldEvents.RaiseEnemyShieldChanged(_shield);
    }

    private IEnumerator RemoveShieldAfter(float seconds, int amount)
    {
        yield return new WaitForSeconds(seconds);
        RemoveShield(amount);
    }

    // Optional accessor if needed by UI
    public int GetShield() => _shield;

    //placeholder enemy damage dealing
    public void DealDamage(Unit target, int damage = 2)
    {
        if (_team == Team.Enemy && target != null)
            target.ChangeHealth(damage, false);
    }
    //placeholder enemy healthbar stuff
    public void UpdateHealthBar()
    {
        if (_enemyHPBar == null) return;
        if (_enemyHPBar.maxValue != _maxHealth) _enemyHPBar.maxValue = _maxHealth;
        _enemyHPBar.value = Mathf.Clamp(_health, 0, _maxHealth);

        if (_enemyHPBar.value != _enemyHPBar.maxValue && !_enemyHPBar.gameObject.activeInHierarchy)
            _enemyHPBar.gameObject.SetActive(true); // Adam added 10-5, enemy bar hidden by default, show once dmg taken
    }
    private void RaiseHealthEvent()
    {
        if (_team == Team.Friendly)
            DamageEvents.RaisePlayerDamaged(_health, _maxHealth);
        else
            DamageEvents.RaiseEnemyDamaged(_health, _maxHealth);
    }

    public void RefreshAP()
    {
        _ap = _maxAP;
        OnApChanged?.Invoke(this);
    }

    public bool CanSpend(int cost) => _ap >= cost;

    public bool SpendAP(int cost, bool spendNow = true)
    {
        if (!CanSpend(cost))
            return false;
        if (spendNow)
        {
            _ap -= cost;
            OnApChanged?.Invoke(this);
        }
        return true;
    }

    public bool CanMove()
    {
        
        // Block movement if the game is paused
        if (PauseMenu.isPaused)
        {
            return false;
        }
        
        // Prevent movement if targeting is active
        if (AbilityEvents.IsTargeting)
        {
            return false;
        }
        return true; // Or your existing movement conditions
    }

    public void ShowHitChance(int hitChance)
    {
        if (_hitChanceText == null)
        {
            return;
        }

        _hitChanceText.gameObject.SetActive(true);
        _hitChanceText.text = $"{hitChance}%";
    }

    public void HideHitChance()
    {
        if (_hitChanceText == null)
        {
            return;
        }

        _hitChanceText.gameObject.SetActive(false);
    }
    public void StartTargetingCoroutine(IEnumerator targetingCoro)
    {
        _targetingCoroutine = StartCoroutine(targetingCoro);
    }
}