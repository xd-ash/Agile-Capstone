using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using CardSystem;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance { get; private set; }
    public enum Turn { Player, Enemy }
    public Turn currTurn { get; private set; } = Turn.Player;

    [Header("Units")] 
    private Unit _curUnit;
    private List<Unit> _unitTurnOrder;
    private int _turnTracker = -1;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _apText;

    [Header("Turn settings")]

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

    public void UpdateApText(Team unitTeam = Team.Friendly)
    {
        if (_apText == null) return;
        if (currTurn == Turn.Player && _curUnit != null)
            _apText.text = $"Player AP:\n{_curUnit.GetAP}/{_curUnit.GetMaxAP}";
        else if (currTurn == Turn.Enemy && _curUnit != null)
            _apText.text = $"Enemy AP:\n{_curUnit.GetAP}/{_curUnit.GetMaxAP}";
    }

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
        currTurn = _curUnit.GetTeam == Team.Friendly ? Turn.Player : Turn.Enemy;
        _curUnit.transform.Find("turnHighligher").gameObject.SetActive(true);

        if (_turnText != null)
            _turnText.text = $"{currTurn}'s Turn";
        _curUnit?.RefreshAP();

        if (currTurn == Turn.Enemy)
            _curUnit.GetComponent<GoapAgent>().ResetStates();

        // Draw player's starting hand when player's turn begins
        if (_curUnit.GetTeam == Team.Friendly)
            DeckAndHandManager.instance?.DrawStartingHand(true);

        UpdateApText();
    }

    // Mapped to end turn button in combat scene
    public void EndPlayerTurn()
    {
        if (currTurn != Turn.Player) return; // avoid turn end spam
        AudioManager.instance?.PlayButtonSFX();

        SetTurn();
        AbilityEvents.TargetingStopped();

        // discard player's hand at end of player turn
        DeckAndHandManager.instance?.DiscardAll();
        OnPlayerTurnEnd?.Invoke();
    }
}