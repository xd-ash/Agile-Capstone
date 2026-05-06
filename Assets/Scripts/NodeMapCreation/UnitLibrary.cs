using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitLibrary", menuName = "Libraries/New Unit Library")]
public partial class UnitLibrary : ScriptableObject
{
    [SerializeField] private UnitDataContainer _playerUnit;

    [SerializeField] private UnitDataContainer[] _normalEnemies;
    [SerializeField] private UnitDataContainer[] _bossEnemies;

    public UnitSO GetPlayerSO => _playerUnit?.unitSO;
    public GameObject GetPlayerPrefab => _playerUnit?.unitPrefab;

    public UnitSO[] GetAllNormalEnemySOs => GetEnemySOs(_normalEnemies);
    public UnitSO[] GetAllBossEnemySOs => GetEnemySOs(_bossEnemies);

    private UnitSO[] GetEnemySOs(UnitDataContainer[] enemiesCollection)
    {
        List<UnitSO> tmp = new();
        foreach (var e in enemiesCollection)
            if (e != null && e.unitSO != null)
                tmp.Add(e.unitSO);
        return tmp.ToArray();
    }

    public GameObject GetPrefabFromUnitSO(UnitSO unitSO)
    {
        if (unitSO == null) return null;

        if (unitSO == _playerUnit.unitSO) return _playerUnit.unitPrefab;

        foreach (var normalEnemy in _normalEnemies)
            if (normalEnemy != null && normalEnemy.unitSO == unitSO)
                return normalEnemy.unitPrefab;

        foreach (var bossEnemy in _bossEnemies)
            if (bossEnemy != null && bossEnemy.unitSO == unitSO)
                return bossEnemy.unitPrefab;

        Debug.Log($"Unit not found in library ({unitSO.name})");
        return null;
    }

    public bool SetEnemyData()
    {
        bool changesMade = false;

        if (_playerUnit != null && _playerUnit.SetDataFromPrefab())
            changesMade = true;

        List<UnitDataContainer> temp = null;
        if (_normalEnemies != null)
         temp = new(_normalEnemies);
        if (_bossEnemies != null)
            temp.AddRange(_bossEnemies);

        if (temp == null) return changesMade;

        for (int i = 0; i < temp.Count; i++)
            if (temp[i] != null && temp[i].SetDataFromPrefab())
                changesMade = true;
        return changesMade;
    }

    public static GameObject GetPlayerUnitPrefab()
    {
        var unitLibrary = Resources.Load<UnitLibrary>("Libraries/UnitLibrary");
        if (unitLibrary == null) return null;
        return unitLibrary.GetPlayerPrefab;
    }
    public static GameObject GetUnitPrefab(UnitSO unitSO)
    {
        if (unitSO == null)
        {
            Debug.Log($"Unit SO null.");
            return null;
        }
        var unitLibrary = Resources.Load<UnitLibrary>("Libraries/UnitLibrary");
        if (unitLibrary == null) return null;
        return unitLibrary.GetPrefabFromUnitSO(unitSO);
    }

    private static int _maxEnemyDuplicates = 1;
    private static int _maxMedicDuplicates = 0;

    public static UnitSO[] GetRandomEnemies(int count, int seed, bool useNormalEnemyPool, UnitType excludedEnemyType = UnitType.Player)
    {
        return GetRandomEnemies(count, seed, useNormalEnemyPool, new UnitType[1] {excludedEnemyType});
    }
    public static UnitSO[] GetRandomEnemies(int count, int seed, bool useNormalEnemyPool, UnitType[] excludedEnemyTypes)
    {
        var unitLibrary = Resources.Load<UnitLibrary>("Libraries/UnitLibrary");
        if (unitLibrary == null) return null;

        List<UnitSO> enemies = useNormalEnemyPool ? new(unitLibrary.GetAllNormalEnemySOs) : new(unitLibrary.GetAllBossEnemySOs);
        if (enemies.Count == 0) return null;

        var tmp = new UnitSO[count];

        for (int i = enemies.Count - 1; i >= 0; i--)
            if (enemies[i] != null && excludedEnemyTypes.Contains(enemies[i].GetUnitType))
                enemies.RemoveAt(i);

        int miscCounter = 0;
        for (int i = 0; i < count; i++)
        {
            UnitSO rngEnemy = null;
            int failCount = 0;
            do
            {
                UnityEngine.Random.InitState(seed - int.Parse($"{i}{miscCounter}"));
                int rng = UnityEngine.Random.Range(0, enemies.Count);
                rngEnemy = enemies[rng];
                miscCounter++;
                failCount++;
            } while (/*(CheckForExcludedEnemyTypes(rngEnemy, excludedEnemyTypes, ref enemies) ||*/ tmp.Length < enemies.Count && !CheckArrayForDuplicates(tmp, rngEnemy) && failCount < 20);
            if (failCount >= 20)
                Debug.LogWarning($"Enemy rng while loop excessive failures.");

            tmp[i] = rngEnemy;
        }
        return tmp;
    }
    private static bool CheckForExcludedEnemyTypes(UnitSO rngUnitSO, UnitType[] excludedEnemyTypes, ref List<UnitSO> collection)
    {
        if (excludedEnemyTypes.Length == Enum.GetValues(typeof(UnitType)).Length)
        {
            Debug.LogWarning($"All unit types were excluded from random enemies grab. Filter not applied.");
            return false;
        }
        if (excludedEnemyTypes.Contains(rngUnitSO.GetUnitType))
        {
            collection.Remove(rngUnitSO);
            return false;
        }
        return true;
    }
    private static bool CheckArrayForDuplicates(UnitSO[] eArray, UnitSO rngUnitSO)
    {
        if (rngUnitSO == null) return false;

        int rngUnitMatchesInArray = 0;
        foreach (var enemy in eArray)
            if (enemy != null && enemy == rngUnitSO)
                rngUnitMatchesInArray++;
        int maxDuplicates = rngUnitSO.GetUnitType == UnitType.MedicEnemy ? _maxMedicDuplicates : _maxEnemyDuplicates;
        return rngUnitMatchesInArray <= maxDuplicates;
    }

    [System.Serializable]
    private class UnitDataContainer
    {
        [HideInInspector] public string unitName = string.Empty;
        public GameObject unitPrefab = null;
        public UnitSO unitSO = null;
        
        public UnitDataContainer(GameObject prefab)
        {
            unitPrefab = prefab;
            SetDataFromPrefab();
        }

        public bool SetDataFromPrefab()
        {
            if (unitPrefab == null && (unitName != string.Empty || unitSO != null))
            {
                unitName = string.Empty;
                unitSO = null;
                return true;
            }
            else if (unitPrefab != null)
            {
                var enemyUnit = unitPrefab?.GetComponent<Unit>();
                if (unitName != unitPrefab.name || unitSO != enemyUnit?.GetUnitSO)
                {
                    unitName = unitPrefab.name;
                    unitSO = enemyUnit?.GetUnitSO;
                    return true;
                }
            }
            return false;
        }
    }
}
