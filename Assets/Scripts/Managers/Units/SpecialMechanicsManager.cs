using System.Collections.Generic;
using UnityEngine;

public class SpecialMechanicsManager : MonoBehaviour
{
    Dictionary<Unit, List<bool>> _coinFlipsByUnitThisCombat = new();
    Dictionary<Unit, List<int>> _dieRollsByUnitThisCombat = new();

    public bool GetLastCoinFlipOutcome(Unit unit) => !_coinFlipsByUnitThisCombat.ContainsKey(unit) || 
                                                            _coinFlipsByUnitThisCombat[unit].Count == 0 ? false : _coinFlipsByUnitThisCombat[unit][^1];
    public int GetNumHeadsThisCombat(Unit unit) => GrabNumOfCoinSides(unit, true);
    public int GetNumTailsThisCombat(Unit unit) => GrabNumOfCoinSides(unit, false);
    private int GrabNumOfCoinSides(Unit unit, bool coinSide)
    {
        if (!_coinFlipsByUnitThisCombat.ContainsKey(unit)) return 0;

        int temp = 0;
        foreach (var b in _coinFlipsByUnitThisCombat[unit])
            if (b == coinSide)
                temp++;
        return temp;
    }

    public int GetLastDieOutcome(Unit unit) => !_dieRollsByUnitThisCombat.ContainsKey(unit) || 
                                                            _dieRollsByUnitThisCombat[unit].Count == 0 ? -1 : _dieRollsByUnitThisCombat[unit][^1];
    public static SpecialMechanicsManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CoinFlip.CoinFlipped += AddCoinFlip;
        DiceRoll.DiceRolled += AddDiceRoll;
        WinLossManager.CombatNodeCompleted += ClearCombatCoinFlips;
        WinLossManager.CombatNodeCompleted += ClearCombatDieRolls;
    }
    private void OnDestroy()
    {
        CoinFlip.CoinFlipped -= AddCoinFlip;
        DiceRoll.DiceRolled -= AddDiceRoll;
        WinLossManager.CombatNodeCompleted -= ClearCombatCoinFlips;
        WinLossManager.CombatNodeCompleted -= ClearCombatDieRolls;
    }

    //coin flip management
    private void AddCoinFlip(Unit unit, bool result)
    {
        if (_coinFlipsByUnitThisCombat.ContainsKey(unit))
            _coinFlipsByUnitThisCombat[unit].Add(result);
        else
            _coinFlipsByUnitThisCombat.Add(unit, new() { result });

        if (unit.GetTeam != Team.Friendly) return;

        PlayerDataManager.Instance?.AddCoinFlip(result);
    }
    private void ClearCombatCoinFlips()
    {
        _coinFlipsByUnitThisCombat.Clear();
    }
    public void RemoveUnitCoinFlips(Unit unit)
    {
        _coinFlipsByUnitThisCombat.Remove(unit);
    }

    //Dice roll management
    private void AddDiceRoll(Unit unit, int result)
    {
        if (_dieRollsByUnitThisCombat.ContainsKey(unit))
            _dieRollsByUnitThisCombat[unit].Add(result);
        else
            _dieRollsByUnitThisCombat.Add(unit, new() { result });

        if (unit.GetTeam != Team.Friendly) return;

        PlayerDataManager.Instance?.AddDiceRoll(result);
    }
    private void ClearCombatDieRolls()
    {
        _dieRollsByUnitThisCombat.Clear();
    }
    public void RemoveUnitDieRolls(Unit unit)
    {
        _dieRollsByUnitThisCombat.Remove(unit);
    }
}
