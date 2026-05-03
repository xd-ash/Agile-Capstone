using UnityEngine;

public static class OutlawNameGenerator
{
    private static readonly string[] _firstNames = {
        "John", "Billy", "Jesse", "Frank", "Cole",
        "Wyatt", "Doc", "Butch", "Hank", "Clyde",
        "Dutch", "Virgil", "Amos", "Clay", "Rufus",
        "Earl", "Jeb", "Silas", "Levi", "Gus",
        "Elijah", "Emmett", "Jasper", "Colt", "Wade"
    };

    private static readonly string[] _lastNames = {
        "Walker", "Thorn", "Briggs", "Cassidy", "Dalton",
        "Holliday", "McCoy", "Garrett", "Ringo", "Bonney",
        "Hardin", "Starr", "Bass", "Logan", "Quantrill",
        "Younger", "Earp", "Crockett", "Boone", "Hickok",
        "Barlow", "Graves", "Sterling", "Colton", "Decker"
    };
    
    public static string[] GenerateNames(int count, int seed)
    {
        var names = new string[count];
        for (int i = 0; i < count; i++)
        {
            Random.InitState(seed - i * 7);
            int firstIndex = Random.Range(0, _firstNames.Length);
            int lastIndex = Random.Range(0, _lastNames.Length);
            names[i] = $"{_firstNames[firstIndex]} {_lastNames[lastIndex]}";
        }
        return names;
    }

    public static string GetUnitTypeDisplayName(UnitType unitType, bool isBoss)
    {
        string suffix = isBoss ? "Boss" : "Outlaw";
        switch (unitType)
        {
            case UnitType.MeleeEnemy:  return $"Melee {suffix}";
            case UnitType.RangeEnemy:  return $"Ranged {suffix}";
            case UnitType.PyroEnemy:   return $"Pyro {suffix}";
            case UnitType.TankEnemy:   return $"Tank {suffix}";
            case UnitType.MedicEnemy:  return $"Medic {suffix}";
            default:                   return suffix;
        }
    }
}