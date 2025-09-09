using UnityEngine;

public class RangeAttackCard : IRangeAttack
{
    private float _damage;
    private GameObject _projectilePrefab;
    private float _projectileSpeed;
    private bool _isRepeating;
    private int _numRepeats;
    private bool _isAOE;
    private CardSOScript.AOETypes _aoeType;


    public float Damage { get => _damage; private set => _damage = value; }
    public GameObject ProjectilePrefab { get => _projectilePrefab; private set => _projectilePrefab = value; }
    public float ProjectileSpeed { get => _projectileSpeed; private set => _projectileSpeed = value; }
    public bool IsRepeating { get => _isRepeating; private set => _isRepeating = value; }
    public int NumRepeats { get => _numRepeats; private set => _numRepeats = value; }
    public bool IsAOE { get => _isAOE; private set => _isAOE = value; }
    public CardSOScript.AOETypes AOEType { get => _aoeType; private set => _aoeType = value; }

    public void GrabSOData(CardSOScript so)
    {
        
        Damage = so.Damage;
        ProjectilePrefab = so.ProjectilePrefab;
        ProjectileSpeed = so.ProjectileSpeed;
        IsRepeating = so.IsRepeating;
        NumRepeats = so.NumRepeats;
        IsAOE = so.IsAOE;
        AOEType = so.AOEType;
    }

    public void Shoot()
    {
        Debug.Log("I am shooting a thing");
        //instantiate obj @ projspeed
        //once proj reches target, if aoe do aoe stuff
        //if repeating, repeat 
    }
}
