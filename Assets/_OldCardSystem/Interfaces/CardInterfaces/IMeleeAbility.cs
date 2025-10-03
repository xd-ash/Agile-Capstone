using UnityEngine;

namespace CardSystem
{
    public interface IMeleeAbility 
    {
        public GameObject WeaponPrefab { get; }

        public abstract void Hit();
    }
}
