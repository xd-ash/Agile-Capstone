using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using CardSystem;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance { get; private set; }
    public enum Turn { Player, Enemy }
    public Turn currTurn { get; private set; } = Turn.Player;

    [Header("Units")] // might need to switch to a list of friendly/enemy units.
    //[SerializeField] private Unit _player;
    //[SerializeField] private Unit _enemy;
    private Unit _curUnit;
    private List<Unit> _unitTurnOrder;
    private Unit _player;
    private int _turnTracker = -1;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI _turnText;
    [SerializeField] TextMeshProUGUI _apText;

    [Header("Turn settings")]
    [SerializeField] public int _startingHandSize = 5; // draw this many cards at start of player turn

    [Header("Placeholder Enemy Coro stuff")]
    [SerializeField] private AudioClip _enemyDmgSfx;

    public event Action OnGameStart;
    public event Action OnPlayerTurnEnd;
    
    public void EndEnemyTurn() => SetTurn();
    public static bool IsPlayerTurn => instance != null && instance.currTurn == Turn.Player;
    public static bool IsEnemyTurn => instance != null && instance.currTurn == Turn.Enemy;
    public static Unit GetCurrentUnit => instance != null ? instance._curUnit : null;
    public static List<Unit> GetUnitTurnOrder => instance != null ? instance._unitTurnOrder : null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        AbilityEvents.OnAbilityUsed += UpdateApText;

        _unitTurnOrder = GrabUnits();

        //Temp setup for win/loss conditions
        //
        List<Unit> enemies = new List<Unit>();
        foreach (Unit unit in _unitTurnOrder)
            if (unit != null && unit.team == Team.Enemy)
                enemies.Add(unit);
        WinLossManager.instance.enemyUnits = enemies;
        //

        OnGameStart?.Invoke();
        SetTurn(); //Could change by using the dice roll or random.range
    }

    //There is very likely a better way to sort this 
    private List<Unit> GrabUnits()
    {
        var unsortedList = FindObjectsByType<Unit>(sortMode: FindObjectsSortMode.None).ToList<Unit>();
        var sortedList = new List<Unit>();

        for (int i = 0; i < unsortedList.Count; i++)
            //this isnt great for when we have multiple friendlies
            if (unsortedList[i].team == Team.Friendly)
            {
                _player = unsortedList[i];
                sortedList.Add(unsortedList[i]);
                unsortedList.Remove(unsortedList[i]);
                break;
            }
        foreach (var unit in unsortedList)
            sortedList.Add(unit);

        return sortedList;
    }

    public void UpdateApText(Team unitTeam = Team.Friendly)
    {
        if (_apText == null) return;
        if (currTurn == Turn.Player && _curUnit != null)
            _apText.text = $"Player AP:\n{_curUnit.ap}/{_curUnit.maxAP}";
        else if (currTurn == Turn.Enemy && _curUnit != null)
            _apText.text = $"Enemy AP:\n{_curUnit.ap}/{_curUnit.maxAP}";
    }

    private void SetTurn()
    {
        if (_curUnit != null)
            _curUnit.transform.Find("turnHighligher").gameObject.SetActive(false);

        _turnTracker++;
        if (_turnTracker >= _unitTurnOrder.Count) _turnTracker = 0; //reset turn tracker for looping turn order
        if (_unitTurnOrder[_turnTracker] == null)
        {
            SetTurn();
            return;
        }
        _curUnit = _unitTurnOrder[_turnTracker];
        currTurn = _curUnit.team == Team.Friendly ? Turn.Player : Turn.Enemy;
        _curUnit.transform.Find("turnHighligher").gameObject.SetActive(true);

        //Debug.Log($"[TurnManager]" + currTurn + "'s turn");
        if (_turnText != null)
            _turnText.text = $"{currTurn}'s Turn";
        if (_curUnit != null)
            _curUnit.RefreshAP();

        if (currTurn == Turn.Enemy)
            _curUnit.GetComponent<GoapAgent>().ResetStates();

        // Draw player's starting hand when player's turn begins
        if (_curUnit.team == Team.Friendly && CardManager.instance != null)
            Debug.Log("[TurnManager] Drawing starting hand.");
            CardManager.instance.DrawStartingHand(_startingHandSize);

        UpdateApText();

        //if (currTurn == Turn.Enemy)
            //_curUnit.StartCoroutine(EnemyTurn());
    }

    /*
    private IEnumerator EnemyTurn()
    {
        while (_curUnit != null && _curUnit.CanSpend(5))
        {
            yield return new WaitForSeconds(1f);
            _curUnit.DealDamage(_player, 1);
            _curUnit.SpendAP(5);
            AudioManager.instance?.PlaySFX(_enemyDmgSfx);
            //Debug.Log($"[TurnManager] Enemy Action. Remaining AP: {_curUnit.ap}");
            UpdateApText();
        }
        //Debug.Log("[TurnManager] Enemy ended turn.");
        OnUnitTurnResetStuff?.Invoke(_curUnit);
        EndEnemyTurn();
    }
    */

    public void EndPlayerTurn()
    {
        if (currTurn != Turn.Player) return; // avoid turn end spam

        SetTurn();
        AbilityEvents.TargetingStopped();
        OnPlayerTurnEnd?.Invoke();

        // discard player's hand at end of player turn
        if (CardSystem.CardManager.instance != null)
            CardSystem.CardManager.instance.DiscardAll();
    }
}