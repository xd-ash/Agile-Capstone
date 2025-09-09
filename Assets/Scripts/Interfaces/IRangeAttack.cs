using UnityEngine;

public interface IRangeAttack
{
    public float Damage { get;}
    public GameObject ProjectilePrefab { get;}
    public float ProjectileSpeed { get;}
    public bool IsRepeating { get;}
    public int NumRepeats { get;}
    public bool IsAOE { get;}
    public CardSOScript.AOETypes AOEType { get;} // change location of enums stuff? 
        
    public void Shoot();
    public void GrabSOData(CardSOScript so);
}
