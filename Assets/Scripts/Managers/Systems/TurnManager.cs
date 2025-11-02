using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance { get; private set; }
    public enum Turn { Player, Enemy }
    public Turn currTurn { get; private set; } = Turn.Player;

    [Header("Units")] // might need to switch to a list of friendly/enemy units.
    [SerializeField] private Unit _player;
    [SerializeField] private Unit _enemy;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI _turnText;
    [SerializeField] TextMeshProUGUI _apText;

    [Header("Turn settings")]
    [SerializeField] private int _startingHandSize = 5; // draw this many cards at start of player turn

    public event Action<Turn> OnTurnChanged;

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

        SetTurn(Turn.Player); //Could change by using the dice roll or random.range
    }

    public void UpdateApText()
    {
        if (_apText == null) return;
        if (currTurn == Turn.Player && _player != null)
            _apText.text = $"Player AP:\n{_player.ap}/{_player.maxAP}";
        else if (currTurn == Turn.Enemy && _enemy != null)
            _apText.text = $"Enemy AP:\n{_enemy.ap}/{_enemy.maxAP}";
    }

    private void SetTurn(Turn next)
    {
        currTurn = next;
        Debug.Log($"[TurnManager]" + currTurn + "'s turn");
        if (_turnText != null)
        {
            _turnText.text = $"{currTurn}'s Turn";
        }
        if (currTurn == Turn.Player && _player != null)
        {
            _player.RefreshAP();

            // Draw player's starting hand when player's turn begins
            if (CardSystem.CardManager.instance != null)
            {
                CardSystem.CardManager.instance.DrawStartingHand(_startingHandSize);
            }
        }
        if (currTurn == Turn.Enemy && _enemy != null)
        {
            _enemy.RefreshAP();
        }
        UpdateApText();

        OnTurnChanged?.Invoke(currTurn);

        if (currTurn == Turn.Enemy)
            StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        while (_enemy != null && _enemy.CanSpend(5))
        {
            yield return new WaitForSeconds(2f);
            _enemy.DealDamage(2);
            _enemy.SpendAP(5);
            Debug.Log($"[TurnManager] Enemy Action. Remaining AP: {_enemy.ap}");
            UpdateApText();
        }
        Debug.Log("[TurnManager] Enemy ended turn.");
        EndEnemyTurn();
    }

    public void EndPlayerTurn()
    {
        // discard player's hand at end of player turn
        if (CardSystem.CardManager.instance != null)
            CardSystem.CardManager.instance.DiscardAll();

        SetTurn(Turn.Enemy);
    }
    public void EndEnemyTurn() => SetTurn(Turn.Player);

    public static bool IsPlayerTurn => instance != null && instance.currTurn == Turn.Player;
    public static bool IsEnemyTurn => instance != null && instance.currTurn == Turn.Enemy;
    public static Unit GetCurrentUnit => instance != null && instance.currTurn == Turn.Player ? instance._player : instance._enemy; // Adam added 10/5
                                                                                                                                    //      - Grabbing current unit (for when friendlies added)
                                                                                                                                    //      - unit used for ability stuff
}