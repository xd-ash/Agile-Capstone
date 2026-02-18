using System.Collections.Generic;
using UnityEngine;

public class SpecialMechanicsManager : MonoBehaviour
{
    Dictionary<Unit, List<bool>> _coinFlipsByUnitThisCombat = new();

    //public int GetNumHeadsThisCombat(Unit unit) { return _coinFlipsByUnitThisCombat[unit].FindAll(x => true).Count; }
    //public int GetNumTailsThisCombat(Unit unit) { return _coinFlipsByUnitThisCombat[unit].FindAll(x => false).Count; }
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
        WinLossManager.CombatNodeCompleted += ClearCombatCoinFlips;
    }
    private void OnDestroy()
    {
        CoinFlip.CoinFlipped -= AddCoinFlip;
        WinLossManager.CombatNodeCompleted -= ClearCombatCoinFlips;
    }
    private void AddCoinFlip(Unit unit, bool result)
    {
        //Debug.Log($"{(result ? "heads" : "tails")}");
        if (_coinFlipsByUnitThisCombat.ContainsKey(unit))
            _coinFlipsByUnitThisCombat[unit].Add(result);
        else
            _coinFlipsByUnitThisCombat.Add(unit, new() { result });

        if (unit.GetTeam != Team.Friendly) return;
        /*if (result)
            Debug.Log($"Player Heads Flips:{GetNumHeadsThisCombat(unit)}");
        else 
            Debug.Log($"Player Tails Flips:{GetNumTailsThisCombat(unit)}");*/

        PlayerDataManager.Instance?.AddCoinflip(result);
    }
    private void ClearCombatCoinFlips()
    {
        _coinFlipsByUnitThisCombat.Clear();
    }
    public void RemoveUnitCoinFlips(Unit unit)
    {
        _coinFlipsByUnitThisCombat.Remove(unit);
    }
}
