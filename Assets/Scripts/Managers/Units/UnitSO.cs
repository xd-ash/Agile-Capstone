using UnityEngine;

public enum UnitType { Player, MeleeEnemy, RangeEnemy, PyroEnemy, TankEnemy, MedicEnemy }

[CreateAssetMenu(fileName = "UnitSO", menuName = "UnitSO")]
public class UnitSO : ScriptableObject
{
    [SerializeField] private Team _team = Team.Enemy;
    [SerializeField] private UnitType _unitType = UnitType.MeleeEnemy;
    [SerializeField] private int _maxHealth = 15;
    [SerializeField] private int _maxShield = 25;
    [SerializeField] private int _maxAP = 10;

    public Team GetTeam => _team;
    public UnitType GetUnitType => _unitType;
    public int GetMaxHealth => _maxHealth;
    public int GetMaxShield => _maxShield;
    public int GetMaxAP => _maxAP;
}
