using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CardSystem;
using AStarPathfinding;

public enum Team {Friendly, Enemy, None, All}
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

    [Header("Placeholder Stuff")]
    [SerializeField] private Slider _enemyHPBar;
    [SerializeField] private Slider _enemyShieldBar;
    [SerializeField] private TextMeshProUGUI _hitChanceText;

    [Header("Misc")]
    [SerializeField] private bool _canMove = true;

    private FloatingTextController _floatingText;
    private Coroutine _targetingCoroutine;
    private Guid _unitGuid = new();

    public Team GetTeam => _team;
    public int GetMaxHealth => _maxHealth;
    public int GetHealth => _health;
    public int GetMaxShield => _maxShield;
    public int GetShield => _shield;
    public int GetEffectiveHealth => _health + _shield;
    public int GetMaxEffectiveHealth => _maxHealth + _maxShield;
    public int GetMaxAP => _maxAP;
    public int GetAP => _ap;
    public FloatingTextController GetFloatingText => _floatingText;
    public bool GetCanMove => _canMove;
    public bool GetIsMoving => TryGetComponent(out UnitMovementController unitMover) && unitMover.GetIsMoving;
    public Guid GetGuid => _unitGuid;

    public event Action<Unit> OnApChanged;

    private void Awake()
    {
        _floatingText = GetComponentInChildren<FloatingTextController>();

        _health = _maxHealth;
        _ap = _maxAP;
        RaiseHealthEvent();
        HideHitChance();

        // Ensure UI gets initial shield state
        if (_team == Team.Friendly)
            ShieldEvents.RaisePlayerShieldChanged(_shield);
        else
        {
            //_enemyHPBar.gameObject.SetActive(false); // commented this out so enemy HP bar show from start
            _enemyShieldBar.gameObject.SetActive(false);
            ShieldEvents.RaiseEnemyShieldChanged(_shield);
        }
    }
    private void Start()
    {
        if (_team != Team.Friendly) return;
        DeckAndHandManager.Instance.OnCardAblityCancel += () => StopTargetingCoro(this);
        TurnManager.Instance.OnTurnEnd += StopTargetingCoro;
    }
    private void OnDestroy()
    {
        if (_team != Team.Friendly) return;
        DeckAndHandManager.Instance.OnCardAblityCancel -= () => StopTargetingCoro(this);
        TurnManager.Instance.OnTurnEnd -= StopTargetingCoro;
    }
    private void StopTargetingCoro(Unit unit)
    {
        if (unit != this || _targetingCoroutine == null) return;

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
                AudioManager.Instance?.PlayShieldHitSFX();

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
                AudioManager.Instance?.PlayDamageTakeSFX(this);
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
                if (TurnManager.GetCurrentUnit == this)
                    TurnManager.Instance.EndEnemyTurn();
                if (WinLossManager.Instance.GetEnemyUnits.Count == 0)
                    GameOverEvents.OnGameWinOrLoss(true);
            }
            //

            ByteMapController.Instance.UpdateUnitPositionByteMap(this, IsoMetricConversions.ConvertToGridFromIsometric(transform.localPosition));
            Destroy(gameObject);
            //Debug.Log($"[{team}] '{name}' unit died");
        }

        //Placeholder enemy healthbar updating
        if (_team == Team.Enemy)
            UpdateEnemyUIBars();

        RaiseHealthEvent();
    }

    /// <summary>
    /// Add shield amount. If duration > 0, shield amount will be removed after duration seconds.
    /// </summary>
    public void AddShield(int amount)
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

        if (_team == Team.Enemy)
            UpdateEnemyUIBars();
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

    //placeholder enemy healthbar stuff
    public void UpdateEnemyUIBars()
    {
        if (_enemyHPBar == null || _enemyShieldBar == null) return;

        if (_enemyHPBar.maxValue != _maxHealth) _enemyHPBar.maxValue = _maxHealth;
        if (_enemyShieldBar.maxValue != _maxShield) _enemyShieldBar.maxValue = _maxShield;

        _enemyHPBar.value = Mathf.Clamp(_health, 0, _maxHealth);
        _enemyShieldBar.value = Mathf.Clamp(_shield, 0, _maxShield);

        //if (_enemyHPBar.value != _enemyHPBar.maxValue && !_enemyHPBar.gameObject.activeInHierarchy)
            //_enemyHPBar.gameObject.SetActive(true);
        //if (_enemyShieldBar.value != _enemyShieldBar.maxValue && !_enemyShieldBar.gameObject.activeInHierarchy)
        _enemyShieldBar.gameObject.SetActive(_enemyShieldBar.value > 0);
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
    public void RestoreAP(int amount)
    {
        if (amount == 0) return;

        _ap = Mathf.Clamp(_ap + amount, 0, _maxAP);
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

    public void ToggleCanMove(bool canMove, bool sendText = true)
    {
        if (_canMove == canMove) return; //avoid any extra texts 

        _canMove = canMove;

        if (!sendText) return;
        _floatingText.SpawnFloatingText(_canMove ? "Freed" : "Rooted", TextPresetType.MissTextPreset);
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

    public void AddHealthDelta(int delta)
    {
        if (delta == 0) return;
        ChangeHealth(Mathf.Abs(delta), isGain: delta > 0);
    }
}