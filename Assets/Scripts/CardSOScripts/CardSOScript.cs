using UnityEngine;

[CreateAssetMenu(fileName = "CardSOScript", menuName = "Deckbuilding System/New Card")]
public class CardSOScript : ScriptableObject
{
    //General Data
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private int _apCost;
    [SerializeField] private float _range;
    // add in fields for any art assets needed
    public enum CardTypes
    {
        None,
        RangeAttack,
        MeleeAttack,
        MiscThrown,
        Utility
    }
    [SerializeField] private CardTypes _cardType;

    public string Name { get => _name; }
    public string Description { get => _description; }
    public int APCost { get => _apCost; }
    public float Range { get => _range; }
    public CardTypes CardType { get => _cardType; }


    //Range Card Data
    [SerializeField] private float _damage;
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private bool _isRepeating;
    [SerializeField] private int _numRepeats;
    [SerializeField] private bool _isAOE;
    public enum AOETypes
    {
        None,
        Cone,
        Line,
        Circle
    }
    [SerializeField] AOETypes _aoeType;
    public float Damage { get => _damage; }
    public GameObject ProjectilePrefab { get => _projectilePrefab; }
    public float ProjectileSpeed { get => _projectileSpeed; }
    public bool IsRepeating { get => _isRepeating; }
    public int NumRepeats { get => _numRepeats; }
    public bool IsAOE { get => _isAOE; }
    public AOETypes AOEType { get => _aoeType; }

    //Melee Card Data

    //Thrown Card Data

    //Utility Card Data
}
