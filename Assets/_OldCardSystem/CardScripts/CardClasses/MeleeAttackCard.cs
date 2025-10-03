using UnityEngine;

namespace OldCardSystem
{
    public class MeleeAttackCard : CardBase, IMeleeAbility, IDealDamage
    {
        public MeleeAttackCard(CardSO so) : base(so)
        {

        }
        //relevant fields for these interface properties are inherited from CardBase
        public GameObject WeaponPrefab { get => _weaponPrefab; }
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
        public void Hit()
        {
            Debug.Log("Melee Attack Hit Triggered.");
        }
        public void DealDamage()
        {
            Debug.Log("Melee Attack Damage Triggered.");
        }
    }
}
