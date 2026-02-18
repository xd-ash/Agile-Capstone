using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using CardSystem;
using AStarPathfinding;

public class TurnManager : MonoBehaviour
{
    public enum Turn { Player, Enemy }
    public Turn CurrTurn { get; private set; } = Turn.Player;

    [Header("Units")] 
    private Unit _curUnit;
    private List<Unit> _unitTurnOrder;
    private int _turnTracker = -1;

    public static event Action OnGameStart;
    public event Action<Unit> OnTurnEnd;
    public event Action<Unit> OnTurnStart;

    //public event Action OnPlayerTurnEnd;
    
    public void EndEnemyTurn() => SetTurn();
    public static bool IsPlayerTurn => Instance != null && Instance.CurrTurn == Turn.Player;
    public static bool IsEnemyTurn => Instance != null && Instance.CurrTurn == Turn.Enemy;
    public static Unit GetCurrentUnit => Instance != null ? Instance._curUnit : null;
    public static List<Unit> GetUnitTurnOrder => Instance != null ? Instance._unitTurnOrder : null;

    public static TurnManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        //if (PlayerDataManager.Instance == null)
           // Instantiate(Resources.Load<GameObject>("SaveDataManager"));
        _unitTurnOrder = GrabUnits();

        OnGameStart?.Invoke();
        SetTurn();
    }

    private List<Unit> GrabUnits()
    {
        var unsortedList = FindObjectsByType<Unit>(sortMode: FindObjectsSortMode.None).ToList<Unit>();
        var sortedList = new List<Unit>();

        for (int i = 0; i < unsortedList.Count; i++)
            if (unsortedList[i].GetTeam == Team.Friendly) //this won't be great if/when there are multiple friendlies
            {
                sortedList.Add(unsortedList[i]);
                unsortedList.Remove(unsortedList[i]);
                break;
            }
        foreach (var unit in unsortedList)
            sortedList.Add(unit);

        return sortedList;
    }

    /*public void UpdateApText(Team unitTeam = Team.Friendly)
    {
        if (_apText == null) return;
        if (currTurn == Turn.Player && _curUnit != null)
            _apText.text = $"Player AP:\n{_curUnit.GetAP}/{_curUnit.GetMaxAP}";
        else if (currTurn == Turn.Enemy && _curUnit != null)
            _apText.text = $"Enemy AP:\n{_curUnit.GetAP}/{_curUnit.GetMaxAP}";
    }*/

    private void SetTurn()
    {
        _curUnit?.transform.Find("turnHighligher").gameObject.SetActive(false);

        _turnTracker++;
        if (_turnTracker >= _unitTurnOrder.Count) _turnTracker = 0; //reset turn tracker for looping turn order
        if (_unitTurnOrder[_turnTracker] == null)
        {
            SetTurn();
            return;
        }
        _curUnit = _unitTurnOrder[_turnTracker];
        CurrTurn = _curUnit.GetTeam == Team.Friendly ? Turn.Player : Turn.Enemy;
        _curUnit.transform.Find("turnHighligher").gameObject.SetActive(true);

        //if (_turnText != null)
        //_turnText.text = $"{currTurn}'s Turn";
        GameUIManager.instance.UpdateTurnText(CurrTurn);
        _curUnit?.RefreshAP();

        if (CurrTurn == Turn.Enemy)
            _curUnit.GetComponent<GoapAgent>().ResetStates();

        // Draw player's starting hand when player's turn begins
        if (_curUnit.GetTeam == Team.Friendly)
            if (!DeckAndHandManager.Instance._startingHandDrawn)
                DeckAndHandManager.Instance?.DrawStartingHand(true);
            else if (DeckAndHandManager.Instance._startingHandDrawn)
                DeckAndHandManager.Instance?.DrawCard(1);

        OnTurnStart?.Invoke(_curUnit);
        GameUIManager.instance.UpdateApText();
    }

    // Mapped to end turn button in combat scene
    public void EndPlayerTurn()
    {
        if (CurrTurn != Turn.Player) return; // avoid turn end spam
        if (_curUnit != null && _curUnit.TryGetComponent(out FindPathAStar aStar) && aStar.GetIsMoving) return; //avoid turn end before movement is complete

        AudioManager.Instance?.PlayButtonSFX();

        SetTurn();
        AbilityEvents.TargetingStopped();

        // discard player's hand at end of player turn
        // DeckAndHandManager.Instance?.DiscardAll();

        //OnPlayerTurnEnd?.Invoke();
        OnTurnEnd?.Invoke(_curUnit);
    }
}