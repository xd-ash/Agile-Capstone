using System;
using System.Collections;
using AStarPathfinding;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;

public enum Team {Friendly, Enemy}
public class Unit : MonoBehaviour, IDamagable
{
    [Header("Team and stats")] 
    public Team team;
    public int maxHealth;
    public int health;

    [Header("Shield")]
    [SerializeField] private int shield = 0; // current shield amount (absorb damage before health)

    //[Header("Target for Enemy units")]
    //[SerializeField] private Unit _target;

    [Header("Action Points")] 
    public int maxAP;
    public int ap;

    [Header("SFX")]
    public AudioClip stepSfx;
    public AudioClip damageSfx;
    public AudioClip shieldHitSfx;

    [Header("Placeholder Stuff")]
    [SerializeField] private Slider _enemyHPBar;

    private FindPathAStar _aStar;

    public event Action<Unit> OnApChanged;

    private void Awake()
    {
        health = maxHealth;
        ap = maxAP;
        RaiseHealthEvent();

        // Ensure UI gets initial shield state
        if (team == Team.Friendly)
            ShieldEvents.RaisePlayerShieldChanged(shield);
        else
        {
            _enemyHPBar = GetComponentInChildren<Slider>();
            //_enemyHPBar.gameObject.SetActive(false); // commented this out so enemy HP bar show from start
            ShieldEvents.RaiseEnemyShieldChanged(shield);
        }
    }
    private void Start()
    {
        _aStar = GetComponent<FindPathAStar>();

        //**********************************************************************//
        if (team != Team.Friendly) return;                                      //
        CardSystem.CardManager.instance.OnCardAblityCancel += StopAllCoroutines;// I SHOULD BE CHANGED TO A BETTER SYSTEM
        TurnManager.instance.OnPlayerTurnEnd += StopAllCoroutines;              // I SHOULD BE CHANGED TO A BETTER SYSTEM
        //**********************************************************************//
    }
    private void OnDestroy()
    {
        //**********************************************************************//
        if (team != Team.Friendly) return;                                      //
        CardSystem.CardManager.instance.OnCardAblityCancel -= StopAllCoroutines;// I SHOULD BE CHANGED TO A BETTER SYSTEM
        TurnManager.instance.OnPlayerTurnEnd -= StopAllCoroutines;              // I SHOULD BE CHANGED TO A BETTER SYSTEM
        //**********************************************************************//
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

            if (shield > 0)
            {
                AudioManager.instance?.PlaySFX(shieldHitSfx);

                int absorbed = Mathf.Min(shield, remainingDamage);
                shield -= absorbed;
                remainingDamage -= absorbed;
                //Debug.Log($"[{team}] '{name}' shield absorbed {absorbed} damage (shield remaining: {shield}).");

                // Notify UI about shield change
                if (team == Team.Friendly)
                    ShieldEvents.RaisePlayerShieldChanged(shield);
                else
                    ShieldEvents.RaiseEnemyShieldChanged(shield);
            }

            if (remainingDamage > 0)
            {
                health -= remainingDamage;
                AudioManager.instance?.PlaySFX(damageSfx);
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
            health += uAmount;
            //Debug.Log($"[{team}] '{name}' healed {uAmount}. Health now {health}/{maxHealth}.");
        }

        // Clamp and death handling
        if (health >= maxHealth)
            health = maxHealth;
        else if (health <= 0)
        {
            health = 0;

            //Temp Win/Loss condition stuff
            //
            if (team == Team.Friendly)
                GameOverEvents.OnGameWinOrLoss(false);
            else
            {
                WinLossManager.instance.enemyUnits.Remove(this);
                if (WinLossManager.instance.enemyUnits.Count == 0)
                    GameOverEvents.OnGameWinOrLoss(true);
            }
            //

            Destroy(gameObject); // TODO: replace with proper on-death handling if needed
            //Debug.Log($"[{team}] '{name}' unit died");
        }

        //Placeholder enemy healthbar updating
        if (team == Team.Enemy)
            UpdateHealthBar();

        RaiseHealthEvent();
    }

    /// <summary>
    /// Add shield amount. If duration > 0, shield amount will be removed after duration seconds.
    /// </summary>
    public void AddShield(int amount, float duration = 0f)
    {
        if (amount <= 0) return;
        shield += amount;
        //Debug.Log($"[{team}] '{name}' gained {amount} shield (total shield: {shield}).");

        // Raise shield event for UI
        if (team == Team.Friendly)
            ShieldEvents.RaisePlayerShieldChanged(shield);
        else
            ShieldEvents.RaiseEnemyShieldChanged(shield);

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
        int removed = Mathf.Min(shield, amount);
        shield -= removed;
        //Debug.Log($"[{team}] '{name}' lost {removed} shield (remaining shield: {shield}).");

        // Raise shield event for UI
        if (team == Team.Friendly)
            ShieldEvents.RaisePlayerShieldChanged(shield);
        else
            ShieldEvents.RaiseEnemyShieldChanged(shield);
    }

    private IEnumerator RemoveShieldAfter(float seconds, int amount)
    {
        yield return new WaitForSeconds(seconds);
        RemoveShield(amount);
    }

    // Optional accessor if needed by UI
    public int GetShield() => shield;

    //placeholder enemy damage dealing
    public void DealDamage(Unit target, int damage = 2)
    {
        if (team == Team.Enemy && target != null)
            target.ChangeHealth(damage, false);
    }
    //placeholder enemy healthbar stuff
    public void UpdateHealthBar()
    {
        if (_enemyHPBar == null) return;
        if (_enemyHPBar.maxValue != maxHealth) _enemyHPBar.maxValue = maxHealth;
        _enemyHPBar.value = Mathf.Clamp(health, 0, maxHealth);

        if (_enemyHPBar.value != _enemyHPBar.maxValue && !_enemyHPBar.gameObject.activeInHierarchy)
            _enemyHPBar.gameObject.SetActive(true); // Adam added 10-5, enemy bar hidden by default, show once dmg taken
    }
    private void RaiseHealthEvent()
    {
        if (team == Team.Friendly)
            DamageEvents.RaisePlayerDamaged(health,maxHealth);
        else
            DamageEvents.RaiseEnemyDamaged(health, maxHealth);
    }

    public void RefreshAP()
    {
        ap = maxAP;
        OnApChanged?.Invoke(this);
    }

    public bool CanSpend(int cost) => ap >= cost;

    public bool SpendAP(int cost, bool spendNow = true)
    {
        if (!CanSpend(cost))
            return false;
        if (spendNow)
        {
            ap -= cost;
            OnApChanged?.Invoke(this);
        }
        return true;
    }

    public bool CanMove()
    {
        // Prevent movement if targeting is active
        if (AbilityEvents.IsTargeting)
        {
            return false;
        }
        return true; // Or your existing movement conditions
    }
}