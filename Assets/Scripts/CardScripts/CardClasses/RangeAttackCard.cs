using UnityEngine;

namespace CardSystem
{
    public class RangeAttackCard : CardBase, IRangeAbility, IDealDamage
    {
        public RangeAttackCard(CardSO so) : base(so)
        {

        }
        //relevant fields for these interface properties are inherited from CardBase
        public GameObject ProjectilePrefab { get => _projectilePrefab; }
        public float ProjectileSpeed { get => _projectileSpeed; }
        public bool IsRepeating { get => _isRepeating; }
        public int NumRepeats { get => _numRepeats; }
        public int Range { get => _range; }

        public IDealDamage.CardDamageTypes[] DamageTypes { get => _damageTypes; }
        public int DamageValue { get => _damageValue; }

        public override void Use()
        {
            //
        }
        public override void Discard()
        {
            //
        }
        public void Shoot(GameObject proj)
        {
            Debug.Log("Range Attack Shoot Triggered.");
        }
        public void DealDamage()
        {
            Debug.Log("Range Attack Damage Triggered.");
        }
    }
}
