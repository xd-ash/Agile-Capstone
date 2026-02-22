using UnityEngine;
using System.Collections.Generic;
using System;

//Temp Class for easy Win/Loss condition and cyclical gameplay for build
public class WinLossManager : MonoBehaviour
{
    [SerializeField] private float textDuration = 3f;
    private bool _didWin;

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
    }

    private void OnDestroy()
    {
        TurnManager.OnGameStart -= GrabEnemyUnits;
        GameOverEvents.OnGameOver -= OnGameDone;
    }

    public void GrabEnemyUnits()
    {
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
    }
    public void OnGameDone(bool didWin)
    {
        _didWin = didWin;
        CombatNodeCompleted?.Invoke();
        GameUIManager.instance.ToggleWinLossText(_didWin);
        Invoke(nameof(TriggerSceneTrans), textDuration);
    }

    public void TriggerSceneTrans()
    {
        if (_didWin)
        {
            NodeMapManager.Instance.CompleteCurrentNode();
            CombatNodeCompleted?.Invoke();

            if (!NodeMapManager.Instance.GetIsNodeMapComplete)
            {
                NodeMapManager.Instance.ReturnToMap();
                return;
            }
        }

        GameReset?.Invoke();
        SaveLoadScript.CreateNewGame?.Invoke();
        TransitionScene.Instance?.StartTransition();
    }
}