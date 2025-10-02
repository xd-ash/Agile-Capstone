using UnityEngine;

namespace OldCardSystem
{
    public interface IRangeAbility
    {
        public GameObject ProjectilePrefab { get; }
        public float ProjectileSpeed { get; }
        public bool IsRepeating { get; }
        public int NumRepeats { get; }
        public int Range {  get; }

        public abstract void Shoot(GameObject proj);
    }
}
