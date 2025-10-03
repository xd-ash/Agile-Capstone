using UnityEngine;

namespace OldCardSystem
{
    public interface IMeleeAbility 
    {
        public GameObject WeaponPrefab { get; }

        public abstract void Hit();
    }
}
