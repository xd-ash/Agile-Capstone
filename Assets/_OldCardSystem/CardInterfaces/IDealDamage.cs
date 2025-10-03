using UnityEngine;

namespace OldCardSystem
{
    public interface IDealDamage
    {
        public enum CardDamageTypes
        {
            None,
            Physical,
            Fire,
            Emotional
        }
        public CardDamageTypes[] DamageTypes { get; }
        public int DamageValue { get; }

        public abstract void DealDamage(); //add param for enemy script?
    }
}
