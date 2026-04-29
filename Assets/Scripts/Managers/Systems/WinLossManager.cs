using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using CardSystem;

//Temp Class for easy Win/Loss condition and cyclical gameplay for build
public class WinLossManager : MonoBehaviour
{
    private RewardsDisplayScript _rewardsPanel;

    [SerializeField] private float textDuration = 3f;
    private bool _didWin;
    public bool IsGameComplete { get; private set; }

    [SerializeField] private List<Unit> _enemyUnits;
    public List<Unit> GetEnemyUnits => _enemyUnits;

    public static Action CombatNodeCompleted;
    public static Action GameReset;

    public static WinLossManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        TurnManager.OnGameStart += GrabEnemyUnits;

        GameOverEvents.OnGameOver += OnGameDone;

        _rewardsPanel = FindAnyObjectByType<RewardsDisplayScript>(FindObjectsInactive.Include);
        _rewardsPanel.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        TurnManager.OnGameStart -= GrabEnemyUnits;
        GameOverEvents.OnGameOver -= OnGameDone;
    }

    public void GrabEnemyUnits()
    {
        IsGameComplete = false;

        List<Unit> enemies = new();
        foreach (Unit unit in TurnManager.GetUnitTurnOrder)
            if (unit != null && unit.GetTeam == Team.Enemy)
                enemies.Add(unit);
        _enemyUnits = enemies;
    }
    public void RemoveEnemyFromPlay(Unit unit)
    {
        if (unit.GetTeam == Team.Friendly) return;

        if (!_enemyUnits.Contains(unit)) return;

        _enemyUnits.Remove(unit);
        SpecialMechanicsManager.Instance.RemoveUnitCoinFlips(unit);
        SpecialMechanicsManager.Instance.RemoveUnitDieRolls(unit);
    }

    public void OnGameDone(bool didWin)
    {
        IsGameComplete = true;
        _didWin = didWin;
        CombatNodeCompleted?.Invoke();
        GameUIManager.instance.ToggleWinLossText(_didWin);
        DeckAndHandManager.Instance?.ToggleCollidersOnHover(null, true);
        Invoke(nameof(TriggerSceneTrans), textDuration);
    }

    public void TriggerSceneTrans()
    {
        if (_didWin)
        {
            _rewardsPanel.gameObject.SetActive(true);
            return;
        }

        GameReset?.Invoke();
        SaveLoadScript.CreateNewGame?.Invoke();
        TransitionScene.Instance?.StartTransition();
    }
}