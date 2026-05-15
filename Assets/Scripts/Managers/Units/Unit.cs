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
    [SerializeField, HideInInspector] private UnitSO _unitSO;

    [Header("Team and stats")] 
    [Space(10), SerializeField, HideInInspector] private Team _team;
    [SerializeField, HideInInspector] private int _maxHealth = 0;
    [SerializeField, HideInInspector] private int _health = 0;

    [Header("Shield")]
    [SerializeField, HideInInspector] private int _maxShield = 0;
    [SerializeField, HideInInspector] private int _shield = 0;

    [Header("Action Points")]
    [SerializeField, HideInInspector] private int _maxAP = 0;
    [SerializeField, HideInInspector] private int _ap;

    [Header("Placeholder Stuff")]
    [SerializeField] private Slider _enemyHPBar;
    [SerializeField] private Slider _enemyShieldBar;
    [SerializeField] private TextMeshProUGUI _hitChanceText;

    [Header("Misc")]
    [SerializeField] private bool _canMove = true;

    private FloatingTextController _floatingText;
    private HitVFXSpawner _hitVFXSpawner;    
    private Coroutine _targetingCoroutine;
    private Guid _unitGuid = new();

    public UnitSO GetUnitSO => _unitSO;
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
    public HitVFXSpawner GetHitVFXSpawner => _hitVFXSpawner;

    public bool GetCanMove => _canMove;
    public bool GetIsMoving => TryGetComponent(out UnitMovementController unitMover) && unitMover.GetIsMoving;
    public Guid GetGuid => _unitGuid;

    public bool IsDead { get; private set; }

    public event Action<Unit> OnApChanged;

    private void Awake()
    {
        _floatingText = GetComponentInChildren<FloatingTextController>();
        _hitVFXSpawner = GetComponent<HitVFXSpawner>(); 

        GrabSOData();

        RaiseHealthEvent();
        HideHitChance();

        if (_team == Team.Friendly)
        {
            ShieldEvents.RaisePlayerShieldChanged(_shield);
        }
        else
        {
            _enemyShieldBar.gameObject.SetActive(false);
            ShieldEvents.RaiseEnemyShieldChanged(_shield);
        }
    }

    private void GrabSOData()
    {
        _maxHealth = _unitSO.GetMaxHealth;
        _maxShield = _unitSO.GetMaxShield;
        _maxAP = _unitSO.GetMaxAP;
        _team = _unitSO.GetTeam;

        //GrabRunBuffs();

        _health = (_team == Team.Friendly && !TransitionScene.IsTutorial) ? PlayerDataManager.Instance.GetCurrentHealth : _maxHealth;
        _shield = 0;
        _ap = _maxAP;
    }
    private void UpdatePlayerHealth(bool didWin)
    {
        if (!didWin || _health <= 0) return;
        PlayerDataManager.Instance.UpdateHealthForRun(_health);
    }
    /*private void GrabRunBuffs()
    {
        var pdm = PlayerDataManager.Instance;
        if (pdm == null) return;
        _maxHealth += pdm.GetMaxHealthBuff;
        _maxAP += pdm.GetMaxAPBuff;
    }
    public void IncreaseStat(RestOptions stat)
    {
        switch (stat)
        {
            case RestOptions.AP:
                break;
            case RestOptions.MaxHealth:
                break;
        }
    }*/
    private void Start()
    {
        if (_team != Team.Friendly) return;
        DeckAndHandManager.Instance.OnCardAblityCancel += () => StopTargetingCoro(this);
        TurnManager.Instance.OnTurnEnd += StopTargetingCoro;
        GameOverEvents.OnGameOver += UpdatePlayerHealth;

        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.GetHasRegenBuff && SpecialMechanicsManager.Instance != null)
        {
            ChangeHealth(SpecialMechanicsManager.Instance.GetHealthRegenVal, true);
            _floatingText?.SpawnFloatingText($"{SpecialMechanicsManager.Instance.GetHealthRegenVal}", TextPresetType.HealPreset);
        }
    }
    private void OnDestroy()
    {
        if (_team != Team.Friendly) return;
        DeckAndHandManager.Instance.OnCardAblityCancel -= () => StopTargetingCoro(this);
        TurnManager.Instance.OnTurnEnd -= StopTargetingCoro;
        GameOverEvents.OnGameOver -= UpdatePlayerHealth;
    }
    private void StopTargetingCoro(Unit unit)
    {
        if (unit != this || _targetingCoroutine == null) return;

        StopCoroutine(_targetingCoroutine);
    }
    
    public void PlayFlinchAnim(Vector2Int attackerGridPos)
    {
        if (IsDead) return;

        var dirAnimator = GetComponent<DirectionAnimator>();
        if (dirAnimator == null) return;

        Vector2Int myPos = IsoMetricConversions.ConvertToGridFromIsometric(transform.localPosition);
        Vector2Int delta = attackerGridPos - myPos;
        int dir = DirectionAnimator.GetDirIndexFromDelta(delta);

        dirAnimator.PlayAttack(AttackAnimKey.TakeDamage, dir, null);
    }
    
    public void ChangeHealth(int amount, bool isGain)
    {
        int uAmount = Math.Abs(amount);

        if (!isGain)
        {
            int remainingDamage = uAmount;

            if (_shield > 0)
            {
                AudioManager.Instance?.PlayShieldHitSFX();

                int absorbed = Mathf.Min(_shield, remainingDamage);
                _shield -= absorbed;
                remainingDamage -= absorbed;

                if (_team == Team.Friendly)
                    ShieldEvents.RaisePlayerShieldChanged(_shield);
                else
                    ShieldEvents.RaiseEnemyShieldChanged(_shield);
            }

            if (remainingDamage > 0)
            {
                _health -= remainingDamage;
                AudioManager.Instance?.PlayDamageTakeSFX(this);
            }
        }
        else
        {
            _health += uAmount;
        }

        if (_health >= _maxHealth)
            _health = _maxHealth;
        else if (_health <= 0)
        {
            _health = 0;

            if (_team == Team.Friendly)
                GameOverEvents.OnGameWinOrLoss(false);
            else
            {
                WinLossManager.Instance.EnemyUnits.Remove(this);
                if (TurnManager.GetCurrentUnit == this)
                    TurnManager.Instance.EndEnemyTurn();
                if (WinLossManager.Instance.EnemyUnits.Count <= 0 && !WinLossManager.Instance.IsGameComplete)
                    GameOverEvents.OnGameWinOrLoss(true);
            }

            ByteMapController.Instance.UpdateUnitPositionByteMap(this, IsoMetricConversions.ConvertToGridFromIsometric(transform.localPosition));

            //mark dead, disable collider, play death anim before destroying
            IsDead = true;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            PlayDeathAndDestroy();
        }

        if (_team == Team.Enemy)
            UpdateEnemyUIBars();

        RaiseHealthEvent();
    }
    private void PlayDeathAndDestroy()
    {
        var dirAnimator = GetComponent<DirectionAnimator>();
        if (dirAnimator == null)
        {
            Destroy(gameObject);
            return;
        }

        dirAnimator.PlayAttack(AttackAnimKey.Die, dirAnimator.GetLastDir, () =>
        {
            Destroy(gameObject);
        });
    }

    public void AddShield(int amount)
    {
        if (amount <= 0) return;
        _shield += amount;
        if (_shield >= _maxShield)
            _shield = _maxShield;

        if (_team == Team.Friendly)
            ShieldEvents.RaisePlayerShieldChanged(_shield);
        else
            ShieldEvents.RaiseEnemyShieldChanged(_shield);

        if (_team == Team.Enemy)
            UpdateEnemyUIBars();
    }

    public void RemoveShield(int amount)
    {
        if (amount <= 0) return;
        int removed = Mathf.Min(_shield, amount);
        _shield -= removed;

        if (_team == Team.Friendly)
            ShieldEvents.RaisePlayerShieldChanged(_shield);
        else
            ShieldEvents.RaiseEnemyShieldChanged(_shield);
    }

    public void UpdateEnemyUIBars()
    {
        if (_enemyHPBar == null || _enemyShieldBar == null) return;

        if (_enemyHPBar.maxValue != _maxHealth) _enemyHPBar.maxValue = _maxHealth;
        if (_enemyShieldBar.maxValue != _maxShield) _enemyShieldBar.maxValue = _maxShield;

        _enemyHPBar.value = Mathf.Clamp(_health, 0, _maxHealth);
        _enemyShieldBar.value = Mathf.Clamp(_shield, 0, _maxShield);

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
        if (_canMove == canMove) return;

        _canMove = canMove;
        MovementRangeCalculator.Instance.RebuildForCurrentUnit();

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